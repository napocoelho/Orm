using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using CoreDll.LinqExpressions;

namespace CoreDll.Bindables
{
    public abstract class BindableBaseOld : INotifyPropertyChanged, INotifyPropertyChanging, IResumeNotifications
    {
        // Weak Events:
        private readonly WeakPropertyChangedSource PropertyChangedSource = new WeakPropertyChangedSource();
        private readonly WeakPropertyChangingSource PropertyChangingSource = new WeakPropertyChangingSource();

        //Propriedades:
        protected Dictionary<string, object> PropertyStore { get; private set; } = new Dictionary<string, object>(StringComparer.Ordinal);
        protected Dictionary<string, string[]> AlsoNotifyProperties { get; private set; } = new Dictionary<string, string[]>(StringComparer.Ordinal);

        protected HashSet<string> StartedLazyProperties { get; private set; } = new HashSet<string>(StringComparer.Ordinal);
        //private Dictionary<string, Predicate<object>> Constraints { get; set; } = new Dictionary<string, Predicate<object>>(StringComparer.Ordinal);
        protected bool IsChanged { get; set; }  // utilizado (magicamente) pelo [Fody.PropertyChanged]

        // Event Locks
        private HashSet<string> DeferedNotifications { get; set; } = new HashSet<string>();
        private bool IsDeferedNotification { get => _deferedReferenceCount > 0; }
        private uint _deferedReferenceCount = 0;
        private bool IsLazyLoadLocked { get; set; }




        #region Eventos;

        //public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => PropertyChangedSource.Subscribe(value);
            remove => PropertyChangedSource.Unsubscribe(value);
        }


        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (!IsDeferedNotification)
            {
                PropertyChangedSource.Raise(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                DeferedNotifications.Add(propertyName);
            }

            //this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public event PropertyChangingEventHandler PropertyChanging;


        public event PropertyChangingEventHandler PropertyChanging
        {
            add => PropertyChangingSource.Subscribe(value);
            remove => PropertyChangingSource.Unsubscribe(value);
        }


        public virtual void OnPropertyChanging([CallerMemberName] string propertyName = "")
        {
            if (!IsDeferedNotification)
            {
                PropertyChangingSource.Raise(this, new PropertyChangingEventArgs(propertyName));
            }

            //this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        public void OnAllPropertiesChanging()
        {
            Type type = GetType();
            System.Reflection.PropertyInfo[] propertiesInfo = type.GetProperties(System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);

            foreach (System.Reflection.PropertyInfo info in propertiesInfo)
            {
                OnPropertyChanging(info.Name);
            }
        }

        //[PropertyChanged.SuppressPropertyChangedWarnings]
        public void OnAllPropertiesChanged()
        {
            Type type = GetType();
            System.Reflection.PropertyInfo[] propertiesInfo = type.GetProperties(System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance);

            foreach (System.Reflection.PropertyInfo info in propertiesInfo)
            {
                OnPropertyChanged(info.Name);
            }
        }

        #endregion Eventos;



        public BindableBaseOld()
        {
            //this.PropertyStore = new Dictionary<string, object>(StringComparer.Ordinal);    // --> comparação Ordinal é a mais rápida (case-sensitive);
            //this.StartedLazyProperties = new HashSet<string>(StringComparer.Ordinal);       // --> comparação Ordinal é a mais rápida (case-sensitive);



            foreach (System.Reflection.PropertyInfo info in GetType().GetProperties())
            {
                AlsoNotifyForAttribute attr = info.GetCustomAttributes(typeof(AlsoNotifyForAttribute), true).FirstOrDefault() as AlsoNotifyForAttribute;

                if (attr != null)
                {
                    AlsoNotifyProperties.Add(info.Name, attr.PropertyNames);
                }
            }

            IsChanged = false; // deve ficar sempre no final do construtor
        }



        /*
        /// <summary>
        /// Has one with lazy load;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T GetHasOne<T>(int dependentId, [CallerMemberName]string propertyName = "")
        {

        }
        */

        /// <summary>
        /// Obtém um valor da propriedade alvo. Devido a mais recente utilização do [Fody.PropertyChanged], este recurso se tornou obsoleto.
        /// </summary>
        /// <typeparam name="T">Tipo de valor da propriedade</typeparam>
        /// <param name="propertyName">Nome da propriedade. Por padrão, deixar sem especificar</param>
        /// <returns>Retorna o valor da propriedade alvo.</returns>
        //[Obsolete]
        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            return Get<T>(() => default(T), propertyName);
        }

        /// <summary>
        /// Obtém um valor da propriedade alvo. Devido a mais recente utilização do [Fody.PropertyChanged], este recurso se tornou obsoleto.
        /// </summary>
        /// <typeparam name="T">Tipo de valor da propriedade</typeparam>
        /// <param name="propertyName">Nome da propriedade. Por padrão, deixar sem especificar</param>
        /// <returns>Retorna o valor da propriedade alvo.</returns>
        protected T Get<T>(Func<T> initializerExpression, [CallerMemberName] string propertyName = null)
        {
            if (propertyName is null)
                throw new ArgumentNullException(nameof(propertyName));

            object oldObjectValue = null;
            T returnValue;

            if (!PropertyStore.TryGetValue(propertyName, out oldObjectValue))
            {
                returnValue = initializerExpression();

                OnPropertyChanging(propertyName);
                PropertyStore[propertyName] = returnValue;
                OnPropertyChanged(propertyName);
            }
            else
            {
                returnValue = (T)oldObjectValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Atribui um valor à propriedade alvo apenas se passar no teste da constraint.
        /// O valor inicial da propriedade não será validado, portanto, inicialize o valor no construtor da classe em questão.
        /// </summary>
        /// <typeparam name="T">Tipo de valor da propriedade</typeparam>
        /// <param name="newValue">Valor a ser atribuido na propriedade</param>
        /// <param name="propertyName">Nome da propriedade. Por padrão, deixar sem especificar</param>
        /// <param name="constraintPredicate">Verifica se o valor de entrada é válido</param>
        /// <returns>Retorna True se a propriedade for alterada, False caso contrário. Caso o valor seja igual o existente, a propriedade não será alterada.</returns>
        protected bool SetWithConstraint<T>(T newValue, Predicate<T> constraintPredicate, [CallerMemberName] string propertyName = null)
        {
            if (constraintPredicate != null && constraintPredicate(newValue))
            {
                return Set<T>(newValue, propertyName);
            }

            return false;
        }



        /// <summary>
        /// Atribui um valor à propriedade alvo. Devido a mais recente utilização do [Fody.PropertyChanged], este recurso se tornou obsoleto.
        /// </summary>
        /// <typeparam name="T">Tipo de valor da propriedade</typeparam>
        /// <param name="newValue">Valor a ser atribuido na propriedade</param>
        /// <param name="propertyName">Nome da propriedade. Por padrão, deixar sem especificar</param>
        /// <returns>Retorna True se a propriedade for alterada, False caso contrário. Caso o valor seja igual o existente, a propriedade não será alterada.</returns>
        //[Obsolete]
        protected bool Set<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            bool shouldModify = false;
            T oldValue = default(T);
            Type type = typeof(T);

            object oldObjectValue = null;
            object newObjectValue = newValue;

            try
            {
                // Obtendo valor antigo:
                if (!PropertyStore.TryGetValue(propertyName, out oldObjectValue))
                {
                    //this.PropertyStore[propertyName] = oldValue;
                    shouldModify = true;
                }
                else
                {
                    oldValue = (T)oldObjectValue;

                    // Comparando:
                    if (oldObjectValue == null && newObjectValue != null)
                    {
                        shouldModify = true;
                    }
                    else if (oldObjectValue != null && newObjectValue == null)
                    {
                        shouldModify = true;
                    }
                    else if (type.IsPrimitive)
                    {
                        shouldModify = (oldObjectValue != newObjectValue);
                    }
                    else
                    {
                        shouldModify = !object.ReferenceEquals(oldValue, newValue);
                    }
                }


                // Alterando valor, se necessário:
                if (shouldModify)
                {
                    OnPropertyChanging(propertyName);
                    PropertyStore[propertyName] = newValue;
                    IsChanged = true;
                    OnPropertyChanged(propertyName, oldObjectValue, newObjectValue);

                    AlsoNotify(propertyName);
                }

            }
            catch
            {
                shouldModify = true;
                OnPropertyChanging(propertyName);
                InitializeProperty(propertyName);
                OnPropertyChanged(propertyName, oldObjectValue, newObjectValue);

                AlsoNotify(propertyName);
            }

            return shouldModify;
        }

        private void AlsoNotify(string propertyName)
        {
            string[] alsoNotifyArray;
            if (AlsoNotifyProperties.TryGetValue(propertyName, out alsoNotifyArray))
            {
                foreach (string alsoNotify in alsoNotifyArray)
                {
                    OnPropertyChanged(propertyName);
                }
            }
        }

        private void InitializeProperty(string propertyName)
        {
            if (!PropertyStore.ContainsKey(propertyName))
            {
                Type propertyType = GetType().GetProperty(propertyName).PropertyType;

                OnPropertyChanging(propertyName);

                if (propertyType.IsValueType)
                {
                    PropertyStore[propertyName] = Activator.CreateInstance(propertyType);
                }
                else
                {
                    PropertyStore[propertyName] = null;
                }

                OnPropertyChanged(propertyName);
            }
        }

        /*
        [Obsolete("Utilizar o método [Set<T>()] e [Get<T>()] ao invés deste.")]
        protected bool SetValue(object newValue, [CallerMemberName]string propertyName = "")
        {
            bool shouldModify = false;

            string memberName = TranslateAttributeName(propertyName);
            Type type = this.GetType();
            System.Reflection.FieldInfo fieldInfo = type.GetField(memberName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);


            object oldValue = fieldInfo.GetValue(this);
            shouldModify = oldValue != newValue;

            if (shouldModify)
            {
                OnPropertyChanging(propertyName);
                fieldInfo.SetValue(this, newValue);
                this.IsDirty = true;
                OnPropertyChanged(propertyName);
            }

            return shouldModify;
        }
        */

        private string TranslateAttributeName(string propertyName)
        {
            return "_" + propertyName;
        }



        /// <summary>
        /// Desempenha um acesso tardio (Lazy Load) à propriedade alvo. É extremamente indicado para quando o valor da propriedade alvo necessitar de acesso adicional ao banco de dados.
        /// Esta técnica evita que vários relacionamentos sejam carregados antes de serem chamados, economizando chamadas ao banco de dados e evitando procedimentos não necessários.
        /// O procedimento especificado em [lazyCommand] será executado apenas na primeira vez em que houver uma chamada à propriedade alvo. 
        /// </summary>
        /// <param name="lazyCommand">Um closure a ser executado internamente. O closure será executado apenas no primeiro acesso à propriedade alvo</param>
        /// <param name="propertyName">Nome da propriedade alvo</param>
        /// <returns>Retorna um resultado do tipo especificado T</returns>
        //[System.Diagnostics.DebuggerHidden] //this makes the debugger stop in the calling method instead of here.
        //[System.Diagnostics.Conditional("DEBUG")]
        protected T GetLazy<T>(Func<T> lazyCommand, [CallerMemberName] string propertyName = null)
        {
            // Proteção contra acesso precoce (as vezes alguma ferramenta (modo debug do Visual Studio, Fody.PropertyChanged, dentre outros) pode acessar métodos LazyLoad antes da hora):
            if (IsDeferedNotification || IsLazyLoadLocked)
                return default(T);

            object oldValue = null;
            T returnValue = default(T);


            if (!PropertyStore.TryGetValue(propertyName, out oldValue))
            {
                returnValue = lazyCommand != null ? lazyCommand() : returnValue;

                StartedLazyProperties.Add(propertyName);

                OnPropertyChanging();
                PropertyStore[propertyName] = returnValue;
                OnPropertyChanged();
            }
            else
            {
                returnValue = (T)oldValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Somente reseta as que já foram inicializadas.
        /// </summary>
        protected void ResetLazyProperties()
        {
            string[] lazyProperties = StartedLazyProperties.ToArray();
            StartedLazyProperties.Clear();

            foreach (string propertyName in lazyProperties)
            {
                OnPropertyChanging(propertyName);
                PropertyStore.Remove(propertyName);
                OnPropertyChanged(propertyName);
            }
        }

        protected void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> expression)
        {
            string propertyName = ExprenssionHelper.MemberName(expression);
            OnPropertyChanged(propertyName);

        }

        protected void OnPropertyChanged<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
            where TSource : BindableBase
        {
            string propertyName = ExprenssionHelper.MemberName(expression);
            OnPropertyChanged(propertyName);
        }



        /// <summary>
        /// Método especial do "Fody.PropertyChanged".
        /// </summary>
        protected void OnPropertyChanged(string propertyName, object before, object after)
        {
            OnPropertyChanged(propertyName);
        }


        /// <summary>
        /// Prorroga o acionamento dos eventos até que ResumeOnDispose tenha retornado.
        /// * Dica: utilize-o num bloco [Using()] para bloquear eventos.
        /// </summary>
        /// <returns></returns>
        protected IDisposable DeferNotifications()
        {
            ++_deferedReferenceCount;
            return new ResumeOnDispose<BindableBaseOld>(this);
        }

        void IResumeNotifications.ResumeNotifications()
        {
            --_deferedReferenceCount;

            if (!IsDeferedNotification)
            {
                try
                {
                    IsLazyLoadLocked = true;

                    foreach (string propertyName in DeferedNotifications)
                    {
                        OnPropertyChanged(propertyName);
                    }
                    //this.DeferedNotifications.ForEach(action => action());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    DeferedNotifications.Clear();  //--> ******MUITO IMPORTANTE******
                    IsLazyLoadLocked = false;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CoreDll.Extensions;

namespace CoreDll.Bindables
{
    public abstract class BindableBase : INotifyPropertyChanged, IResumeNotifications
    {
        //Propriedades:
        private Dictionary<string, PropertyValue> PropertyStore { get; set; } = new Dictionary<string, PropertyValue>(StringComparer.Ordinal);

        protected bool IsChanged { get; set; }  // utilizado (magicamente) pelo [Fody.PropertyChanged]

        // Event Locks
        private HashSet<string> DeferedNotifications { get; set; } = new HashSet<string>();
        private bool IsDeferedNotification { get => _deferedReferenceCount > 0; }
        private uint _deferedReferenceCount = 0;
        private bool IsLazyLoadLocked { get; set; }






        public BindableBase()
        {
            //this.PropertyStore = new Dictionary<string, object>(StringComparer.Ordinal);    // --> comparação Ordinal é a mais rápida (case-sensitive);
            //this.StartedLazyProperties = new HashSet<string>(StringComparer.Ordinal);       // --> comparação Ordinal é a mais rápida (case-sensitive);

            //foreach (PropertyInfo info in this.GetType().GetProperties())
            //{
            //    AlsoNotifyForAttribute attr = info.GetCustomAttributes(typeof(AlsoNotifyForAttribute), true).FirstOrDefault() as AlsoNotifyForAttribute;

            //    if (attr != null)
            //    {
            //        AlsoNotifyProperties.Add(info.Name, attr.PropertyNames);
            //    }
            //}



            IsChanged = false; // deve ficar sempre no final do construtor
        }


        public static PropertyValue GetPropertyValue(BindableBase entity, string propertyName)
        {
            PropertyValue propertyValue = null;
            entity?.PropertyStore?.TryGetValue(propertyName, out propertyValue);
            return propertyValue;
        }

        public static PropertyValue[] GetPropertyValues(BindableBase entity)
        {
            return entity?.PropertyStore?.Values.ToArray();
        }

        private PropertyValue GeneratePropertyStoreItem(Type type, string propertyName)
        {
            MethodInfo method = GetType().GetMethods().Where(x => x.Name == "GeneratePropertyStoreItem" && x.IsGenericMethod).FirstOrDefault();
            MethodInfo genericMethod = method.MakeGenericMethod(type);

            object newValue = type.IsValueType ? Activator.CreateInstance(type) : null;

            return genericMethod.Invoke(this, new object[] { propertyName, newValue }) as PropertyValue;
        }


        private PropertyValue GeneratePropertyStoreItem<T>(string propertyName, T newValue = default(T))
        {
            AlsoNotifyForAttribute attr = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetCustomAttributes(typeof(AlsoNotifyForAttribute), true).FirstOrDefault() as AlsoNotifyForAttribute;
            string[] alsoNotifyFor = attr?.PropertyNames ?? Enumerable.Empty<string>().ToArray();



            PropertyValue propertyValue = PropertyValue.Load<T>(propertyName, newValue, alsoNotifyFor, null, null);


            MethodInfo onSingleMethod = GetType().GetMethod($"On{propertyName}Changed");

            if (onSingleMethod != null && !onSingleMethod.GetParameters().Any() && onSingleMethod.ReturnParameter.ParameterType.FullName == "System.Void")
            {
                propertyValue.CallOnNamedChanged = () => onSingleMethod.Invoke(this, null);
            }


            MethodInfo onChangedMethod = null;

            try
            {
                onChangedMethod = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                .Where(item => item.Name == "OnPropertyChanged"
                                                                && item.ReturnParameter.ParameterType.FullName == "System.Void"
                                                                && item.GetParameters().Length == 3
                                                                && item.GetParameters().First().ParameterType == typeof(PropertyValue)          // primeiro parâmetro
                                                                && item.GetParameters().Skip(1).First().ParameterType == typeof(object)  // segundo parâmetro
                                                                && item.GetParameters().Last().ParameterType == typeof(object)           // terceiro parâmetro
                                                ).FirstOrDefault();
            }
            catch (Exception ex)
            {
                string mensagem = ex.Message;
            }

            if (onChangedMethod != null)
            {
                //propertyValue.CallOnPropertyChanged = (PropertyValue propertyValue, object oldValue, object newValue) => onChangedMethod.Invoke(this, new object[] { propertyName, oldValue, newValue });
                propertyValue.CallOnPropertyChanged = (PropertyValue propValue, object oldVal, object newVal) => onChangedMethod.Invoke(this, new object[] { propValue, oldVal, newVal });
            }

            if (PropertyStore.ContainsKey(propertyName))
            {
                PropertyStore[propertyName] = propertyValue;
            }
            else
            {
                PropertyStore.Add(propertyName, propertyValue);
            }


            return propertyValue;
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
        //protected T Get<T>([CallerMemberName]string propertyName = null)
        //{
        //    return Get<T>(() => default(T), propertyName);
        //}

        /// <summary>
        /// Obtém um valor da propriedade alvo. Devido a mais recente utilização do [Fody.PropertyChanged], este recurso se tornou obsoleto.
        /// </summary>
        /// <typeparam name="T">Tipo de valor da propriedade</typeparam>
        /// <param name="propertyName">Nome da propriedade. Por padrão, deixar sem especificar</param>
        /// <returns>Retorna o valor da propriedade alvo.</returns>
        //protected T Get<T>(Func<T> initializerExpression, [CallerMemberName]string propertyName = null)
        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            PropertyValue propertyValue = null;

            if (PropertyStore.TryGetValue(propertyName, out propertyValue))
            {
                return (T)propertyValue.Value;
            }
            else
            {
                propertyValue = GeneratePropertyStoreItem<T>(propertyName);
                return (T)propertyValue.Value;
            }
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
            PropertyValue propertyValue = null;

            try
            {
                // Obtendo valor antigo:
                if (!PropertyStore.TryGetValue(propertyName, out propertyValue))
                {
                    propertyValue = GeneratePropertyStoreItem<T>(propertyName, newValue);
                    IsChanged = true;
                    OnInternalPropertyChanged(propertyValue, oldValue, newValue);
                    shouldModify = true;
                }
                else
                {
                    oldValue = (T)propertyValue.Value;

                    // Comparando o valor antigo com o novo:
                    shouldModify = !(
                                        object.ReferenceEquals(oldValue, newValue)
                                        || (propertyValue.Value is null && newValue is null)
                                        || (/*propertyValue.ValueType.IsPrimitive &&*/ oldValue.Equals(newValue))
                                    );

                    if (shouldModify)
                    {
                        propertyValue.Value = newValue;
                        IsChanged = true;
                        OnInternalPropertyChanged(propertyValue, oldValue, newValue);
                        return true;
                    }
                }
            }
            catch
            {
                InitializeProperty<T>(propertyName);
                propertyValue.Value = newValue;
                IsChanged = true;
                OnInternalPropertyChanged(propertyValue, oldValue, newValue);

                shouldModify = true;
            }

            return shouldModify;
        }

        protected void Observes<TAnotherEntity>(string observerPropertyName, TAnotherEntity observableInstance, Expression<Func<TAnotherEntity, object>> observableProperty) where TAnotherEntity : INotifyPropertyChanged
        {
            string observablePropertyName = CoreDll.LinqExpressions.ExprenssionHelper.MemberName(observableProperty);
            //Notifier<TAnotherEntity> notifier = new Notifier<TAnotherEntity>(observableInstance, observablePropertyName);
            //System.Action action = () => OnPropertyChanged(observerPropertyName);

            INotifyPropertyChanged observable = observableInstance;
            observable.PropertyChanged += (object sender, PropertyChangedEventArgs e) => OnInternalPropertyChanged(observerPropertyName);
        }


        private void AlsoNotify(PropertyValue propertyValue)
        {
            //Queue<PropertyValue> queueAntiInfinity = new Queue<PropertyValue>();

            foreach (string alsoNotify in propertyValue.AlsoNotifyProperties)
            {
                PropertyValue anotherPropertyValue = null;

                if (PropertyStore.TryGetValue(alsoNotify, out anotherPropertyValue))
                {
                    OnTopPropertyChanged(anotherPropertyValue.Name);
                    anotherPropertyValue.CallOnNamedChanged?.Invoke();
                }
                else
                {
                    OnTopPropertyChanged(alsoNotify);
                }
            }
        }

        private void InitializeProperty<T>(string propertyName)
        {
            PropertyValue propertyValue = null;

            if (!PropertyStore.TryGetValue(propertyName, out propertyValue))
            {
                Type propertyType = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).PropertyType;

                propertyValue = GeneratePropertyStoreItem<T>(propertyName);

                if (propertyType.IsValueType)
                {
                    propertyValue.Value = Activator.CreateInstance(propertyType);
                }
                else
                {
                    propertyValue.Value = null;
                }

                //this.OnPropertyChanged(propertyName);
                //AlsoNotify(propertyValue);
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

            //object oldValue = null;
            //T returnValue = default(T);

            PropertyValue propertyValue = null;

            if (PropertyStore.TryGetValue(propertyName, out propertyValue))    // se já foi instanciado
            {
                if (!propertyValue.IsLazyLoadStarted)
                {
                    propertyValue.Value = lazyCommand is null ? default(T) : lazyCommand();
                    propertyValue.IsLazyLoadStarted = true;
                }

                return (T)propertyValue.Value;
            }
            else // se ainda não foi instanciado
            {
                propertyValue = GeneratePropertyStoreItem<T>(propertyName);
                propertyValue.Value = lazyCommand is null ? default(T) : lazyCommand();
                propertyValue.IsLazyLoadStarted = true;

                return (T)propertyValue.Value;
            }
        }






        /// <summary>
        /// Prorroga o acionamento dos eventos até que ResumeOnDispose tenha retornado.
        /// * Dica: utilize-o num bloco [Using()] para bloquear eventos.
        /// </summary>
        /// <returns></returns>
        protected IDisposable DeferNotifications()
        {
            ++_deferedReferenceCount;
            return new ResumeOnDispose<BindableBase>(this);
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
                        OnInternalPropertyChanged(propertyName);
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





        #region Eventos;

        //protected void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> expression)
        //{
        //    string propertyName = ExprenssionHelper.MemberName(expression);
        //    this.OnPropertyChanged(propertyName);

        //}

        //protected void OnPropertyChanged<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
        //    where TSource : BindableBase
        //{
        //    string propertyName = ExprenssionHelper.MemberName(expression);
        //    this.OnPropertyChanged(propertyName);
        //}

        /// <summary>
        /// Método especial do "Fody.PropertyChanged".
        /// </summary>
        protected void OnInternalPropertyChanged(PropertyValue propertyValue, object before, object after)
        {
            if (propertyValue != null)
            {
                if (propertyValue.CallOnPropertyChanged != null)
                {
                    propertyValue.CallOnPropertyChanged.Invoke(propertyValue, before, after);
                }
                else
                {
                    OnInternalPropertyChanged(propertyValue);
                }
            }
        }

        protected void ResumeOnPropertyChanged(PropertyValue propertyValue)
        {
            OnInternalPropertyChanged(propertyValue);
        }

        public void OnInternalPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (!IsDeferedNotification)
            {
                PropertyValue propertyValue = null;

                if (PropertyStore.TryGetValue(propertyName, out propertyValue))
                {
                    OnInternalPropertyChanged(propertyValue);
                }
                else
                {
                    Type propertyType = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.PropertyType;
                    propertyValue = GeneratePropertyStoreItem(propertyType, propertyName);
                    OnInternalPropertyChanged(propertyValue);
                }
            }
            else
            {
                DeferedNotifications.Add(propertyName);
            }
        }

        protected void OnInternalPropertyChanged(PropertyValue propertyValue)
        {
            if (!IsDeferedNotification)
            {
                OnTopPropertyChanged(propertyValue.Name);
                propertyValue.CallOnNamedChanged?.Invoke();
                AlsoNotify(propertyValue);
            }
            else
            {
                DeferedNotifications.Add(propertyValue.Name);
            }
        }



        //[PropertyChanged.SuppressPropertyChangedWarnings]
        public void OnAllPropertiesChanged()
        {
            Type type = GetType();
            PropertyInfo[] propertiesInfo = type.GetProperties(BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo info in propertiesInfo)
            {
                OnInternalPropertyChanged(info.Name);
            }
        }

        #endregion Eventos;





        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnTopPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedSource.Raise(this, new PropertyChangedEventArgs(propertyName));
        }

        /*
          private void OnTopPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChangedSource.Raise(this, new PropertyChangedEventArgs(propertyName));
        }
         */

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => PropertyChangedSource.Subscribe(value);
            remove => PropertyChangedSource.Unsubscribe(value);
        }

        // Weak Events:
        private readonly WeakPropertyChangedSource PropertyChangedSource = new WeakPropertyChangedSource();

    }
}
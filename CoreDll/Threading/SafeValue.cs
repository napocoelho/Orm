using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDll.Threading
{
    public class SafeValue<T> : INotifyPropertyChanged
    {
        private readonly object LOCK = new object();
        public event PropertyChangedEventHandler PropertyChanged;
        private T _value;

        public T Value
        {
            get { lock (LOCK) { return _value; } }
            //O SETTER tem que ser privado! Há fortes motivos de "difícil" compreensão para os incautos. Está avisado!
            private set { lock (LOCK) { if (!_value.Equals(value)) { _value = value; OnValue(); } } }
        }


        public SafeValue() { }
        public SafeValue(T value)
        {
            _value = value;
        }



        private void OnValue()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="toSetValue"></param>
        public void SetValue(Func<T, T> toSetValue)
        {
            if (toSetValue is null)
                throw new ArgumentNullException(nameof(toSetValue));

            lock (LOCK)
            {
                Value = toSetValue(Value);
            }
        }

        public void SetValue(Func<T> toSetValue)
        {
            if (toSetValue is null)
                throw new ArgumentNullException(nameof(toSetValue));

            lock (LOCK)
            {
                Value = toSetValue();
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            SafeValue<T> another = obj as SafeValue<T>;
            return another is not null && this.Value.Equals(another);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}

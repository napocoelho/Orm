using System;

namespace CoreDll.Bindables
{
    public class NoEventBindingTrigger : AbstractBindingTrigger
    {
        //public event EventHandler ValueChanged = null;

        public NoEventBindingTrigger(object source, string propertyName)
            : base()
        {
            if (source is null)
                throw new ArgumentNullException("source");

            Source = source;
            Type sourceType = source.GetType();
            EventInfo = null;
            PropertyInfo = sourceType.GetProperty(propertyName);
        }
    }
}
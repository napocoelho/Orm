using System;
using System.Reflection;
using System.ComponentModel;

namespace CoreDll.Bindables
{
    public class PropertyChangedEventHandlerBindingTrigger<TPropertyChanged> : AbstractBindingTrigger
        where TPropertyChanged : System.ComponentModel.INotifyPropertyChanged
    {
        private Delegate DelegateRef { get; set; }

        public PropertyChangedEventHandlerBindingTrigger(TPropertyChanged source, string propertyName)
            : base()
        {
            string eventName = "PropertyChanged";

            Source = source;
            Type sourceType = typeof(TPropertyChanged); // source.GetType();
            EventInfo = sourceType.GetEvent(eventName);
            //Type eventType = this.EventInfo.EventHandlerType;
            PropertyInfo = sourceType.GetProperty(propertyName);

            if (EventInfo.EventHandlerType != typeof(PropertyChangedEventHandler))
            {
                throw new NotSupportedException("O evento não é do tipo PropertyChangedEventHandler!");
            }



            MethodInfo methodInfo = GetType().GetMethod("ActionPerformer");
            //Delegate delegateRef = Delegate.CreateDelegate(eventType, methodInfo);
            DelegateRef = Delegate.CreateDelegate(EventInfo.EventHandlerType, this, methodInfo);



            EventInfo.AddEventHandler(Source, DelegateRef);
            //this.EventInfo.RemoveEventHandler ()
        }

        public void ActionPerformer(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PropertyInfo.Name)
            {
                //this.Value = (T)this.PropertyInfo.GetValue(this.Source);
                Value = PropertyInfo.GetValue(Source);
                OnPerformedAction();
            }
        }
    }
}
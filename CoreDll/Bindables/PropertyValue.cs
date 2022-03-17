using System;
using System.Linq;

namespace CoreDll.Bindables
{
    public class PropertyValue
    {
        private object _value;

        public string Name { get; set; }
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (IsValueStarted) { IsValueChanged = true; }
            }
        }

        public Type ValueType { get; set; }
        public bool IsValueStarted { get; set; }
        public bool IsValueChanged { get; set; }

        public string[] AlsoNotifyProperties { get; set; }
        public bool IsLazyLoadStarted { get; set; }

        public Action CallOnNamedChanged { get; set; }
        public Action<PropertyValue, object, object> CallOnPropertyChanged { get; set; }

        private PropertyValue()
        { }

        public static PropertyValue Load<TValue>(string propertyName, string[] alsoNotifyProperties)
        {
            return PropertyValue.Load<TValue>(propertyName, default(TValue), alsoNotifyProperties, null, null);
        }

        public static PropertyValue Load<TValue>(string propertyName, TValue value, string[] alsoNotifyProperties, Action callOnNamedChanged, Action<PropertyValue, object, object> callOnPropertyChanged)
        {
            PropertyValue property = new PropertyValue();
            property.Name = propertyName;
            property.Value = value;
            property.IsValueStarted = true;
            property.ValueType = typeof(TValue);

            property.AlsoNotifyProperties = alsoNotifyProperties ?? Enumerable.Empty<string>().ToArray();

            property.CallOnNamedChanged = callOnNamedChanged;
            property.CallOnPropertyChanged = callOnPropertyChanged;

            return property;
        }

    }
}
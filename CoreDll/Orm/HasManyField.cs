using System;
using System.Reflection;

namespace CoreDll.Orm
{

    public class HasManyField
    {
        /// <summary>
        /// Field info
        /// </summary>
        public PropertyInfo Info { get; set; }

        public string OwnedIdFieldName { get; set; }
        public Type OwnedType { get; set; }

        public object GetValue(object entityInstance)
        {
            return Info.GetValue(entityInstance);
        }

        public void SetValue(object entityInstance, object value)
        {
            Info.SetValue(entityInstance, value);
        }
    }
}
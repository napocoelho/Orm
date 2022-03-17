using System;
using System.Reflection;

namespace CoreDll.Orm
{
    public class EntityField
    {
        /// <summary>
        /// Field info
        /// </summary>
        public PropertyInfo Info { get; set; }

        /// <summary>
        /// The column name in the table
        /// </summary>
        public string Column { get; set; }

        public TypeCode TypeCode { get; set; }

        public bool IsNullable { get; set; }
        public bool IsValueType { get; set; }
        public bool IsReferenceType { get; set; }

        /// <summary>
        /// Needs single quotation marks
        /// </summary>
        public bool IsQuoted { get; set; }

        public bool IsId { get; set; }
        public bool IsKey { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsEnum { get; set; }


        public bool IsSigned { get; set; }
        public bool IsSignature { get; set; }
        public bool IsViolated { get; set; }


        public Func<object, string> ToSqlConversion { get; set; }
        //public Func<string, object> FromSqlConversion { get; set; }


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
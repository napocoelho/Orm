using System;

namespace CoreDll.Orm.Attributes
{
    /// <summary>
    /// A related entitie's table name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class Table : Attribute
    {
        public string Name { get; set; }

        public Table(string tableName)
        {
            Name = tableName;
        }
    }

    /// <summary>
    /// A related propertie's column name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Column : Attribute
    {
        public string Name { get; set; }

        public Column(string name = null)
        {
            Name = name;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class Default : Attribute
    //{
    //    public string SqlValue { get; set; }

    //    public Default(string sqlValue = null)
    //    {
    //        this.SqlValue = sqlValue;
    //    }
    //}

    /// <summary>
    /// A property that never should be mapped
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Ignore : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotNull : Attribute
    { }

    /// <summary>
    /// A non composite primary key and not nullable field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Key : Attribute
    { }

    /// <summary>
    /// Specifies a field with "Key, ReadOnly and NotNull" attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Id : Attribute
    { }

    /// <summary>
    /// A readonly database column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ReadOnly : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HasMany : Attribute
    {
        public string OwnedIdFieldName { get; set; }
        //public Type Type { get; set; }

        //public HasMany(string ownedIdFieldName, Type type)
        public HasMany(string ownedIdFieldName)
        {
            OwnedIdFieldName = ownedIdFieldName;
            //this.Type = type;
        }
    }



    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Signature : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Signed : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsViolated : Attribute
    { }
}
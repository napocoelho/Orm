using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CoreDll.Extensions;
using CoreDll.Orm.Attributes;

namespace CoreDll.Orm
{
    public class Schema
    {
        private Dictionary<Type, EntityRegistry> PropertyList { get; set; }

        public Schema()
        {
            PropertyList = new Dictionary<Type, EntityRegistry>();
        }

        public EntityRegistry GetOrSetRegistry<T>()
        {
            Type type = typeof(T);
            return GetOrSetRegistry(type);
        }

        public EntityRegistry GetOrSetRegistry(Type type)
        {
            if (!ContainsRegistry(type))
            {
                SetRegistry(type);
            }

            EntityRegistry registry = GetRegistry(type);

            if (registry == null)
                throw new Exception("The object is not a valid [Entity] type!");

            return registry;
        }

        public bool ContainsRegistry<T>()
        {
            return ContainsRegistry(typeof(T));
        }

        public bool ContainsRegistry(Type type)
        {
            return PropertyList.ContainsKey(type);
        }

        public EntityRegistry GetRegistry<T>()
        {
            return GetRegistry(typeof(T));
        }

        public EntityRegistry GetRegistry(Type type)
        {
            EntityRegistry registry = null;
            PropertyList.TryGetValue(type, out registry);
            return registry;
        }

        public void SetRegistry<T>()
        {
            SetRegistry(typeof(T));
        }

        public bool SetRegistry(Type type)
        {
            if (PropertyList == null)
            {
                PropertyList = new Dictionary<Type, EntityRegistry>();
            }

            Table tableAtt = type.GetCustomAttribute<Table>();

            if (tableAtt == null)
                throw new Exception("The object type is not a valid Entity. It does not implements the [Entity] attribute!");

            EntityRegistry registry = new EntityRegistry(type, tableAtt.Name);
            List<EntityField> fields = new List<EntityField>();
            List<EntityField> ignoredFields = new List<EntityField>();
            List<HasManyField> hasManyfields = new List<HasManyField>();

            foreach (PropertyInfo property in type.GetRuntimeProperties())
            {
                PrimaryType propertyType = new PrimaryType(property.PropertyType);
                bool isPublic = property.GetSetMethod() != null;

                if (property.GetCustomAttribute<HasMany>() != null)
                {
                    HasMany hasManyAttr = property.GetCustomAttribute<HasMany>();
                    Type ownedType = property.PropertyType.GetGenericArguments()[0];
                    //ownedType = hasManyAttr.Type;
                    hasManyfields.Add(new HasManyField() { Info = property, OwnedType = ownedType, OwnedIdFieldName = hasManyAttr.OwnedIdFieldName });
                }
                else if (isPublic && property.CanRead && property.CanWrite && property.GetCustomAttribute<Ignore>() == null && property.GetCustomAttribute<IsViolated>() == null)
                {
                    if (TypeHelper.IsPermitedType(propertyType))
                    {
                        EntityField field = new EntityField();

                        field.IsSignature = property.GetCustomAttribute<Signature>() != null;
                        field.IsSigned = property.GetCustomAttribute<Signed>() != null;

                        field.Info = property;
                        field.Column = property.GetCustomAttribute<Column>() == null ? property.Name : property.GetCustomAttribute<Column>().Name;
                        field.TypeCode = TypeHelper.GetTypeCodeOf(propertyType);
                        field.IsQuoted = TypeHelper.IsSqlQuoted(propertyType);

                        field.IsId = property.GetCustomAttribute<Id>() != null;
                        field.IsKey = field.IsId || property.GetCustomAttribute<Key>() != null;

                        field.IsEnum = propertyType.IsEnum;
                        field.IsValueType = propertyType.IsValueType;
                        field.IsReferenceType = !propertyType.IsValueType;
                        field.IsReadOnly = field.IsId || property.GetCustomAttribute<ReadOnly>() != null;


                        if (field.IsSignature)
                        {
                            field.IsNullable = true;
                        }
                        else if (field.IsId || field.IsKey || field.IsSigned || property.GetCustomAttribute<NotNull>().NotIsNull())
                        {
                            field.IsNullable = false;
                        }
                        else
                        {
                            field.IsNullable = propertyType.IsNullable;
                        }

                        if (field.IsId &&
                            (field.TypeCode != TypeCode.Int32 && field.TypeCode != TypeCode.Int64
                            && field.TypeCode != TypeCode.UInt32 && field.TypeCode != TypeCode.UInt64))
                        {
                            throw new Exception("An [Id] property must have an Integer type!");
                        }

                        if (field.IsKey && propertyType.IsValueType && !propertyType.IsNullable)
                        {
                            throw new Exception("An [Id] or [Key] with primitive property type must be Nullable<>! *(not the database column, just the property)");
                        }



                        field.ToSqlConversion = TypeHelper.ConvertionToSqlFunc(propertyType, field.IsNullable);
                        //field.FromSqlConversion = TypeHelper.ConvertionFromSqlFunc(propertyType, field.IsNullable);

                        fields.Add(field);
                    }
                }
                else if (property.GetCustomAttribute<Ignore>() != null || property.GetCustomAttribute<IsViolated>() != null)
                {
                    EntityField field = new EntityField();

                    field.Info = property;
                    field.IsViolated = property.GetCustomAttribute<IsViolated>() != null;
                    ignoredFields.Add(field);
                }
            }

            registry.SetFields(fields.ToArray());
            registry.SetIgnoredFields(ignoredFields.ToArray());
            registry.SetHasManyFields(hasManyfields.ToArray());

            if (registry.Fields.Where(x => x.IsKey).Count() > 1)
                throw new Exception("An [Entity] cannot contains more than one [Key] or [Id] attribute!");


            if (!registry.Fields.Where(x => x.IsKey).Any())
                throw new Exception("An [Entity] must contains a [Key] or an [Id] attribute!");


            #region ------ Campos de assinatura ------
            if (registry.Fields.Count(x => x.IsSignature) > 1)
                throw new Exception("Only one [Signature] attribute is permited in a signed class!");

            if (registry.Fields.Any(x => x.IsSignature) && registry.Fields.First(x => x.IsSignature).Info.PropertyType != typeof(string))
                throw new Exception("A [IsSignature] property must to be a String type!");

            if (registry.Fields.Any(x => x.IsSignature) && registry.IgnoredFields.Count(x => x.IsViolated) != 1)
                throw new Exception("Must have one (and only one) [IsViolated] attribute in a signed class!");

            if (registry.Fields.Any(x => x.IsSignature) && registry.IgnoredFields.First(x => x.IsViolated).Info.PropertyType != typeof(bool))
                throw new Exception("A [IsViolated] property must to be a Boolean type!");

            if (registry.Fields.Any(x => x.IsSignature) && !registry.Fields.Any(x => x.IsSigned))
                throw new Exception("A class signed with [Signature] must to have one or more [Signed] properties!");
            #endregion ------ Campos de assinatura ------


            if (PropertyList.ContainsKey(type))
            {
                PropertyList[type] = registry;
            }
            else
            {
                PropertyList.Add(type, registry);
            }

            return true;
        }
    }
}
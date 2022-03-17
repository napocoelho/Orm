using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CoreDll.Extensions;

namespace CoreDll.Orm
{


    public class EntityRegistry
    {
        public string TableName { get; set; }
        public EntityField KeyField { get; private set; }
        public bool IsSigned { get; private set; }
        public EntityField[] Fields { get; private set; }
        public EntityField[] IgnoredFields { get; private set; }

        public HasManyField[] HasManyFields { get; private set; }

        public Type EntityType { get; private set; }

        private string CachedUpdateFields { get; set; }
        private string CachedInsertFields { get; set; }
        private string CachedSelectFields { get; set; }






        public EntityRegistry(Type entityType, string tableName)
        {
            EntityType = entityType;
            TableName = tableName;
            Fields = new List<EntityField>().ToArray();
            IgnoredFields = new List<EntityField>().ToArray();
            HasManyFields = new List<HasManyField>().ToArray();

            CachedUpdateFields = null;
            CachedInsertFields = null;
            CachedSelectFields = null;
        }

        public void SetFields(EntityField[] fields)
        {
            Fields = fields;
            KeyField = fields.Where(x => x.IsKey).First();
            IsSigned = fields.Any(x => x.IsSignature);
            UpdateCachedFields();
        }

        public void SetIgnoredFields(EntityField[] fields)
        {
            IgnoredFields = fields;
        }

        public void SetHasManyFields(HasManyField[] fields)
        {
            HasManyFields = fields;
        }

        private StringBuilder BuildUpdateSql(string[] selectProperties = null)
        {
            StringBuilder builder = new StringBuilder();
            bool adicionarVirgula = false;
            int idx = 0;

            builder.Append("update ").Append(TableName).Append(" set ");

            IEnumerable<EntityField> fields = selectProperties is null ?
                                                    Fields.Where(x => !x.IsReadOnly)
                                                    : Fields.Where(x => !x.IsReadOnly && x.Info.Name.ExistsIn(selectProperties));

            foreach (EntityField field in fields)
            {
                if (adicionarVirgula)
                    builder.Append(", ");

                builder.Append(field.Column).Append(" = {").Append(idx.ToString()).Append("}");
                adicionarVirgula = true;
                idx++;
            }

            builder.Append(" where ").Append(Fields.Where(x => x.IsKey).First().Column).Append(" = ").Append("{").Append(idx).Append("}");

            return builder;
        }

        private StringBuilder BuildInsertSql(string[] selectProperties = null)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            bool adicionarVirgula = false;
            int idx = 0;

            builder.Append("insert into ").Append(TableName).Append("(");

            IEnumerable<EntityField> fields = selectProperties is null ?
                                                    Fields.Where(x => !x.IsReadOnly)
                                                    : Fields.Where(x => !x.IsReadOnly && x.Info.Name.ExistsIn(selectProperties));

            foreach (EntityField field in fields)
            {
                if (adicionarVirgula)
                {
                    builder.Append(", ");
                    builder2.Append(", ");
                }

                builder.Append(field.Column);
                builder2.Append("{").Append(idx).Append("}");
                adicionarVirgula = true;
                idx++;
            }

            builder.Append(") values (");
            builder.Append(builder2);
            builder.Append(")");

            //builder.Append(" where ").Append(Fields.Where(x => x.IsId).First().Column).Append(" = ").Append("{").Append(idx).Append("}");
            return builder;
        }

        private void UpdateCachedFields()
        {
            bool adicionarVirgula = false;
            StringBuilder builder = null;
            int idx = 0;

            // Update: --------------------------------------------------------------------------------------------------------------------------------
            //builder = new StringBuilder();
            //adicionarVirgula = false;
            //idx = 0;

            //builder.Append("update ").Append(this.TableName).Append(" set ");

            //foreach (EntityField field in this.Fields.Where(x => !x.IsReadOnly))
            //{
            //    if (adicionarVirgula)
            //        builder.Append(", ");

            //    builder.Append(field.Column).Append(" = {").Append(idx.ToString()).Append("}");
            //    adicionarVirgula = true;
            //    idx++;
            //}

            //builder.Append(" where ").Append(Fields.Where(x => x.IsKey).First().Column).Append(" = ").Append("{").Append(idx).Append("}");
            //this.CachedUpdateFields = builder.ToString();

            CachedUpdateFields = BuildUpdateSql().ToString();


            // Insert: --------------------------------------------------------------------------------------------------------------------------------
            //builder = new StringBuilder();
            //StringBuilder builder2 = new StringBuilder();
            //adicionarVirgula = false;
            //idx = 0;

            //builder.Append("insert into ").Append(this.TableName).Append("(");

            //foreach (EntityField field in this.Fields.Where(x => !x.IsReadOnly))
            //{
            //    if (adicionarVirgula)
            //    {
            //        builder.Append(", ");
            //        builder2.Append(", ");
            //    }

            //    builder.Append(field.Column);
            //    builder2.Append("{").Append(idx).Append("}");
            //    adicionarVirgula = true;
            //    idx++;
            //}

            //builder.Append(") values (");
            //builder.Append(builder2);
            //builder.Append(")");

            ////builder.Append(" where ").Append(Fields.Where(x => x.IsId).First().Column).Append(" = ").Append("{").Append(idx).Append("}");
            //this.CachedInsertFields = builder.ToString();

            CachedInsertFields = BuildInsertSql().ToString();

            // Select: --------------------------------------------------------------------------------------------------------------------------------
            builder = new StringBuilder();
            adicionarVirgula = false;
            idx = 0;

            builder.Append("select ");

            foreach (EntityField field in Fields)
            {
                if (adicionarVirgula)
                    builder.Append(", ");

                builder.Append(field.Column);
                adicionarVirgula = true;
                idx++;
            }

            builder.Append(" from ").Append(TableName);
            //builder.Append(" where ").Append(Fields.Where(x => x.IsId).First().Column).Append(" in (").Append("{").Append(idx).Append("}").Append(")");
            CachedSelectFields = builder.ToString();
        }

        public string GetUpdateSql(object entityInstance, string[] selectProperties = null)
        {
            List<string> values = new List<string>();

            IEnumerable<EntityField> fields = selectProperties is null ?
                                                    Fields.Where(x => !x.IsReadOnly)
                                                    : Fields.Where(x => !x.IsReadOnly && x.Info.Name.ExistsIn(selectProperties));


            for (int idx = 0; idx < fields.Count(); idx++)
            {
                EntityField field = fields.ElementAt(idx);
                object value = field.Info.GetValue(entityInstance);
                values.Add(field.ToSqlConversion(value));
            }

            object keyValue = KeyField.Info.GetValue(entityInstance);
            values.Add(KeyField.ToSqlConversion(keyValue));

            string sql = selectProperties is null ?
                                    string.Format(CachedUpdateFields, values.ToArray())
                                    : string.Format(BuildUpdateSql(selectProperties).ToString(), values.ToArray());

            return sql;
        }

        public string GetInsertSql(object entityInstance, string[] selectProperties = null)
        {
            List<string> values = new List<string>();

            IEnumerable<EntityField> fields = selectProperties is null ?
                                                        Fields.Where(x => !x.IsReadOnly)
                                                        : Fields.Where(x => !x.IsReadOnly && x.Info.Name.ExistsIn(selectProperties));

            for (int idx = 0; idx < fields.Count(); idx++)
            {
                EntityField field = fields.ElementAt(idx);
                object value = field.Info.GetValue(entityInstance);
                //values.Add(this.ConvertValueToSql(field, value));
                values.Add(field.ToSqlConversion(value));
            }

            string sql = selectProperties is null ?
                                    string.Format(CachedInsertFields, values.ToArray())
                                    : string.Format(BuildInsertSql(selectProperties).ToString(), values.ToArray());

            return sql;
        }


        public string GetSelectSql(params object[] keyValueArgs)
        {
            string sql = null;

            if (keyValueArgs != null && keyValueArgs.Count() > 0)
            {
                List<string> keyValues = new List<string>();
                StringBuilder builder = new StringBuilder(CachedSelectFields);

                for (int idx = 0; idx < keyValueArgs.Count(); idx++)
                {
                    object keyValue = keyValueArgs[idx];
                    //object keyValue = this.KeyField.Info.GetValue(entityInstance);
                    //keyValues.Add(this.ConvertValueToSql(this.KeyField, keyValue));
                    keyValues.Add(KeyField.ToSqlConversion(keyValue));
                }

                builder.Append(" where ").Append(KeyField.Column).Append(" in (").Append(keyValues.JoinText(", ")).Append(")");
                sql = builder.ToString();
            }
            else
            {
                sql = CachedSelectFields;
            }

            return sql;
        }


        public string GetSelectSql(string where)
        {
            StringBuilder builder = new StringBuilder(CachedSelectFields);

            if (!where.IsNullOrEmpty())
            {
                builder.Append(" where ").Append(where);
            }

            return builder.ToString();
        }

        public object ConvertSqlToValue(EntityField field, object value)
        {
            if (value.IsNull())
                return null;

            if (field.IsEnum)
            {
                System.ComponentModel.TypeConverter convert = System.ComponentModel.TypeDescriptor.GetConverter(field.Info.PropertyType);
                return convert.ConvertFrom(value.ToString());
            }
            else
                return Convert.ChangeType(value, field.TypeCode);
        }


        /*
        [Obsolete]
        public string ConvertValueToSql(EntityField field, object value)
        {
            if (value.IsNull())
            {
                return "null";
            }
            else if (field.IsQuoted)
            {
                return "'" + value.ToString() + "'";
            }
            else
            {
                if (field.TypeCode == TypeCode.Boolean)
                {
                    return ((bool)value) ? "1" : "0";
                }
                else if (field.IsEnum)
                {
                    return value.GetHashCode().ToString();
                }
                else
                {
                    return value.ToString();
                }
            }
        }
        */
    }
}
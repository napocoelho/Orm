using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

using CoreDll.Extensions;
using Connections;
using System.Threading.Tasks;
using CoreDll.Extensions.Conversions;
using System.Text.RegularExpressions;

namespace CoreDll.Orm
{
    public class OrmService
    {
        /// <summary>
        /// A global instance
        /// </summary>
        public static OrmService FirstCreatedInstance { get; set; }
        public static OrmService LastCreatedInstance { get; set; }


        public Schema Schema { get; private set; }
        public ConnectionManager Manager { get; private set; }



        public OrmService(ConnectionManager manager)
        {
            Manager = manager;
            Schema = new Schema();

            if (OrmService.FirstCreatedInstance == null)
            {
                OrmService.FirstCreatedInstance = this;
            }

            OrmService.LastCreatedInstance = this;
        }

        public void TransactionBlock(Action<OrmService> transactionAction)
        {
            try
            {
                Manager.Connection.BeginTransaction();
                transactionAction(this);
                Manager.Connection.CommitTransaction();
            }
            catch (Exception ex)
            {
                Manager.Connection.RollbackTransaction();
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpression">An anonymous type. Exemple: OrmService.Load<Person>(new { Id = 22, Age = 33  }); </param>
        /// <returns></returns>        
        public List<T> Load<T>(string whereExpression) where T : class, new()
        {
            List<T> result = new List<T>();

            EntityRegistry registry = Schema.GetOrSetRegistry<T>();


            string sql = registry.GetSelectSql(whereExpression);

            DataTable tbl = Manager.Connection.ExecuteDataTable(sql);

            foreach (DataRow row in tbl.Rows)
            {
                T instance = LoadInstance<T>(registry, row);
                result.Add(instance);
            }

            return result;
        }


        public List<T> Load<T>(ICollection<T> entityCollection) where T : class, new()
        {
            List<T> result = new List<T>();

            if (entityCollection.NotIsNull())
            {
                EntityRegistry registry = Schema.GetOrSetRegistry<T>();

                List<object> idList = new List<object>();

                foreach (T instance in entityCollection)
                {
                    object id = registry.KeyField.GetValue(instance);
                    idList.Add(id);
                }

                string sql = registry.GetSelectSql(idList);

                DataTable tbl = Manager.Connection.ExecuteDataTable(sql);

                foreach (DataRow row in tbl.Rows)
                {
                    T newInstance = LoadInstance<T>(registry, row);
                    result.Add(newInstance);
                }
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpression">An anonymous type. Exemple: OrmService.Load<Person>(new { Id = 22, Age = 33  }); </param>
        /// <returns></returns>
        public async ValueTask<List<T>> LoadAsync<T>(string whereExpression) where T : class, new()
        {
            return await Task.Run(() => Load<T>(whereExpression));
        }

        /*
        public List<T> Load<T, TKey>(params TKey[] keyArgs) where T : class, new()
        {
            return null;
        }
        */

        public async ValueTask ReloadAsync<T>(ICollection<T> collection) where T : class, new()
        {
            await Task.Run(() => Reload(collection));
        }

        public async ValueTask ReloadAsync<T>(params T[] instanceArgs) where T : class, new()
        {
            await Task.Run(() => Reload(instanceArgs));
        }

        public void Reload<T>(ICollection<T> collection) where T : class, new()
        {
            Reload(collection.ToArray());
        }

        public void Reload<T>(params T[] instanceArgs) where T : class, new()
        {
            if (instanceArgs.NotIsNull())
            {
                EntityRegistry registry = Schema.GetOrSetRegistry<T>();

                foreach (T instance in instanceArgs)
                {
                    if (instance != null)
                    {
                        object id = registry.KeyField.GetValue(instance);
                        string sql = registry.GetSelectSql(id);

                        DataTable tbl = Manager.Connection.ExecuteDataTable(sql);

                        foreach (DataRow row in tbl.Rows)
                        {
                            LoadInstance<T>(registry, row, instance);
                        }
                    }
                }
            }
        }


        public async ValueTask<List<T>> LoadAsync<T>(params object[] keyArgs) where T : class, new()
        {
            return await Task.Run(() => Load<T>(keyArgs));
        }

        public List<T> Load<T>(params object[] keyArgs) where T : class, new()
        {
            List<T> result = new List<T>();

            EntityRegistry registry = Schema.GetOrSetRegistry<T>();

            string sql = null;

            if (keyArgs == null || !keyArgs.Any())
            {
                sql = registry.GetSelectSql();
            }
            else
            {
                sql = registry.GetSelectSql(keyArgs);
            }

            DataTable tbl = Manager.Connection.ExecuteDataTable(sql);

            foreach (DataRow row in tbl.Rows)
            {
                T instance = LoadInstance<T>(registry, row);
                result.Add(instance);
            }

            return result;
        }

        public async ValueTask<T> LoadOneAsync<T>(int? id) where T : class, new()
        {
            return await Task.Run(() => LoadOne<T>(id));
        }

        public T LoadOne<T>(int? id) where T : class, new()
        {
            return Load<T>(id.Value).FirstOrDefault();
        }



        private object LoadFromType(Type type, string whereExpression)
        {
            EntityRegistry registry = Schema.GetOrSetRegistry(type);

            Type genericListType = typeof(List<>).MakeGenericType(new Type[] { type });
            object resultList = Activator.CreateInstance(genericListType);

            string sql = registry.GetSelectSql(whereExpression);

            DataTable tbl = Manager.Connection.ExecuteDataTable(sql);

            foreach (DataRow row in tbl.Rows)
            {
                object instance = LoadInstance(type, registry, row);
                genericListType.GetMethod("Add").Invoke(resultList, new object[] { instance });
            }

            return resultList;
        }

        //public async ValueTask<T> LoadTheFirstOneAsync<T>() where T : class, new()
        //{
        //    return await Task.Run(()=> this.LoadTheFirstOne<T>());
        //}

        //public async ValueTask<T> LoadTheLastOneAsync<T>() where T : class, new()
        //{
        //    return await Task.Run(()=> this.LoadTheLastOne<T>());
        //}

        //public T LoadTheLastOne<T>(string whereExpression = null) where T : class, new()
        //{
        //    EntityRegistry registry = this.Schema.GetOrSetRegistry<T>();

        //    string where = $"{registry.KeyField} IN ( SELECT TOP 1 {registry.KeyField} FROM {registry.TableName} WHERE {whereExpression} )";
        //    string sql = registry.GetSelectSql(where);

        //    DataTable tbl = this.Manager.Connection.ExecuteDataTable(sql);

        //    T instance = null;

        //    if (tbl.Rows.Count > 0)
        //    {
        //        instance = this.LoadInstance<T>(registry, tbl.Rows[0]);
        //    }

        //    return instance;
        //}

        //public T LoadTheFirstOne<T>() where T : class, new()
        //{
        //    EntityRegistry registry = this.Schema.GetOrSetRegistry<T>();

        //    string where = $"{registry.KeyField} IN ( SELECT MIN( {registry.KeyField} ) FROM {registry.TableName} )";
        //    string sql = registry.GetSelectSql(where);

        //    DataTable tbl = this.Manager.Connection.ExecuteDataTable(sql);

        //    T instance = null;

        //    if (tbl.Rows.Count > 0)
        //    {
        //        instance = this.LoadInstance<T>(registry, tbl.Rows[0]);
        //    }

        //    return instance;
        //}

        private object LoadInstance(Type type, EntityRegistry registry, DataRow row)
        {
            object instance = Activator.CreateInstance(type);
            //T instance = new T();

            return LoadInstance(type, registry, row, instance);
        }

        private object LoadInstance(Type type, EntityRegistry registry, DataRow row, object instance)
        {
            foreach (EntityField field in registry.Fields)
            {
                object value = row[field.Column];
                field.SetValue(instance, registry.ConvertSqlToValue(field, value));
            }

            if (registry.IsSigned)
            {
                DecryptFields(instance, registry);
            }

            return instance;
        }

        private T LoadInstance<T>(EntityRegistry registry, DataRow row) where T : class, new()
        {
            return LoadInstance(typeof(T), registry, row) as T;
        }

        private T LoadInstance<T>(EntityRegistry registry, DataRow row, object instance) where T : class, new()
        {
            return LoadInstance(typeof(T), registry, row, instance) as T;
        }

        /// <summary>
        /// Salva (insert ou update) uma entidade em seu registro correspondente.
        /// Todos os campos da entidade serão salvos.
        public void Save<T>(T entity) where T : class, new()
        {
            SaveAll(new T[] { entity });
        }

        /// <summary>
        /// Salva (insert ou update) uma entidade em seu registro correspondente.
        /// Todos os campos da entidade serão salvos.
        public void Save<T>(params T[] entityArgs) where T : class, new()
        {
            SaveAll(entityArgs);
        }

        /// <summary>
        /// Salva (insert ou update) uma entidade em seu registro correspondente.
        /// Todos os campos da entidade serão salvos.
        public void Save<T>(ICollection<T> entityCollection) where T : class, new()
        {
            SaveAll<T>(entityCollection);
        }

        /// <summary>
        /// Atualiza os registros de uma entidade pré-existente.
        /// Os campos a serem atualizados podem ser selecionados através das propriedades de uma Anonymous Class.
        /// O campo [Key] da entidade sempre deverá ser declarado na classe anônima.
        /// </summary>        
        public void Update<T>(object anonymousEntity) where T : class, new()
        {
            UpdateAllAnonymous<T>(new object[] { anonymousEntity });
        }

        private (T, string[]) CreateEntityFromAnonymous<T>(object anonymousEntity) where T : class, new()
        {
            T newEntity = new T();
            EntityRegistry newEntityregistry = Schema.GetOrSetRegistry<T>();

            Type anonymousType = anonymousEntity.GetType();
            List<string> propertiesSelected = new List<string>();

            foreach (PropertyInfo propertyInfo in anonymousType.GetRuntimeProperties())
            {
                EntityField field = newEntityregistry.Fields.Where(x => x.Info.Name == propertyInfo.Name).FirstOrDefault();

                if (field.NotIsNull())
                {
                    field.SetValue(newEntity, propertyInfo.GetValue(anonymousEntity));
                    propertiesSelected.Add(field.Info.Name);
                }
                else if (field.IsKey)
                {
                    throw new Exception("The anonymous entity does not have an entity key");
                }
            }

            return (newEntity, propertiesSelected.ToArray());
        }

        private void UpdateAllAnonymous<T>(ICollection<object> anonymousEntityCollection) where T : class, new()
        {
            string sql = null;

            EntityRegistry registry = Schema.GetOrSetRegistry<T>();

            try
            {
                //this.Manager.Connection.BeginTransaction();

                foreach (object anonymousEntity in anonymousEntityCollection)
                {
                    (T entity, string[] propertiesSelected) = CreateEntityFromAnonymous<T>(anonymousEntity);   // cria uma entidade do tipo original para ser salva pelo Orm Service.

                    //object keyValue = registry.KeyField.GetValue(entity);


                    //if (keyValue.IsNull())
                    //{
                    //    sql = registry.GetInsertSql(entity, propertiesSelected);

                    //    if (registry.KeyField.IsId)
                    //    {
                    //        int? idValue = (int?)registry.KeyField.GetValue(entity);
                    //        this.Manager.Connection.ExecuteScalar(sql);
                    //        //registry.KeyField.SetValue(entity, idValue.Value);
                    //    }
                    //}
                    //else
                    //{
                    sql = registry.GetUpdateSql(entity, propertiesSelected);
                    int count = Manager.Connection.ExecuteNonQuery(sql);
                    //}

                    if (registry.IsSigned)
                    {
                        EncryptFields(entity, registry);
                    }
                }

                //this.Manager.Connection.TryCommitTransaction();
            }
            catch (Exception ex)
            {
                //Manager.Connection.RollbackTransaction();
                throw ex;
            }
        }

        public T New<T>() where T : class, new()
        {
            EntityRegistry registry = Schema.GetOrSetRegistry<T>();
            T instance = new T();

            string columnNames = "'" + registry.Fields.Select(x => x.Column).ToArray().JoinText("', '") + "'";
            string sql = $"SELECT COLUMN_NAME, DEFINITION FROM VIEW_CONSTRAINTS_DF WHERE TABLE_NAME = '{registry.TableName}' AND COLUMN_NAME IN ( {columnNames} )";

            DataTable table = Manager.Connection.ExecuteDataTable(sql);

            foreach (DataRow row in table.Rows)
            {
                EntityField field = registry.Fields.Where(x => x.Column == row["COLUMN_NAME"].ToString()).FirstOrDefault();

                if (field != null)
                {
                    string value = row["DEFINITION"].ToStringN();


                    if (value.IsNull())
                    {
                        field.SetValue(instance, default(T));
                    }
                    else
                    {
                        value = value.Trim();

                        while (value.StartsWith("("))
                        {
                            //(((...)))
                            value = value.SkipLeft(1).SkipRight(1).Trim();
                        }

                        if (value.StartsWith("N"))
                        {
                            //N'...'
                            value = value.SkipLeft(2).SkipRight(1).Trim();
                        }

                        if (value.StartsWith("'"))
                        {
                            //'...'
                            value = value.SkipLeft(1).SkipRight(1).Trim();
                        }


                        if (field.IsQuoted)
                        {
                            value = value.Replace("(", "").Replace(")", "");
                            value = Regex.Replace(value, "", "");
                            field.SetValue(instance, registry.ConvertSqlToValue(field, value));
                        }
                        else if (!field.IsQuoted)
                        {
                            if (field.TypeCode == TypeCode.UInt16 || field.TypeCode == TypeCode.UInt32 || field.TypeCode == TypeCode.UInt64
                                || field.TypeCode == TypeCode.Int16 || field.TypeCode == TypeCode.Int32 || field.TypeCode == TypeCode.Int64)
                            {
                                value = value.Replace("(", "").Replace(")", "").SkipAfter(".", StringComparison.OrdinalIgnoreCase, false);
                                field.SetValue(instance, registry.ConvertSqlToValue(field, value));
                            }
                            else if (field.TypeCode == TypeCode.Boolean)
                            {
                                bool convertedValue = value.Replace("(", "").Replace(")", "").Trim().ToBool();
                                field.SetValue(instance, registry.ConvertSqlToValue(field, convertedValue));

                            }
                            else
                            {
                                value = value.Replace("(", "").Replace(")", "").Trim();
                                field.SetValue(instance, registry.ConvertSqlToValue(field, value));
                            }


                        }
                    }
                }
            }

            return instance;
        }



        private void SaveAll<T>(ICollection<T> entityCollection) where T : class, new()
        {
            string sql = null;
            EntityRegistry registry = null;

            Type genericType = typeof(T).GenericTypeArguments.FirstOrDefault();

            if (!(genericType is null))
            {
                throw new Exception($"O tipo genérico não foi especificado para o método {nameof(SaveAll)}<>");
            }


            registry = Schema.GetOrSetRegistry<T>();

            try
            {
                //this.Manager.Connection.BeginTransaction();

                foreach (T entity in entityCollection)
                {
                    object keyValue = registry.KeyField.GetValue(entity);

                    if (keyValue.IsNull())
                    {
                        /*
                        string[] selectedProperties = null;

                        BindableBase bindable = entity as BindableBase;

                        if (bindable != null)
                        {
                            selectedProperties = BindableBase.GetPropertyValues(bindable).Where(x => x.IsValueChanged).Select(x => x.Name).ToArray<string>();
                        }
                        */

                        //sql = registry.GetInsertSql(entity, selectedProperties);
                        sql = registry.GetInsertSql(entity);

                        if (registry.KeyField.IsId)
                        {
                            int? idValue = (int?)registry.KeyField.GetValue(entity);
                            idValue = Manager.Connection.ExecuteScalarAndGetLastInsertedId(sql);
                            registry.KeyField.SetValue(entity, idValue.Value);
                        }
                    }
                    else
                    {
                        sql = registry.GetUpdateSql(entity);
                        int count = Manager.Connection.ExecuteNonQuery(sql);
                    }

                    if (registry.IsSigned)
                    {
                        EncryptFields(entity, registry);
                    }
                }

                //this.Manager.Connection.TryCommitTransaction();
            }
            catch (Exception ex)
            {
                //Manager.Connection.RollbackTransaction();
                throw ex;
            }
        }



        private void EncryptFields(object entity, EntityRegistry registry)
        {
            List<string> parametros = new List<string>();
            List<EntityField> signedFields = registry.Fields.Where(x => x.IsSigned && !x.IsSignature).ToList();

            foreach (EntityField field in signedFields)
            {
                string valor = Convert.ChangeType(field.GetValue(entity), TypeCode.String).ToString().Ignore('|');
                parametros.Add($"{field.Info.Name}={valor}");
            }

            EntityField signatureField = registry.Fields.First(x => x.IsSignature);
            string signature = parametros.JoinText("|");
            signature = Cryp.Cryptography.Encrypt(signature);

            signatureField.SetValue(entity, signature);
            string keyValue = registry.KeyField.ToSqlConversion(registry.KeyField.Info.GetValue(entity));
            string sql = $"UPDATE [{registry.TableName}] SET {signatureField.Column} = '{signature}' WHERE {registry.KeyField.Column} = {keyValue}";
            Manager.Connection.ExecuteNonQuery(sql);
        }

        private void DecryptFields(object entity, EntityRegistry registry)
        {
            bool isViolated = false;

            EntityField signatureField = registry.Fields.First(x => x.IsSignature);
            EntityField isViolatedField = registry.IgnoredFields.First(x => x.IsViolated);
            List<EntityField> signedFields = registry.Fields.Where(x => x.IsSigned && !x.IsSignature).ToList();

            string signature = signatureField.GetValue(entity).ToString();
            signature = CoreDll.Cryp.Cryptography.Decrypt(signature);
            Dictionary<string, string> parameters = signature.SplitText("|", "=");

            foreach (KeyValuePair<string, string> pair in parameters)
            {
                EntityField field = registry.Fields.FirstOrDefault(x => x.Info.Name == pair.Key);
                Type propertyTypeForConversion = Nullable.GetUnderlyingType(field.Info.PropertyType) ?? field.Info.PropertyType;

                object oldValue = field.Info.GetValue(entity);
                object newValue = Convert.ChangeType(pair.Value, propertyTypeForConversion);

                if (!oldValue.Equals(newValue))
                {
                    isViolated = true;
                }

                field.Info.SetValue(entity, newValue);
            }

            isViolatedField.Info.SetValue(entity, isViolated);
        }

        public void Delete<T>(params T[] entityArgs) where T : class, new()
        {
            EntityRegistry registry = Schema.GetOrSetRegistry<T>();

            if (entityArgs == null || entityArgs.Count() == 0)
            {
                Delete<T>();
            }
            else
            {
                List<int> values = new List<int>();

                for (int idx = 0; idx < entityArgs.Count(); idx++)
                {
                    object value = registry.KeyField.GetValue(entityArgs[idx]);

                    if (value.IsNull())
                        continue;

                    values.Add((int)value);
                }

                Delete<T>(values.ToArray());
            }
        }

        public void Delete<T>(params int[] idArgs) where T : class, new()
        {
            EntityRegistry registry = Schema.GetOrSetRegistry<T>();

            if (idArgs == null || idArgs.Count() == 0)
            {
                Delete<T>();
            }
            else
            {
                StringBuilder builder = new StringBuilder();

                builder.Append(registry.KeyField.Column).Append(" in ").Append("(");

                for (int idx = 0; idx < idArgs.Count(); idx++)
                {
                    if (idx > 0)
                        builder.Append(", ");

                    builder.Append(idArgs[idx]);
                }

                builder.Append(")");

                Delete<T>(builder.ToString());
            }
        }

        public void Delete<T>(string whereExpression = null) where T : class, new()
        {
            //List<T> instances = new List<T>();
            EntityRegistry registry = Schema.GetOrSetRegistry<T>();

            StringBuilder builder = new StringBuilder("delete from ");

            builder.Append(registry.TableName);

            if (whereExpression != null)
            {
                builder.Append(" where ").Append(whereExpression);
            }

            try
            {
                //this.Manager.Connection.BeginTransaction();

                int count = Manager.Connection.ExecuteNonQuery(builder.ToString());

                //this.Manager.Connection.CommitTransaction();
            }
            catch (Exception ex)
            {
                //Manager.Connection.RollbackTransaction();
                throw ex;
            }
        }
    }
}
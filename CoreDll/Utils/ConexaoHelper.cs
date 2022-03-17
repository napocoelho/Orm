using Connections;
using System;
using System.Collections.Generic;
using System.Data;


namespace CoreDll.Utils
{
    public static class ConexaoHelper
    {
        private static Connections.ConnectionManager MANAGER = null;
        //public static string ConnectionString { get; private set; }

        public static ConnectionManager Manager
        {
            get
            {
                return ConexaoHelper.MANAGER;
            }
        }

        public static ThreadSafeConnection Connection
        {
            get
            {

                if (ConexaoHelper.MANAGER == null)
                {
                    throw new Exception($"Falha na conexão com o banco de dados! Não foi encontrada nenhuma conexão.");
                }

                return ConexaoHelper.MANAGER.Connection;
            }
        }

        public static ThreadSafeConnection Conectar(string server, string database, string appName, int connectionTimeout)
        {
            if (server == null)
                throw new ArgumentNullException($"O parâmetro [{nameof(server)}] não pode ser nulo");

            if (database == null)
                throw new ArgumentNullException($"O parâmetro [{nameof(database)}] não pode ser nulo");


            List<string> initialCommands = new List<string>();
            initialCommands.Add("SET LANGUAGE 'Português (Brasil)'");
            initialCommands.Add("SET LOCK_TIMEOUT 5000");

            string connectionString = $"Initial Catalog={database};Data Source={server};User ID=sistema;Password=schwer_wissen;Connect Timeout={connectionTimeout};Application Name='{appName}';pooling=false;";

            //ConexaoHelper.ConnectionString = connectionString;

            ConexaoHelper.MANAGER = ConnectionManager.CreateInstance(connectionString);

            return ConexaoHelper.Connection;
        }

        public static List<dynamic> GetAll(string sqlExpression)
        {
            List<dynamic> items = new List<dynamic>();

            DataTable table = ConexaoHelper.Connection.ExecuteDataTable(sqlExpression);

            foreach (DataRow row in table.Rows)
            {
                dynamic item = new System.Dynamic.ExpandoObject();

                foreach (DataColumn column in table.Columns)
                {
                    ((IDictionary<String, Object>)item).Add(column.ColumnName, row[column.ColumnName]);
                }

                items.Add(item);
            }

            return items;
        }

        public static List<T> GetAll<T>(string sqlExpression) where T : new()
        {
            List<T> items = new List<T>();
            List<System.Reflection.PropertyInfo> fields = new List<System.Reflection.PropertyInfo>();

            DataTable table = ConexaoHelper.Connection.ExecuteDataTable(sqlExpression);

            foreach (DataColumn column in table.Columns)
            {
                foreach (System.Reflection.PropertyInfo info in typeof(T).GetProperties())
                {
                    if (info.Name.Equals(column.ColumnName, StringComparison.Ordinal))
                    {
                        fields.Add(info);
                    }
                }
            }

            foreach (DataRow row in table.Rows)
            {
                T item = new T();

                foreach (System.Reflection.PropertyInfo field in fields)
                {
                    field.SetValue(item, row[field.Name]);
                }

                items.Add(item);
            }

            return items;
        }

        /*
        public static List<T> GetAll<T>(string sqlExpression)
        {
            Convert.ChangeType()
        }
        */
    }
}
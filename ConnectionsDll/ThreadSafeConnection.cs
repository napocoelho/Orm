using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
//using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Data.SqlClient;



namespace Connections
{
    public class ThreadSafeConnection
    {

        public Thread CurrentThread { get; set; }
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }
        //<teste>public SqlDataReader DataReader { get; set; }

        private CommitLevel TryCommitLevel { get; set; }
        private bool TransactionBlock { get; set; }
        private ConnectionManager ConnectionManagerOwner { get; set; }



        private List<string> InitialCommands { get; set; }




        /// <summary>
        /// Get the last CommandText (query or sql command) ordered through this connections
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>        
        public string LastExecutedCommandText { get; private set; }


        public event EventHandler<string> ExecutedCommand;


        private void RaiseExecutedCommandText(string commandText)
        {
            LastExecutedCommandText = commandText;
            ExecutedCommand?.Invoke(this, commandText);
            //ExecutedCommandTextList.Add(command);
        }



        public ThreadSafeConnection(ConnectionManager owner, Thread currentThread, string connectionString, List<string> initialCommands = null)
        {

            InitialCommands = new List<string>();

            ConnectionManagerOwner = owner;
            TransactionBlock = false;
            TryCommitLevel = new CommitLevel();
            CurrentThread = currentThread;
            Connection = new SqlConnection();

            Connection.ConnectionString = connectionString;



            // Preenchendo lista:
            if (initialCommands != null)
            {
                initialCommands.ForEach(x => InitialCommands.Add(x));
            }

            TestConnection();
            Transaction = null;
        }



        public void TestConnection()
        {
            try
            {
                // Força o fechamento da conexão, se estiver aberta:
                Close(); ;
            }
            catch { }

            try
            {
                Connection.Open();

                if (Connection.State == ConnectionState.Open && InitialCommands != null)

                    foreach (string xSql in InitialCommands)
                    {

                        SqlCommand Comando = new SqlCommand();
                        Comando.CommandType = CommandType.Text;
                        Comando.Connection = Connection;
                        Comando.CommandText = xSql;
                        Comando.CommandTimeout = 5000;
                        Comando.ExecuteNonQuery();
                        Comando.Dispose();
                    }

                //this.Close();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Connection.Dispose();

                if (ex.Number == 4060)
                {

                    //MsgBox("Nome de base de dados inválido!", MsgBoxStyle.Exclamation, Ex.Number)

                    throw new Exception("Nome de base de dados inválido!", ex);
                }

                else if (ex.Number == 1231)
                {

                    //MsgBox("Nome do servidor SQL inválido!", MsgBoxStyle.Exclamation, Ex.Number)

                    throw new Exception("Nome do servidor SQL inválido!", ex);
                }

                throw ex;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                {
                    Close(); ;
                }
            }
        }

        public void Reconnect()
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }


            }
            catch { }
        }


        public ConnectionState State
        {
            get
            {
                return Connection.State;
            }
        }




        /// <summary>
        /// Verifica se há alguma transação ativa que não tenha sido finalizada na Thread atual.
        /// </summary>    
        public bool HasActiveTransaction
        {
            get
            {
                lock (CurrentThread)
                {
                    return (Transaction != null);
                }
            }
        }

        /// <summary>
        /// Executa um comando em SQL.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>Retorna o número de registros afetados.</returns>
        public int ExecuteNonQuery(string sqlCommand)
        {
            SqlCommand comando = new SqlCommand();

            lock (CurrentThread)
            {
                try
                {
                    if (Connection.State == ConnectionState.Closed)
                    {
                        Reconnect();
                    }

                    comando.CommandType = CommandType.Text;
                    comando.Connection = Connection;
                    comando.CommandText = sqlCommand;
                    comando.CommandTimeout = 0;

                    if (HasActiveTransaction)
                    {
                        comando.Transaction = Transaction;
                    }

                    try
                    {
                        int value = comando.ExecuteNonQuery();
                        return value;
                    }
                    finally
                    {
                        RaiseExecutedCommandText(comando.CommandText);
                    }
                }
                finally
                {
                    comando.Dispose();

                    if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                    {
                        Close(); ;
                    }
                }
            }
        }

        public IEnumerable<T> ExecuteMapping<T>(string query) where T : class, new()
        {
            List<T> lista = new List<T>();


            DataTable tbl = ExecuteDataTable(query);
            PropertyInfo[] infos = typeof(T).GetProperties();

            Dictionary<PropertyInfo, DataColumn> names = new Dictionary<PropertyInfo, DataColumn>();

            // Fazendo o INTERSECT
            foreach (PropertyInfo property in infos)
            {
                foreach (DataColumn column in tbl.Columns)
                {
                    if (property.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        names.Add(property, column);
                    }
                }
            }

            if (names.Any())
            {
                foreach (DataRow row in tbl.Rows)
                {
                    T newEntity = new T();

                    foreach (KeyValuePair<PropertyInfo, DataColumn> pair in names)
                    {
                        PropertyInfo property = pair.Key;
                        DataColumn column = pair.Value;

                        object value = row[column];
                        value = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(newEntity, value);
                    }

                    lista.Add(newEntity);
                }
            }

            return lista;
        }



        /// <summary>
        /// Retorna um ou mais registros dado um comando SQL.
        /// É importante que este método seja utilizado em conjunto com CloseDataReader() e, quando necessário, HasActiveDataReader.
        ///                                                              
        /// Obs 1.: Aconselha-se o uso de ExecuteDataTable(...) ao invés deste, por questões de escalabilidade;
        /// Obs 2.: Nenhum outro Comando SQL funcionará enquanto existir um DataReader ativo na conexão.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>O DataReader retornado segura a conexão com o banco de dados.</returns>
        public void ExecuteDataReader(string sqlCommand, Action<SqlDataReader> consumeDataReaderAction)
        {
            //**** IMPORTANTE ****
            Block();


            if (Connection.State == ConnectionState.Closed)
            {
                Reconnect();
            }

            try
            {
                using (SqlCommand comando = new SqlCommand())
                {
                    /*
                    if (this.HasActiveDataReader)
                    {
                        this.CloseDataReader();
                    }
                    */

                    comando.CommandType = CommandType.Text;
                    comando.Connection = Connection;
                    comando.CommandText = sqlCommand;
                    comando.CommandTimeout = 0;

                    if (HasActiveTransaction)
                    {
                        comando.Transaction = Transaction;
                    }

                    try
                    {
                        using (SqlDataReader dataReader = comando.ExecuteReader())
                        {
                            consumeDataReaderAction(dataReader);
                        }
                    }
                    finally
                    {
                        RaiseExecutedCommandText(comando.CommandText);
                    }
                }
            }
            finally
            {
                if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                {
                    Close();
                }
            }

            //consumeDataReaderAction();

            //return this.DataReader;
        }

        /// <summary>
        /// Verifica se o comando SQL retornará algum registro, mesmo que existam valores nulos.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        public bool ExecuteExists(string sqlCommand)
        {
            try
            {
                using (SqlCommand Comando = new SqlCommand())
                {
                    Comando.CommandType = CommandType.Text;
                    Comando.Connection = Connection;
                    Comando.CommandText = sqlCommand;
                    Comando.CommandTimeout = 0;


                    if (HasActiveTransaction)
                    {
                        Comando.Transaction = Transaction;
                    }

                    try
                    {
                        using (SqlDataReader dataReader = Comando.ExecuteReader())
                        {
                            return dataReader.HasRows;
                        }
                    }
                    finally
                    {
                        RaiseExecutedCommandText(sqlCommand);
                    }
                }
            }
            finally
            {
                if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                {
                    Close(); ;
                }
            }
        }

        /// <summary>
        /// Retorna um único valor, dado um comando SQL.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>Possivelmente um objeto contendo: texto, numero, data ou sequencia de bits.</returns>
        public object ExecuteScalar(string sqlCommand)
        {
            SqlCommand Comando = new SqlCommand();

            lock (CurrentThread)
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Reconnect();
                }

                try
                {
                    Comando.CommandType = CommandType.Text;
                    Comando.Connection = Connection;
                    Comando.CommandText = sqlCommand;
                    Comando.CommandTimeout = 0;


                    if (HasActiveTransaction)
                    {
                        Comando.Transaction = Transaction;
                    }

                    try
                    {
                        object value = Comando.ExecuteScalar();
                        Comando.Dispose();
                        return value;
                    }
                    finally
                    {
                        RaiseExecutedCommandText(sqlCommand);
                    }
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                    {
                        Close(); ;
                    }
                }
            }
        }

        /// <summary>
        /// Executa um comando SQL e retorna a última chave inserida.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>Possivelmente um objeto contendo: texto, numero, data ou sequencia de bits.</returns>
        public int? ExecuteScalarAndGetLastInsertedId(string sqlCommand)
        {
            int? retorno = null;
            SqlCommand Comando = new SqlCommand();

            lock (CurrentThread)
            {
                try
                {
                    if (Connection.State == ConnectionState.Closed)
                    {
                        Reconnect();
                    }

                    Comando.CommandType = CommandType.Text;
                    Comando.Connection = Connection;
                    Comando.CommandText = sqlCommand;
                    Comando.CommandTimeout = 0;



                    if (HasActiveTransaction)
                    {
                        Comando.Transaction = Transaction;
                    }

                    try
                    {
                        Comando.ExecuteScalar();
                    }
                    finally
                    {
                        RaiseExecutedCommandText(sqlCommand);
                    }

                    try
                    {
                        Comando.CommandText = "SELECT @@IDENTITY";
                        //object value;

                        try
                        {
                            object value = Comando.ExecuteScalar();
                            retorno = int.Parse(value.ToString());
                        }
                        finally
                        {
                            RaiseExecutedCommandText(Comando.CommandText);
                        }
                    }
                    catch (Exception)
                    {
                        retorno = null;
                    }

                    Comando.Dispose();
                    //CloseConnection()

                    return retorno;
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                    {
                        Close(); ;
                    }
                }
            }

        }

        /*<teste>
        /// <summary>
        /// Fecha o DataReader aberto por ExecuteDataReader(...).
        /// </summary>
        public void CloseDataReader()
        {
            lock (this.CurrentThread)
            {

                try
                {
                    if (this.HasActiveDataReader)
                    {
                        this.DataReader.Close();

                        //**** IMPORTANTE ****
                        this.Unblock();
                    }
                }
                finally
                {
                    this.DataReader = null;
                }
                //CloseConnection()
            }
        }
        */


        /// <summary>
        /// Retorna um ou mais registros dado um comando SQL
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>O DataTable retornado não segura a conexão com o banco de dados.</returns>
        public System.Data.DataTable ExecuteDataTable(string sqlCommand, bool ignoreConstraints = true)
        {
            lock (Thread.CurrentThread)
            {
                if (ignoreConstraints)
                {
                    return ExecuteDataTableIgnoringConstraints(sqlCommand);
                }
                else
                {
                    System.Data.DataTable xTabela = new System.Data.DataTable();

                    /*<teste>
                    SqlDataReader DataReader_Aux = this.ExecuteDataReader(sqlCommand);


                    if (DataReader_Aux != null)
                    {

                        xTabela.Load(DataReader_Aux);
                    }

                    this.CloseDataReader();
                    //CloseConnection()
                    */


                    if (Connection.State == ConnectionState.Closed)
                    {
                        Reconnect();
                    }


                    try
                    {
                        ExecuteDataReader(sqlCommand, dr =>
                        {
                            if (dr != null)
                            {
                                xTabela.Load(dr);
                            }
                        });
                    }
                    finally
                    {
                        if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                        {
                            Close(); ;
                        }
                    }


                    return xTabela;
                }
            }
        }


        /// <summary>
        /// Retorna todos os registros da primeira coluna encontrada
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>Lista com todos os registros encontrados na coluna</returns>
        public List<object> ExecuteColumn(string sqlCommand, bool ignoreConstraints = true)
        {
            List<object> columnElements = new();

            DataTable table = ExecuteDataTable(sqlCommand, ignoreConstraints);

            foreach (DataRow rows in table.Rows)
            {
                columnElements.Add(rows[0]);
            }

            return columnElements;
        }


        /// <summary>
        /// Retorna um ou mais registros dado um comando SQL, ignorando qualquer constraint.
        /// * Obs.: é útil quando o SGBD não respeita as próprias constraints, como unique ou qualquer outro tipo.
        ///         As vezes, uma determinada coluna pode ser unique, mas por alguma falha interna, o SGBD mantém informações duplicadas.
        ///         Este método fará com que o DataTable ignorare qualquer constraint.
        /// </summary>
        /// <param name="sqlCommand">Comando SQL válido.</param>
        /// <returns>O DataTable retornado não segura a conexão com o banco de dados.</returns>
        public System.Data.DataTable ExecuteDataTableIgnoringConstraints(string sqlCommand)
        {
            lock (Thread.CurrentThread)
            {
                System.Data.DataTable dtOutputTable = null;

                if (Connection.State == ConnectionState.Closed)
                {
                    Reconnect();
                }

                try
                {
                    //<teste> SqlDataReader dtReader = this.ExecuteDataReader(sqlCommand);

                    //-------------------------------------------------------------

                    ExecuteDataReader(sqlCommand, dtReader =>
                    {
                        System.Data.DataTable dtSchemaTable = new System.Data.DataTable();
                        dtOutputTable = new System.Data.DataTable();
                        DataColumn dtColumn = null;
                        DataRow dtRow = null;

                        dtSchemaTable = dtReader.GetSchemaTable();



                        // Verificando se há colunas repetidas:
                        HashSet<string> moreThanOnceFieldList = new HashSet<string>();

                        for (int i = 0; i < dtSchemaTable.Rows.Count; i++)
                        {
                            int countOcurrencies = 0;

                            for (int j = 0; j < dtSchemaTable.Rows.Count; j++)
                            {
                                if (dtSchemaTable.Rows[i]["ColumnName"].ToString().Equals(dtSchemaTable.Rows[j]["ColumnName"].ToString(), StringComparison.OrdinalIgnoreCase))
                                {
                                    countOcurrencies++;

                                    if (countOcurrencies > 1)
                                    {
                                        string columnName = dtSchemaTable.Rows[i]["ColumnName"].ToString();
                                        moreThanOnceFieldList.Add(columnName);
                                        break;
                                    }
                                }
                            }
                        }

                        if (moreThanOnceFieldList.Count > 0)
                        {
                            throw new Exception(string.Format("The columns [{0}] were found more than once!", string.Join(", ", moreThanOnceFieldList)));
                        }



                        for (int i = 0; i < dtSchemaTable.Rows.Count; i++)
                        {
                            dtColumn = new DataColumn();

                            if (!dtOutputTable.Columns.Contains(dtSchemaTable.Rows[i]["ColumnName"].ToString()))
                            {
                                /*
                                ColumnName, ColumnOrdinal, ColumnSize, NumericPrecision, NumericScale, 
                                DataType, ProviderType, IsLong, AllowDBNull, IsReadOnly, IsRowVersion, 
                                IsUnique, IsKey, IsAutoIncrement, IsAliased, IsExpression, BaseSchemaName, 
                                BaseCatalogName, BaseTableName, BaseColumnName
                                */

                                dtColumn.ColumnName = dtSchemaTable.Rows[i]["ColumnName"].ToString();
                                dtColumn.Unique = false;
                                dtColumn.AllowDBNull = true;
                                dtColumn.DataType = dtSchemaTable.Rows[i]["DataType"] as Type;
                                dtColumn.Caption = dtColumn.ColumnName;
                                dtOutputTable.Columns.Add(dtColumn);
                            }
                        }

                        while (dtReader.Read())
                        {
                            dtRow = dtOutputTable.NewRow();

                            for (int i = 0; i < dtOutputTable.Columns.Count; i++)
                            {
                                dtRow[i] = dtReader.GetValue(i);
                                /*
                                try
                                {
                                    
                                }
                                catch (Exception ex)
                                {
                                    string teste = ex.Message;
                                }
                                */
                            }

                            dtOutputTable.Rows.Add(dtRow);
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (Connection.State == ConnectionState.Open && !HasActiveTransaction)
                    {
                        Close(); ;
                    }
                }

                return dtOutputTable;
            }
        }


        /// <summary>
        /// Inicia uma transação com o banco de dados. 
        /// Deve ser utilizado em conjunto os seguintes métodos: CommitTransaction() ou RollbackTransaction().
        /// Este método é ThreadSafe.
        /// </summary>
        public void BeginTransaction(System.Data.IsolationLevel? isolationLevelInfo = null)
        {
            //**** IMPORTANTE ****
            Block();

            TryCommitLevel.Up();

            if (!HasActiveTransaction)
            {
                Reconnect();

                if (!isolationLevelInfo.HasValue)
                {
                    Transaction = Connection.BeginTransaction();
                }
                else
                {
                    Transaction = Connection.BeginTransaction(isolationLevelInfo.Value);
                }

            }

        }

        /// <summary>
        /// Envia toda a sequência de comandos para o banco de dados, desde a chamada por BeginTransaction().
        /// Deve ser utilizado em conjunto com o método BeginTransaction().
        /// Este método é ThreadSafe.
        /// </summary>
        public void CommitTransaction()
        {

            if (HasActiveTransaction)
            {
                TryCommitLevel.Reset();

                Transaction.Commit();
                Transaction.Dispose();
                Transaction = null;

                //**** IMPORTANTE ****
                Unblock();
                Close();
            }
        }

        /// <summary>
        /// Executa [CommitTransaction] apenas quando o mesmo número de [BeginTransaction] e [TryCommitTransaction] forem chamados.
        /// Este método é aconselhado para quando se deseja transferir a responsabilidade de execução da transação para fora
        /// do escopo referido. Lembrando que [CommitTransaction] fará imediatamente a função proposta.
        /// </summary>
        public void TryCommitTransaction()
        {
            TryCommitLevel.Down();

            if (TryCommitLevel.Level == 0)
            {

                CommitTransaction();
            }
        }

        /// <summary>
        /// Cancela toda a sequência de comandos enviados para o banco de dados, desde a chamada por BeginTransaction().
        /// Deve ser utilizado em conjunto com o método BeginTransaction().
        /// Este método é ThreadSafe.
        /// </summary>
        public void RollbackTransaction()
        {
            if (HasActiveTransaction)
            {
                //Transacao = DictTransacoesAsync(Thread.CurrentThread)
                //ConnectionManager.GetConnection.RollbackTransaction()
                Transaction.Rollback();
                Transaction.Dispose();
                TryCommitLevel.Reset();
                Transaction = null;

                //**** IMPORTANTE ****
                Unblock();
                Close(); ;
            }
        }



        /// <summary>
        /// Inicia e controla bloqueios de ConnectionManager.
        /// </summary>
        /// <remarks>
        /// Gerencia os bloqueios para que seja criado apenas 1 por thread, tanto 
        /// para Transactions quanto para DataReaders.
        /// </remarks>
        private void Block()
        {
            lock (CurrentThread)
            {
                if (!TransactionBlock)
                {
                    Monitor.Enter(CurrentThread);
                    TransactionBlock = true;
                }
            }
        }

        /// <summary>
        /// Controla e finaliza bloqueios de ConnectionManager.
        /// </summary>
        /// <remarks>
        /// Gerencia os bloqueios para que não finalize enquanto
        /// existir DataReaders e/ou Transactions ativos. O último
        /// que terminar - Transaction ou DataReader -, finalizará 
        /// o bloqueio.
        /// </remarks>
        private void Unblock()
        {
            lock (CurrentThread)
            {
                if (TransactionBlock)
                {
                    //<teste> if (!this.HasActiveDataReader && !this.HasActiveTransaction)
                    if (!HasActiveTransaction)
                    {
                        Monitor.Exit(CurrentThread);
                        TransactionBlock = false;
                    }
                }
            }
        }




        //// <summary>
        //// Encerra a instância de ConnectionManager.
        //// </summary>
        public void Dispose()
        {            
            Close();
            ConnectionManagerOwner.PurgeConnection(this);

            CurrentThread = null;
            Connection = null;
            Transaction = null;
            //<teste> this.DataReader = null;
            TryCommitLevel = null;



            //GC.Collect();

            //GC.SuppressFinalize(this);
        }

        public void Close()
        {
            try
            {
                Connection.Close(); ;
            }
            catch { }
        }

        ~ThreadSafeConnection()
        {
            // Simply call Dispose(false).
            Dispose();
        }

        //private void OpenConnection()
        //    Try
        //        this.Connection.Open()
        //    finally
        //    }
        //}

        //private void CloseConnection()
        //    Try
        //        this.Connection.Close()
        //    finally
        //    }

        //}

    }

}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

//using ConnectionManagerDll.Collections;



namespace Connections
{

    /// <summary>
    /// Gerencia conexões primárias com o banco de dados, encapsulando as classes SqlTransaction, SqlConnection e SqlDataReader, SqlDataTable.
    /// Simplifica o uso de transações e ainda cria conexões independentes para cada thread ativa que utilizar ConnectionManager, evitando
    /// bloqueios e falhas ao fazer dar comandos no banco de dados. Não é necessário criar novas instâncias de ConnectionManager quando estiver 
    /// em diferentes threads. Além de tudo, os métodos de ConnectionManager são ThreadSafe.
    /// </summary>
    public class ConnectionManager
    {
        private static bool DISPOSED = false;
        private static object BLOQUEIO = new object();  //apenas gera um objeto aleatório na memória (não pode ser um texto fixo para evitar internalização de string, por isso foi utilizado o array de chars)
        //private static ConnectionManager INSTANCE = null;

        private ConcurrentDictionary<Thread, ThreadSafeConnection> ConnectionPool;
        //private WeakConnectionPool ConnectionPool;

        public string ConnectionString { get; private set; }

        private List<string> InitialCommands;

        //public int DeadThreadsRemovedFromThePool { get; private set; }

        /// <summary>
        /// Construtor de um singleton.
        /// </summary>    
        private ConnectionManager(string connectionString, params string[] initialCommands)
        {
            ConnectionString = connectionString;
            InitialCommands = initialCommands.ToListValue();
            ConnectionPool = new ConcurrentDictionary<Thread, ThreadSafeConnection>();
            //this.ConnectionPool = new WeakConnectionPool();

            this.StartGarbageCollector();
        }

        private void StartGarbageCollector()
        {
            Task.Run(() =>
            {
                while (!DISPOSED)
                {
                    Thread.Sleep(10000);
                    ThreadState[] notOkThreadStates = { ThreadState.Aborted, ThreadState.Stopped };

                    try
                    {
                        KeyValuePair<Thread, ThreadSafeConnection>[] exclusoes = ConnectionPool.Where(item =>
                                                                                                                            notOkThreadStates.Any(state => state.GetHashCode() == item.Key.ThreadState.GetHashCode())
                                                                                                                            ).ToArray();
                        for (int idx = exclusoes.Count() - 1; idx >= 0; idx--)
                        {
                            KeyValuePair<Thread, ThreadSafeConnection> item = exclusoes[idx];
                            ThreadSafeConnection connTemp;

                            this.ConnectionPool.TryRemove(item.Key, out connTemp);
                            item.Value.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
            });
        }

        /// <summary>
        /// Inicializa o serviço e retorna uma única instância criada de ConnectionManager.
        /// Este método segue o conceito de singleton.
        /// </summary>
        /// <param name="ConnectionString">String de conexão com o banco de dados (o mesmo usado para criar uma SqlConnection).</param>
        /// <param name="StartingSqlCommands">Lista de comandos sql que serão executados ao iniciar a conexão com o banco de dados.</param>
        /// <returns>Retorna uma instância de ConnectionManager.</returns>
        public static ConnectionManager CreateInstance(string connectionString, params string[] initialCommands)
        {
            //connectionString = connectionString + ";pooling =false;";

            ConnectionManager manager = null;

            lock (BLOQUEIO)
            { //--> um mesmo lock para todas Threads
                manager = new ConnectionManager(connectionString, initialCommands);

                manager.Connection.ExecuteNonQuery("select top 1 * from sysobjects");    //testando a conexão
            }

            return manager;
        }

        public void Reconnect()
        {
            lock (ConnectionManager.BLOQUEIO)
            {
                Connection.Reconnect();
            }
        }

        /// <summary>
        /// Fechar todas as conexões.
        /// </summary>
        public void CloseAllConnections()
        {
            lock (ConnectionManager.BLOQUEIO)
            {
                foreach (KeyValuePair<Thread, ThreadSafeConnection> item in this.ConnectionPool)
                {
                    try
                    {
                        item.Value.Close();
                    }
                    catch (Exception)
                    {
                    }
                }

            }
        }

        /// <summary>
        /// Expurgar todas as conexões.
        /// </summary>
        public void PurgeAllConnections()
        {
            lock (ConnectionManager.BLOQUEIO)
            {

                foreach (KeyValuePair<Thread, ThreadSafeConnection> item in this.ConnectionPool)
                {
                    try
                    {
                        this.PurgeConnection(item.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public ThreadSafeConnection Connection
        {
            get
            {
                lock (Thread.CurrentThread)
                {
                    ThreadSafeConnection connection = null;

                    ClearPool();

                    if (ConnectionPool.ContainsKey(Thread.CurrentThread))
                    {
                        connection = ConnectionPool[Thread.CurrentThread];
                    }
                    else
                    {
                        connection = new ThreadSafeConnection(this, Thread.CurrentThread, ConnectionString, InitialCommands);
                        ConnectionPool[Thread.CurrentThread] = connection;
                    }

                    /*
                    if (!this.ConnectionPool.TryGetConnection(Thread.CurrentThread, out connection))
                    {
                        connection = new ThreadSafeConnection(this, Thread.CurrentThread, this.ConnectionString, this.InitialCommands);
                        this.ConnectionPool.Set(connection);
                    }
                    */
                    return connection;
                }
            }
        }

        private void ClearPool()
        {
            lock (BLOQUEIO)
            {
                List<Thread> deadThreads = new List<Thread>();

                // Separando dead threads:
                foreach (KeyValuePair<Thread, ThreadSafeConnection> pair in ConnectionPool)
                {
                    if (pair.Key == null || !pair.Key.IsAlive)
                    {
                        deadThreads.Add(pair.Key);
                    }
                }

                // Removendo dead threads do pool:
                foreach (Thread thread in deadThreads)
                {
                    ThreadSafeConnection connTemp;
                    ConnectionPool.TryRemove(thread, out connTemp);

                }
            }
        }



        ///// <summary>
        ///// Encerra a instância de ConnectionManager.
        ///// </summary>
        public void Dispose()
        {
            //this.Close();
            ConnectionPool.Clear();
            ConnectionPool = null;
            //INSTANCE = null;
            //System.GC.Collect();

            //this.Dispose();
        }


        public void PurgeConnection(ThreadSafeConnection connection)
        {
            try
            {
                if (ConnectionPool is not null)
                {
                    if (connection.CurrentThread is not null)
                    {
                        ThreadSafeConnection connTemp;
                        ConnectionPool.TryRemove(connection.CurrentThread, out connTemp);
                    }
                }
            }
            catch { }
        }


        ~ConnectionManager()
        {
            DISPOSED = true;
            // Simply call Dispose(false).
            Dispose();
        }
    }
}
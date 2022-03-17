using System;
using System.Collections.Generic;
using System.Threading;


namespace Connections
{

    /*
     * 
     * ---- NÃO ESTÁ SENDO UTILIZADO ----
     * 
     * 
     */


    public class WeakConnectionPool
    {
        protected class WeakConnectionPoolPair
        {
            public Thread Thread { get; set; }
            public ThreadSafeConnection Connection { get; set; }

            public WeakConnectionPoolPair()
            {
                Thread = null;
                Connection = null;
            }
        }

        private object LOCK = new object();
        private List<WeakReference<WeakConnectionPoolPair>> WeakList { get; set; }
        private int _releasedCounter = 0;

        public int ReleasedCounter
        {
            get
            {
                lock (LOCK)
                {
                    return _releasedCounter;
                }
            }
            private set
            {
                lock (LOCK)
                {
                    _releasedCounter = value;
                }
            }
        }


        public WeakConnectionPool()
        {
            ReleasedCounter = 0;
            WeakList = new List<WeakReference<WeakConnectionPoolPair>>();
        }

        public int CallGarbageCollector()
        {
            GC.Collect();
            return -1;
        }

        public bool TryGetConnection(Thread thread, out ThreadSafeConnection connection)
        {
            lock (LOCK)
            {
                bool itIsTimeToRelease = false;
                bool returnValue = false;
                connection = null;

                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        if (object.ReferenceEquals(thread, strongReferenceFound.Thread))
                        {
                            connection = strongReferenceFound.Connection;
                            returnValue = true;
                            break;
                        }
                    }
                    else
                    {
                        itIsTimeToRelease = true;
                    }
                }

                if (itIsTimeToRelease)
                {
                    ReleaseWeakReferences();
                }

                return returnValue;
            }
        }

        public bool TryGetThread(ThreadSafeConnection connection, out Thread thread)
        {
            lock (LOCK)
            {
                bool itIsTimeToRelease = false;
                bool returnValue = false;
                thread = null;

                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        if (object.ReferenceEquals(connection, strongReferenceFound.Connection))
                        {
                            thread = strongReferenceFound.Thread;
                            returnValue = true;
                            break;
                        }
                    }
                    else
                    {
                        itIsTimeToRelease = true;
                    }
                }

                if (itIsTimeToRelease)
                {
                    ReleaseWeakReferences();
                }

                return returnValue;
            }
        }

        public void Set(ThreadSafeConnection newConnection)
        {
            lock (LOCK)
            {
                WeakConnectionPoolPair strongReferenceFound = null;

                ReleaseWeakReferences();


                // Verifica por referências duplicadas, tanto de Thread quanto de ThreadSafeConnection:
                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    strongReferenceFound = null;

                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        if (object.ReferenceEquals(newConnection.CurrentThread, strongReferenceFound.Thread))
                        {
                            break;
                        }
                    }
                }

                if (strongReferenceFound == null)
                {
                    strongReferenceFound = new WeakConnectionPoolPair();
                    strongReferenceFound.Thread = newConnection.CurrentThread;
                    strongReferenceFound.Connection = newConnection;

                    WeakList.Add(new WeakReference<WeakConnectionPoolPair>(strongReferenceFound));
                }
                else
                {
                    strongReferenceFound.Connection = newConnection;
                }
            }
        }

        public void ReleaseWeakReferences()
        {
            lock (LOCK)
            {
                List<WeakReference<WeakConnectionPoolPair>> newWeakList = new List<WeakReference<WeakConnectionPoolPair>>();
                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        newWeakList.Add(weakItem);
                    }
                    else
                    {
                        ReleasedCounter = ReleasedCounter + 1;
                    }
                }

                WeakList = newWeakList;
            }
        }

        public List<ThreadSafeConnection> GetAllConnections()
        {
            List<ThreadSafeConnection> connections = new List<ThreadSafeConnection>();

            lock (LOCK)
            {
                ReleaseWeakReferences();

                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        connections.Add(strongReferenceFound.Connection);
                    }
                }

                return connections;
            }
        }

        public bool Remove(ThreadSafeConnection connection)
        {
            lock (LOCK)
            {
                List<WeakReference<WeakConnectionPoolPair>> newWeakList = new List<WeakReference<WeakConnectionPoolPair>>();
                ReleaseWeakReferences();
                bool isRemoved = false;

                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        if (object.ReferenceEquals(connection, strongReferenceFound.Connection)
                                || object.ReferenceEquals(connection.CurrentThread, strongReferenceFound.Thread)
                            )
                        {
                            isRemoved = true;
                        }
                        else
                        {
                            newWeakList.Add(weakItem);
                        }
                    }
                }

                WeakList = newWeakList;
                return isRemoved;
            }
        }

        public bool Remove(Thread thread)
        {
            lock (LOCK)
            {
                List<WeakReference<WeakConnectionPoolPair>> newWeakList = new List<WeakReference<WeakConnectionPoolPair>>();
                ReleaseWeakReferences();
                bool isRemoved = false;

                WeakConnectionPoolPair strongReferenceFound = null;

                foreach (WeakReference<WeakConnectionPoolPair> weakItem in WeakList)
                {
                    if (weakItem.TryGetTarget(out strongReferenceFound))
                    {
                        if (object.ReferenceEquals(thread, strongReferenceFound.Thread))
                        {
                            isRemoved = true;
                        }
                        else
                        {
                            newWeakList.Add(weakItem);
                        }
                    }
                }

                WeakList = newWeakList;
                return isRemoved;
            }
        }

        public void Clear()
        {
            lock (LOCK)
            {
                WeakList.Clear();
                WeakList = null;
            }
        }

    }

}
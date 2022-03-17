using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;

namespace CoreDll.Utils
{
    public static class LogBasico
    {
        //private LogBasico Default { get; set; }
        private static string DirPath { get; set; }

        public static bool IsFatalActived = true;
        public static bool IsDebugActived = false;
        public static bool IsErrorActived = true;

        public static bool IsInfoActived = true;
        public static bool IsTraceActived = false;
        public static bool IsWarnActived = false;



        private static ConcurrentQueue<string> MessageQueue { get; set; } = new ConcurrentQueue<string>();

        public static void CreateLog(string dirPath = @"./")
        {
            LogBasico.DirPath = Path.Combine(dirPath, "log.txt");
            File.WriteAllText(LogBasico.DirPath, "");
        }

        public static void Debug(string className, string methodName, string message)
        {
            if (LogBasico.IsDebugActived)
            {
                LogBasico.QueueMessage($@"debug@{className}.{methodName}:{message}");
            }
        }

        public static void Fatal(string className, string methodName, string message)
        {
            if (LogBasico.IsFatalActived)
            {
                LogBasico.QueueMessage($@"fatal@{className}.{methodName}:{message}");
            }
        }

        public static void Info(string className, string methodName, string message)
        {
            if (LogBasico.IsInfoActived)
            {
                LogBasico.QueueMessage($@"info@{className}.{methodName}:{message}");
            }
        }

        public static void Warn(string className, string methodName, string message)
        {
            if (LogBasico.IsWarnActived)
            {
                LogBasico.QueueMessage($@"warn@{className}.{methodName}:{message}");
            }
        }

        /// <summary>
        /// Para medir tempo de execução de uma função
        /// </summary>
        /// <param name="message"></param>
        public static void Trace(string className, string methodName, string message, Action action)
        {
            if (LogBasico.IsTraceActived)
            {
                if (action == null)
                    return;

                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

                try
                {
                    stopWatch.Start();
                    action();
                    stopWatch.Stop();
                }
                catch (Exception ex)
                {
                    stopWatch.Stop();
                    LogBasico.QueueMessage($@"trace(INTERRUPTED)@{className}.{methodName}[{stopWatch.ElapsedMilliseconds}ms]:{message}");
                    LogBasico.QueueMessage($@"-->error(INTERRUPTION)@{className}.{methodName}:{ex.Message}");
                    throw ex;
                }

                LogBasico.QueueMessage($@"trace@{className}.{methodName}[{stopWatch.ElapsedMilliseconds}ms]:{message}");
            }
        }

        public static void Error(string className, string methodName, string message)
        {
            if (LogBasico.IsErrorActived)
            {
                LogBasico.QueueMessage($@"error@{className}.{methodName}:{message}");
            }
        }

        private static void QueueMessage(string message)
        {
            LogBasico.MessageQueue.Enqueue(message);
            LogBasico.Flush();
        }

        public static void Error(string className, string methodName, Exception ex)
        {
            LogBasico.Error(className, methodName, ex?.Message);
        }

        public static void Fatal(string className, string methodName, Exception ex)
        {
            LogBasico.Fatal(className, methodName, ex?.Message);
        }

        public static void Flush()
        {
            while (LogBasico.MessageQueue.Count > 0)
            {
                string result = null;
                if (LogBasico.MessageQueue.TryDequeue(out result))
                {
                    File.AppendAllLinesAsync(LogBasico.DirPath, new List<string> { result });
                }
            }
        }
    }
}
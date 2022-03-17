

using System;



namespace CoreDll.Threading
{
    // Fake...
    public class GuiTaskSync
    {
        public Action Action { get; private set; }

        public GuiTaskSync(Action action, object formInvoker)
        {
            Action = action;
        }

        public static GuiTaskSync Run(Action action, object formInvoker)
        {
            //CodeExecutingChainingProtected protectedChainPart = new CodeExecutingChainingProtected();
            //CodeExecutingChaining chain = new CodeExecutingChaining(protectedChainPart);

            GuiTaskSync task = new GuiTaskSync(action, formInvoker);
            task.RunSync();
            return task;
        }

        public void RunSync()
        {
            Action();
        }

        /*
        public void RunAsync()
        {
            Task task = Task.Run(() =>
                {
                    this.RunSync();
                });
        }
        */

        /// <summary>
        /// Permite que tarefas sejam executadas entre threads diferentes sem causar problemas na interface.
        /// </summary>
        /// <param name="code">Uma [Action] com o código a ser executado</param>
        public static GuiTaskSync Run(Action action)
        {
            return GuiTaskSync.Run(action, null);
        }
    }

    //    /// <summary>
    //    /// Permite alterar a GUI através de GuiTaskSync.Run(Action action) para evitar inconsistências de sincronização entre a thread principal (que executa a GUI) e outras threads secundárias.
    //    /// *** Lembrando que, pode haver atrasos na atualização da GUI em relação à thread a qual foi chamada. ***
    //    /// </summary>
    //    public class GuiTaskSync
    //    {
    //        public Exception Exception { get; private set; }
    //        public bool IsWorkDone { get; private set; }
    //        public Action Action { get; private set; }
    //        public  Control FormInvoker { get; private set; }
    //        public event EventHandler<Exception> ExceptionOcurredEvent;
    //        //public event EventHandler IsWorkDoneEvent;

    //        /*
    //        private void RunCallback<T>(Action<T> callback)
    //        {
    //            this.RunSync();
    //        }
    //        */

    //        /// <summary>
    //        /// Sinaliza o término do procedimento.
    //        /// </summary>
    //        private void Done()
    //        {
    //            this.IsWorkDone = true;
    //            //this.OnIsWorkDoneEvent();
    //        }

    //        public GuiTaskSync(Action action, Form formInvoker)
    //        {
    //            this.IsWorkDone = false;
    //            this.Action = action;
    //            this.FormInvoker = formInvoker;
    //            this.Exception = null;
    //            //this.PropagarErros = false;
    //        }

    //        /// <summary>
    //        /// Espera o procedimento da thread principal concluir (a thread da interface) para continuar a execução.
    //        /// </summary>
    //        public void WaitForDone()
    //        {
    //            while (!this.IsWorkDone)
    //            {
    //                Thread.Yield();
    //            }
    //        }

    //        public void RunSync()
    //        {
    //            try
    //            {
    //                if (this.FormInvoker != null)
    //                {
    //                    if (this.FormInvoker.InvokeRequired)
    //                    {
    //                        this.FormInvoker.Invoke(this.Action);
    //                    }
    //                    else
    //                    {
    //                        this.Action();
    //                    }
    //                }
    //                else
    //                {
    //                    this.Action();
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                this.Exception = ex;
    //                this.OnExceptionOcurredEvent(ex);
    //            }

    //            this.Done();
    //        }

    //        /*
    //        public void RunAsync()
    //        {
    //            Task task = Task.Run(() =>
    //                {
    //                    this.RunSync();
    //                });
    //        }
    //        */

    //        /// <summary>
    //        /// Permite que tarefas sejam executadas entre threads diferentes sem causar problemas na interface.
    //        /// </summary>
    //        /// <param name="code">Uma [Action] com o código a ser executado</param>
    //        /// <param name="invokerForm">Formulário cujo thread o código deverá ser executado</param>
    //        /// <returns></returns>
    //        public static GuiTaskSync Run(Action action, Form formInvoker)
    //        {
    //            //CodeExecutingChainingProtected protectedChainPart = new CodeExecutingChainingProtected();
    //            //CodeExecutingChaining chain = new CodeExecutingChaining(protectedChainPart);

    //            GuiTaskSync task = new GuiTaskSync(action, formInvoker);

    //            task.RunSync();

    //            return task;
    //        }

    //        /// <summary>
    //        /// Permite que tarefas sejam executadas entre threads diferentes sem causar problemas na interface.
    //        /// </summary>
    //        /// <param name="code">Uma [Action] com o código a ser executado</param>
    //        public static GuiTaskSync Run(Action action)
    //        {
    //            Form formInvoker = null;

    //            if (Application.OpenForms.Count > 0)
    //            {
    //                formInvoker = Application.OpenForms[0];
    //            }

    //            return GuiTaskSync.Run(action, formInvoker);
    //        }


    //        /*
    //        public static GuiTaskSync RunWithCallback<T>(Func<T> action, Action<T> callback)
    //        {
    //            Form formInvoker = null;

    //            if (Application.OpenForms.Count > 0)
    //            {
    //                formInvoker = Application.OpenForms[0];
    //            }

    //            GuiTaskSync task = new GuiTaskSync(action, formInvoker);

    //            task.RunAsync();

    //            return task;

    //            T result = action();
    //            callback(result);
    //        }
    //        */

    //        private void OnExceptionOcurredEvent(Exception ex)
    //        {
    //            if (ExceptionOcurredEvent != null)
    //            {
    //                ExceptionOcurredEvent(this, ex);
    //            }
    //        }

    //        /*
    //        private void OnIsWorkDoneEvent()
    //        {
    //            if (IsWorkDoneEvent != null)
    //            {
    //                IsWorkDoneEvent(this, new EventArgs());
    //            }
    //        }
    //        */
    //    }

}

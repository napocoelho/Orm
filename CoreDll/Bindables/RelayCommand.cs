using System;
using System.Windows.Input;


namespace CoreDll.Bindables
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        private readonly WeakEventHandlerSource CanExecuteChangedSource = new WeakEventHandlerSource();

        public event EventHandler CanExecuteChanged
        {
            /*
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
            */
            add => CanExecuteChangedSource.Subscribe(value);
            remove => CanExecuteChangedSource.Unsubscribe(value);
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                execute(parameter);
            }
        }
    }
}
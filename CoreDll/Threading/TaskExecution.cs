using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel;



namespace CoreDll.Threading
{
    public class TaskExecution<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SafeValue<TaskExecutionStatus> StatusValue { get; set; } = new SafeValue<TaskExecutionStatus>();
        public TaskExecutionStatus Status
        {
            get { return StatusValue.Value; }
            private set { StatusValue.SetValue(() => value); OnPropertyChange(nameof(Status)); }
        }

        //public async ValueTask<TaskExecutionStatus> GetStatus()
        //{
        //    return await Task.Run<TaskExecutionStatus>(() => Status).ConfigureAwait(true);
        //}

        private System.Collections.Concurrent.ConcurrentQueue<T> InputQueue { get; set; } = new System.Collections.Concurrent.ConcurrentQueue<T>();
        private List<TaskInfo<T>> ExecutionTasks { get; set; } = new List<TaskInfo<T>>();

        public System.Collections.Concurrent.ConcurrentQueue<ExecutionInfo<T>> ResultQueue { get; private set; } = new System.Collections.Concurrent.ConcurrentQueue<ExecutionInfo<T>>();
        private System.Collections.Concurrent.ConcurrentBag<ExecutionInfo<T>> ResultBag { get; set; } = new System.Collections.Concurrent.ConcurrentBag<ExecutionInfo<T>>();

        //public event EventHandler IsCompleted;

        //private void OnIsCompleted()
        //{
        //    this.IsCompleted?.Invoke(this, new EventArgs());
        //}

        public int TotalTasksCount { get; private set; }
        public int CompletedTasksCount { get => ResultBag.Count; }
        public int ExecutingTasksCount { get; private set; }
        public ExecutionInfo<T>[] CompletedItems { get => ResultBag.ToArray(); }

        public void Stop()
        {
            if (Status == TaskExecutionStatus.Running)
            {
                Status = TaskExecutionStatus.Stopping;
            }
        }


        private Action<T> Action { get; set; }
        public int MaxParallelDegree { get; private set; }

        public TaskExecution(Action<T> action, ICollection<T> collection, int maxParallelDegree)
        {
            if (collection is null)
                new ArgumentNullException(nameof(collection));

            Action = action;
            TotalTasksCount = collection.Count;
            MaxParallelDegree = maxParallelDegree;
            Status = TaskExecutionStatus.Stopped;

            foreach (T item in collection)
                InputQueue.Enqueue(item);
        }

        public void Run()
        {
            if (Status != TaskExecutionStatus.Stopped)
                return;

            Status = TaskExecutionStatus.Running;

            Task.Run(() =>
            {

                Status = TaskExecutionStatus.Running;

                while (InputQueue.Count > 0 || ExecutionTasks.Count > 0)
                {
                    try
                    {
                        // Inicia uma nova tarefa:
                        while (ExecutionTasks.Count <= MaxParallelDegree && InputQueue.Count > 0)
                        {
                            if (Status == TaskExecutionStatus.Stopping || Status == TaskExecutionStatus.Stopped)
                                break;

                            T item;

                            if (InputQueue.TryDequeue(out item))
                            {
                                Task newTask = Task.Run(() => Executor(item));

                                TaskInfo<T> taskInfo = new TaskInfo<T> { Task = newTask, InputValue = item };
                                ExecutionTasks.Add(taskInfo);
                                ExecutingTasksCount = ExecutionTasks.Count;
                            }
                        }

                        //Task.WhenAny(this.ExecutionTasks.Select(infoTask => infoTask.Task));

                        while (!ExecutionTasks.Any(infotask => infotask.IsCompleted))
                        {
                            Task.Delay(50);
                        }

                        foreach (TaskInfo<T> removableInfoTask in ExecutionTasks.Where(infoTask => infoTask.IsCompleted).ToArray())
                        {
                            ExecutionTasks.Remove(removableInfoTask);

                            ExecutionInfo<T> info = new ExecutionInfo<T> { Exception = removableInfoTask.Task.Exception, InputValue = removableInfoTask.InputValue };
                            ResultQueue.Enqueue(info);
                            ResultBag.Add(info);
                            ExecutingTasksCount = ExecutionTasks.Count;

                            //GC.Collect();
                        }



                        if ((Status == TaskExecutionStatus.Stopping || Status == TaskExecutionStatus.Stopped) && ExecutionTasks.Count == 0)
                            break;
                    }
                    catch (Exception)
                    {

                    }
                }

                ExecutingTasksCount = 0;

                if (Status == TaskExecutionStatus.Stopping)
                {
                    Status = TaskExecutionStatus.Stopped;
                }
                else
                {
                    //this.OnIsCompleted();
                    Status = TaskExecutionStatus.Completed;
                }

            });

        }

        private async ValueTask Executor(T item)
        {
            Action(item);
        }

        private void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(propertyName)));
        }
    }

    public class TaskInfo<T>
    {
        public T InputValue { get; set; }
        public Task Task { get; set; }

        public bool IsCompleted { get => Task?.IsCompleted ?? true; }
    }

    public class ExecutionInfo<T>
    {
        public Exception Exception { get; set; }
        public T InputValue { get; set; }
    }

    public enum TaskExecutionStatus
    {
        Running,
        Stopping,
        Stopped,
        Completed
    }
}
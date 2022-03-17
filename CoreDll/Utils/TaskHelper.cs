using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;

namespace CoreDll.Utils
{
    public static class TaskHelper
    {
        public async static ValueTask ProcessManyAtOnce<T>(ICollection<T> items, Func<T, Task> func, int maxDegreeOfParallelism = 20)
        {
            ExecutionDataflowBlockOptions maxParallelismDegree = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

            ActionBlock<T> actionBlock = new ActionBlock<T>(func, maxParallelismDegree);

            foreach (T item in items)
            {

                await actionBlock.SendAsync(item);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
        }
    }




}
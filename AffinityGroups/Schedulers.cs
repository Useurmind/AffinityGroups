using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ThreadAffineObjects
{
    /// <summary>
    /// Helper functions for working with the different kinds of schedulers.
    /// </summary>
    public static class Schedulers
    {
        /// <summary>
        /// Retrieves the task scheduler for a giving dispatcher.
        /// </summary>
        /// <param name="dispatcher">The dispatcher for which a task scheduler is required.</param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static Task<TaskScheduler> ToTaskSchedulerAsync(
            this Dispatcher dispatcher,
            DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<TaskScheduler>();
            var invocation = dispatcher.BeginInvoke(new Action(() =>
                {
                    taskCompletionSource.SetResult(TaskScheduler.FromCurrentSynchronizationContext());
                }),
                priority);

            invocation.Aborted += (s, e) => taskCompletionSource.SetCanceled();

            return taskCompletionSource.Task;
        }
    }
}

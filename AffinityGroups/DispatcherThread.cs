using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ThreadAffineObjects
{
    /// <summary>
    /// A class that automatically creates a dispatcher on a thread and offers 
    /// means to wait for the dispatcher becoming ready.
    /// It can also be used to stop a thread via its <see cref="Dispose"/> funtion.
    /// </summary>
    public class DispatcherThread : IDisposable
    {
        private TaskCompletionSource<Dispatcher> dispatcherTaskCompletionSource;
        private TaskCompletionSource<>

        /// <summary>
        /// Task that is done as soon as the dispatcher on this thread
        /// was created.
        /// </summary>
        public Task<Dispatcher> DispatcherTask { get; private set; }

        /// <summary>
        /// The underlying thread (because the Thread class is sealed we cannot inherit).
        /// </summary>
        public Thread Thread { get; private set; }

        /// <summary>
        /// Creates a thread that starts a dispatcher on startup.
        /// </summary>
        public DispatcherThread() : this(null, 0)
        {

        }

        /// <summary>
        /// Creates a thread that starts a dispatcher on startup.
        /// </summary>
        /// <param name="threadStart">An action that is executed before the dispatcher is started.</param>
        public DispatcherThread(Action threadStart) : this(threadStart, 0)
        {

        }

        /// <summary>
        /// Creates a thread that starts a dispatcher on startup.
        /// </summary>
        /// <param name="threadStart">An action that is executed before the dispatcher is started.</param>
        /// <param name="maxStackSize">Same as for Thread constructor, maximum stack size in byte, or zero for maximum default stack size.</param>
        public DispatcherThread(Action threadStart, int maxStackSize)
        {
            this.dispatcherTaskCompletionSource = new TaskCompletionSource<Dispatcher>();
            this.DispatcherTask = this.dispatcherTaskCompletionSource.Task;

            Action threadStartAction = () =>
            {
                try
                {
                    if (threadStart != null)
                    {
                        threadStart();
                    }

                    var d = Dispatcher.CurrentDispatcher;

                    this.dispatcherTaskCompletionSource.SetResult(d);

                    Dispatcher.Run();
                }
                catch (Exception e)
                {
                    this.dispatcherTaskCompletionSource.SetException(e);
                }
            };

            var threadStartInternal = new ThreadStart(threadStartAction);
            this.Thread = new Thread(threadStartInternal, maxStackSize);

        }

        /// <summary>
        /// Start the thread and its dispatcher.
        /// </summary>
        public void Start()
        {
            this.Thread.Start();
        }

        /// <summary>
        /// Stops the dispatcher and the thread.
        /// The dispatcher is stopped synchronously and afterwards the thread is joined.
        /// </summary>
        public void Stop()
        {
            this.DispatcherTask.Result.InvokeShutdown();
            this.Thread.Join();
        }

        /// <summary>
        /// Begins to stop the dispatcher.
        /// Afterwards you can 
        /// </summary>
        public void StopAsync()
        {
            this.DispatcherTask.Result.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        /// <summary>
        /// Join this thread.
        /// </summary>
        public void Join()
        {
            this.Thread.Join();
        }

        /// <summary>
        /// Stops the dispatcher and the thread.
        /// The dispatcher is stopped synchronously and afterwards the thread is joined.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }
    }
}

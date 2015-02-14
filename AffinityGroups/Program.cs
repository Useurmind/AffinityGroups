using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Windows.Threading;

using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace ThreadAffineObjects
{
    interface IMyThreadAffineObject
    {
        int TestProperty { get; set; }
    }

    class MyThreadAffineObject : IMyThreadAffineObject
    {
        private int testProperty;
        public int TestProperty
        {
            get
            {
                MyConsole.WriteWithThread("Get_TestProperty={0}", testProperty);
                return testProperty;
            }
            set
            {
                MyConsole.WriteWithThread("Set_TestProperty={0}", value);
                testProperty = value;
            }
        }
    }

    class MyThreadAffineProxy : ThreadAffineObject, IMyThreadAffineObject
    {
        private IMyThreadAffineObject proxiedObject;

        private ISubject<int> testPropertySubject;

        public MyThreadAffineProxy(Dispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
            this.Dispatcher.Invoke(() => proxiedObject = new MyThreadAffineObject());
        }

        public ISubject<int> TestPropertySubject
        {
            get
            {
                if (testPropertySubject == null)
                {
                    return this.DispatchAndWait(() =>
                    {
                        if (testPropertySubject == null)
                        {
                            testPropertySubject = new ReplaySubject<int>(1);
                            testPropertySubject.OnNext(proxiedObject.TestProperty);
                        }
                        return testPropertySubject;
                    });
                }

                return testPropertySubject;
            }
        }

        public int TestProperty
        {
            get
            {
                return this.DispatchAndWait(() => this.proxiedObject.TestProperty);
            }

            set
            {
                this.Dispatcher.InvokeAsync(() =>
                {
                    proxiedObject.TestProperty = value;
                    if (testPropertySubject != null)
                    {
                        MyConsole.WriteWithThread("OnNext on TestPropertySubject with value {0}", value);
                        testPropertySubject.OnNext(value);
                    }
                });
            }
        }
    }

    class ThreadAffineObject
    {
        public Dispatcher Dispatcher { get; set; }

        protected T DispatchAndWait<T>(Func<T> dispatchedAction)
        {
            var dispatcherOperation = this.Dispatcher.InvokeAsync<T>(dispatchedAction);
            dispatcherOperation.Task.Wait();
            return dispatcherOperation.Task.Result;
        }
    }

    static class MyConsole
    {
        public static void WriteWithThread(string format, params object[] args)
        {
            var thread = string.Format("(thread {0}) ", Thread.CurrentThread.ManagedThreadId);
            var message = string.Format(format, args);
            Console.Write(thread);
            Console.WriteLine(message);
        }
    }

    class Program
    {
        static Dispatcher StartNewThread(Action initialAction)
        {
            Semaphore m = new Semaphore(0, 1);
            var thread = new Thread(new ThreadStart(() =>
            {
                initialAction();
                var currentDispatcher = Dispatcher.CurrentDispatcher;
                m.Release();
                Dispatcher.Run();
            }));

            thread.Start();

            m.WaitOne();

            return Dispatcher.FromThread(thread);
        }

        static void Main(string[] args)
        {
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                MainDispatched();
            });

            Dispatcher.Run();
        }

        static void MainDispatched()
        {
            MyConsole.WriteWithThread("Main started");

            var thread1Dispatcher = StartNewThread(() => MyConsole.WriteWithThread("New thread 1 started"));

            var proxy = new MyThreadAffineProxy(thread1Dispatcher);

            proxy.TestProperty = 1;

            MyConsole.WriteWithThread("TestProperty value is {0}", proxy.TestProperty);

            var mainScheduler = new DispatcherScheduler(Dispatcher.CurrentDispatcher);
            var thread1Scheduler = new DispatcherScheduler(thread1Dispatcher);

            proxy.TestPropertySubject.SubscribeOn(mainScheduler)
                                     .Subscribe(val =>
                                     {
                                         MyConsole.WriteWithThread("TestProperty changed to {0}", val);
                                     });

            proxy.TestProperty = 2;
        }
    }
}

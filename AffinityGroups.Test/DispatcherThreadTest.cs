using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadAffineObjects;

namespace AffinityGroups.Test
{
    [TestFixture]
    class DispatcherThreadTest : IOfferConcurrencyStressTest
    {
        private DispatcherThread dispatcherThread;

        [SetUp]
        public void Setup()
        {
            this.dispatcherThread = new DispatcherThread();
        }

        [TearDown]
        public void Teardown()
        {
            this.dispatcherThread.Dispose();
        }

        [Test]
        public void Start_AfterDispatcherCreated_EverythingsIsInitialized()
        {
            this.dispatcherThread.Start();

            this.dispatcherThread.DispatcherTask.Wait();

            Assert.AreEqual(ThreadState.Running, this.dispatcherThread.Thread.ThreadState);
            Assert.IsTrue(this.dispatcherThread.DispatcherTask.IsCompleted);
            Assert.IsNotNull(this.dispatcherThread.DispatcherTask.Result);
        }

        [Test]
        [Category(Categories.ConcurrencyStress)]
        public void ConcurrencyStress_Start_AfterDispatcherCreated_ThreadRunning()
        {
            this.StressTest(this.Start_AfterDispatcherCreated_EverythingsIsInitialized);
        }

        [Test]
        public void Stop_AfterDispatcherCreated_ThreadStopped()
        {
            this.dispatcherThread.Start();

            this.dispatcherThread.DispatcherTask.Wait();

            this.dispatcherThread.Stop();

            Assert.AreEqual(ThreadState.Stopped, this.dispatcherThread.Thread.ThreadState);
        }

        [Test]
        [Category(Categories.ConcurrencyStress)]
        public void ConcurrencyStress_Stop_AfterDispatcherCreated_ThreadStopped()
        {
            this.StressTest(Stop_AfterDispatcherCreated_ThreadStopped);
        }
    }
}

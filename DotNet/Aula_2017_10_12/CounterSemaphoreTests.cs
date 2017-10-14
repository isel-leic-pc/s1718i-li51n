using NUnit.Framework;
using System.Threading;

namespace Aula_2017_10_12 {
    [TestFixture]
    class CounterSemaphoreTests {

        private class Args {
            internal CounterSemaphoreEN sem;
            internal int reqUnits;
            internal bool memInterrupt;
            internal bool interrupted;

            internal Args(CounterSemaphoreEN s, int n, bool mem) {
                sem = s;
                reqUnits = n;
                memInterrupt = mem;
                interrupted = false;
            } 
        }
      
        private void ThreadFunc(object _args) {
            Args args = (Args) _args;
            try {
                args.sem.Acquire(args.reqUnits);
            }
            catch (ThreadInterruptedException) {
                if (args.memInterrupt)
                    args.interrupted = true;
            }
        }

        [Test]
        public void CheckRemoveRequestOnTimeout() {
            CounterSemaphoreEN sem = new CounterSemaphoreEN(0);

            bool res = sem.Acquire(1, 1000);
            Assert.IsTrue(!res && sem.Units == 0 && sem.WaitersCount == 0);
        }

        [Test]
        public void CheckFIFO_OnMultipleAcquisition() {
            CounterSemaphoreEN sem = new CounterSemaphoreEN(0);
            Args args1_2 = new Args(sem, 4, false);
            Thread t1 = new Thread(ThreadFunc);
            t1.Start(args1_2);

            Thread t2 = new Thread(ThreadFunc); 
            t2.Start(args1_2);
            // synchronize with the waiting of the threads
            while (sem.WaitersCount != 2) Thread.Sleep(1000);
            Args args3 = new Args(sem, 4, true);
            Thread t3 = new Thread(ThreadFunc); 
            t3.Start(args3);
            // synchronize with the waiting of the last thread
            while (sem.WaitersCount != 3) Thread.Sleep(1000);
            // awake first two threads
            sem.Release(10);
            t1.Join();
            t2.Join();
            
            // check current semaphore state
            bool res = sem.Units == 2 && sem.WaitersCount == 1;

            t3.Interrupt();
            t3.Join();
            Assert.IsTrue(res && args3.interrupted);
        }
    }
}

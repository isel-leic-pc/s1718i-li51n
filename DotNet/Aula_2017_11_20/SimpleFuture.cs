using System;
using System.Threading;

namespace Aula_2017_11_20 {
    class SimpleFuture<T> {
        private object monitor = new object();
        private const int CREATED = 1, RUNNING = 2, COMPLETED = 3, FAULTED = 4;

        // the future state
        private volatile int state;

        // result holders
        private T result;
        private Exception exc;

        // to minimize lock aquisition
        private volatile int waiters;

        private Func<T> builder;

        public SimpleFuture(Func<T> builder) {
            this.builder = builder;
            state = CREATED;
        }

        public void Start() {
            if (Interlocked.CompareExchange(ref state, RUNNING, CREATED) != CREATED)
                throw new InvalidOperationException("Future already started!");

            ThreadPool.QueueUserWorkItem((o) => {
                try {
                    result = builder();
                    state = COMPLETED;

                }
                catch (Exception e) {
                    exc = e;
                    state = FAULTED;
                }

                // the barrier is necessary to avoid reordering 
                // with the previous write of "state" field
                Thread.MemoryBarrier();
                if (waiters > 0) {
                    lock (monitor)
                        Monitor.PulseAll(monitor);

                }

            });
        }

        public int State {
            get { return state; }
        }

        private bool IsValueAvailable() {
            int observed = state;
            if (observed == COMPLETED) return true;
            if (observed == FAULTED)
                throw new Exception("Error execution builder", exc);
            return false;
        }

        public T Result {
            get {
                if (IsValueAvailable()) return result;
                lock (monitor) {
                    try {
                        waiters++;
                        Thread.MemoryBarrier();

                        while (!IsValueAvailable())
                            Monitor.Wait(monitor);

                        return result;
                    }
                    finally {
                        waiters--;
                    }
                }
            }
        }
    }
}

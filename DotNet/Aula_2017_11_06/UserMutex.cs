using System;
 
using System.Threading;

namespace Aula_2017_11_06 {
    class UserMutex {
        private int OwnerId;
        private AutoResetEvent Waiter;
        private volatile int Count;
        private int RecurseCount;
        private readonly int spinCount = 1000;
        public UserMutex() {
            Waiter = new AutoResetEvent(false);
        }

        public void Acquire() {
            if (Thread.CurrentThread.ManagedThreadId == OwnerId) {
                RecurseCount++;
                return;
            }
            bool acquired = false;
            for (int i = 0; i < spinCount; ++i) {
                if (Count == 0 && Interlocked.CompareExchange(ref Count, 1, 0) == 0) {
                    acquired = true; break;
                }
            }
            if (!acquired) {
                if (Interlocked.Increment(ref Count) > 1)
                    Waiter.WaitOne();
            }

            RecurseCount = 1;
            OwnerId = Thread.CurrentThread.ManagedThreadId;

        }

        public void Release() {
            if (OwnerId != Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException();
            RecurseCount--;
            if (RecurseCount == 0) {
                OwnerId = 0;
                if (Interlocked.Decrement(ref Count) > 0) {
                    Waiter.Set();
                }

            }
        }
    }
}

using System;
using System.Threading;
using Utils;

namespace Exercicios1 {
    public class FutureHolder<T> where T : class {
        private Object monitor = new Object();
        private T value;

        public void SetValue(T val) {
            lock(monitor) {
                if (value != null || val == null)
                    throw new InvalidOperationException("Value already exists or is null!");
                value = val;
                Monitor.PulseAll(monitor);
            }
        }

        public T Get(int timeout)   {
            lock(monitor) {
                if (value != null) return value;
                if (timeout == 0) return null;
                do {
                    int refTime = Environment.TickCount;
                    Monitor.Wait(monitor,timeout);
                    if (value != null) return value;
                    timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                    if (timeout == 0) return null;
                }
                while (true);
            }
        }

        public bool isValueAvailable() {
            lock(monitor) {
                return value != null;
            }
        }

    }

}

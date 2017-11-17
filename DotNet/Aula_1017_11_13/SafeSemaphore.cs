using System;
using System.Threading;

namespace Aula_1017_11_13 {

    public class UnsafeSemaphore {
        private readonly int maxPermits;
        private volatile int permits;

        public UnsafeSemaphore(int initial, int maximum) {
            if (initial < 0 || initial > maximum) throw new InvalidOperationException();
            permits = initial; maxPermits = maximum;
        }
        public bool tryAcquire(int acquires) {
            /*
            if (permits < acquires) return false;
            permits -= acquires;
            return true;

            */
            int observed;
            do {
                observed = permits;
                if (observed < acquires) return false;
            }
            while (Interlocked.CompareExchange(ref permits, observed - acquires,
                    observed) != observed);
            return true;
        }
        public void release(int releases) {
            /*
            if (permits + releases < permits || permits + releases > maxPermits)
                throw new InvalidOperationException();
            permits += releases;
            */
            int observed;
            do {
                observed = permits;
                if (observed + releases < observed || observed + releases > maxPermits)
                    throw new InvalidOperationException();
            }
            while (Interlocked.CompareExchange(ref permits, observed + releases,
                    observed) != observed);
            
        }
    }
    public class SafeSemaphore {
        private readonly int maxPermits;
        private volatile int permits;

        public SafeSemaphore(int initial, int maximum) {
            if (initial < 0 || initial > maximum) throw new InvalidOperationException();
            permits = initial; maxPermits = maximum;
        }
        public bool tryAcquire(int acquires) {
            int observed;
            do {
                observed = permits;
                if (observed < acquires) return false;

            }
            while (Interlocked.CompareExchange(ref permits, observed - acquires, observed) != observed);
            return true;
        }

        public void release(int releases) {
            /*
            if (permits + releases < permits || permits + releases > maxPermits)
                throw new InvalidOperationException();
            permits += releases;
            */
        }
    }
}

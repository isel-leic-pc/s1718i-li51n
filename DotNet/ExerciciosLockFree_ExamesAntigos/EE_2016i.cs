using System;
using System.Threading;

namespace ExerciciosLockFree_ExamesAntigos {
    class EE_2016i {
        /*
         * Esta implementação reflete a semântica de sincronização de um semáforo, 
         * contudo não é thread-safe.Implemente em Java ou em C#, sem utilizar locks, 
         * uma versão thread-safe deste sincronizador.
         */
        public class UnsafeSemaphore {
            private int maxPermits, permits;
            public UnsafeSemaphore(int initial, int maximum) {
                if (initial < 0 || initial > maximum) throw new InvalidOperationException();
                permits = initial; maxPermits = maximum;
            }
            public bool tryAcquire(int acquires) {
                if (permits < acquires) return false;
                permits -= acquires;
                return true;
            }
            public void release(int releases) {
                if (permits + releases < permits || permits + releases > maxPermits)
                    throw new InvalidOperationException();
                permits += releases;
            }
        }


        // versão thread-safe
        public class SafeSemaphore {
            // marked as readonly, volatile is not necessary in this case
            private readonly int maxPermits;

            // marked as volatile to correct publication
            private volatile int permits;
            public SafeSemaphore(int initial, int maximum) {
                if (initial < 0 || initial > maximum)
                    throw new InvalidOperationException();
                permits = initial; maxPermits = maximum;
            }
            public bool tryAcquire(int acquires) {
                int obs;
                do {
                    obs = permits;

                    if (obs < acquires)
                        // the observation doen't allow success
                        return false;
                }
                while (Interlocked.CompareExchange(ref permits, obs - acquires, obs) != obs);
                return true;
            }

            public void release(int releases) {
                int obs;
                do {
                    obs = permits;
                    if (obs + releases < obs || obs + releases > maxPermits)
                        throw new InvalidOperationException();

                }
                while (Interlocked.CompareExchange(ref permits, obs + releases, obs) != obs);

            }
        }
    }
}

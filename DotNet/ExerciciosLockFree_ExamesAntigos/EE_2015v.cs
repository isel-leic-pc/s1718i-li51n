using System;
using System.Threading;

namespace ExerciciosLockFree_ExamesAntigos {
    class EE_2015v {
        /**
        * Esta implementação reflete a semântica de sincronização de um read/write lock, 
        * contudo não é threadsafe. Implemente em Java ou em C #, sem utilizar locks, 
        * uma versão threadsafe deste sincronizador.
        */
        class UnsafeReadWriteLock {
            private int state;
            public void lockWrite() {
                while (state != 0) Thread.Yield();
                state = 1;
            }
            public void unlockWrite() { state = 0; }
            public void lockRead() {
                while (state < 0) Thread.Yield();
                state++;
            }
            public void unlockRead() {
                state--;
            }
        }

        /**
         * Implementação thread safe do ReadWriteLock
         */
        class SafeReadWriteLock {
            // to correct publishing the variable is marked as volatile
            private volatile int state;

            /// <summary>
            /// This is the classic lock aquisition
            /// </summary>
            public void lockWrite() {
                while (Interlocked.CompareExchange(ref state, 1, 0) != 0)
                    Thread.Yield();
            }

            /// <summary>
            /// Just publish 0 on state
            /// </summary>
            public void unlockWrite() {
                state = 0;
            }

            /// <summary>
            /// A reader is comming. Increment state if possible
            /// </summary>
            public void lockRead() {
                do {
                    int obs = state;
                    if (obs < 0) Thread.Yield();
                    else if (Interlocked.CompareExchange(ref state, obs + 1, obs) == obs)
                        return;
                }
                while (true);
            }

            /// <summary>
            /// This implementation check for an invalid call, i.e., the state mus be positive
            /// on a legal call
            /// </summary>
            public void unlockRead() {
                int obs;
                do {
                    obs = state;
                    if (obs <= 0)
                        throw new InvalidOperationException();
                }
                while ((Interlocked.CompareExchange(ref state, obs - 1, obs) != obs));
            }
        }
    }
}

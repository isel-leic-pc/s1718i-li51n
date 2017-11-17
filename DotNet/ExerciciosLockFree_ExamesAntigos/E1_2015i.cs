using System;
 
using System.Threading;

namespace ExerciciosLockFree_ExamesAntigos {
    class E1_2015i {
        /*
         * A implementação deste sincronizador, cuja semântica de sincronização é idêntica à do tipo 
         * Lazy<T> do . NET Framework, não é threadsafe. Sem utilizar locks, 
         * implemente uma versão threadsafe deste sincronizador.
         */
        public class UnsafeSpinLazy<T> where T : class {
            private const int UNCREATED = 0, BEING_CREATED = 1, CREATED = 2;

            private int state = UNCREATED;
            private Func<T> factory;
            private T value;
            public UnsafeSpinLazy(Func<T> factory) { this.factory = factory; }
            public bool IsValueCreated { get { return state == CREATED; } }
            public T Value {
                get {
                    SpinWait sw = new SpinWait();
                    do {
                        if (state == CREATED) {
                            break;
                        }
                        else if (state == UNCREATED) {
                            state = BEING_CREATED; value = factory(); state = CREATED; break;
                        }
                        sw.SpinOnce();
                    } while (true);
                    return value;
                }
            }
        }

        public class SafeSpinLazy<T> where T : class {
            private const int UNCREATED = 0, BEING_CREATED = 1, CREATED = 2;

            // the state must be marke as volatile to ensure correct publication
            private volatile int state = UNCREATED;

            // the factory and value fields need not be marked as volatile
            // since synchronization is piggybacked by state 
            private Func<T> factory;
            private T value;
            public SafeSpinLazy(Func<T> factory) { this.factory = factory; }
            public bool IsValueCreated {
                get { return state == CREATED; }
            }
            public T Value {
                get {
                    SpinWait sw = new SpinWait();
                    do {
                        int obs = state;
                        if (obs == CREATED) {
                            return value;
                        }
                        else if (obs == UNCREATED &&
                             Interlocked.CompareExchange(ref state, BEING_CREATED, obs) == obs) {
                            value = factory();
                            state = CREATED;
                            return value;
                        }
                        sw.SpinOnce();
                    } while (true);
                }
            }
        }
    }
 
}

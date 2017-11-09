using System;
 
using System.Threading;

namespace Aula_2017_11_06 {

    /// <summary>
    /// An immutable stack. Each push/pop operation creates a new stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ImmutableStack<T> {
        private static ImmutableStack<T> empty = new ImmutableStack<T>();

        private readonly T head;
        private readonly ImmutableStack<T> tail;

        /// <summary>
        /// This auxiliary function do an atomic change to a shared stack reference
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="transformer"></param>
        public static void UpdateRef(ref ImmutableStack<T> loc, 
            Func<ImmutableStack<T>, ImmutableStack<T>> transformer) {
            ImmutableStack<T> observed, newStack;
            do {
                observed = loc;
                newStack = transformer(observed);

            }
            while (Interlocked.CompareExchange(ref loc, newStack, observed) == observed);
        }


        // Normally initial state of a stack is defined by factory methods
        // like this one
        public static ImmutableStack<T> CreateNew(T item) {
            return new ImmutableStack<T>(item);
        }


        // the cosntructors are private!
        private ImmutableStack(T item) {
            head = item;
            tail = empty;
        }

        private ImmutableStack() {
            tail = null;
        }

        private ImmutableStack(T item, ImmutableStack<T> s) {
            head = item;
            tail = s;
        }


        // public operations
        public ImmutableStack<T> Push(T item) {
            return new ImmutableStack<T>(item, this);
        }

        public T Front() {
            if (tail == null) throw new InvalidOperationException("Stack empty");
            return head;
        }

        public ImmutableStack<T> Pop(out T item) {
            if (tail == null) throw new InvalidOperationException("Stack empty");
            item = head;
            return tail;
        }

    }
}

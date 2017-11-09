 
using System.Threading;

namespace Aula_2017_11_02 {
    class LFStack<T> {

        private class Node {
            internal T val;

            // this field was marked as volatile 
            // but this is not really necessary due to "head" field read/write volatile barriers
            // on push & pop operations
            internal Node next;

            public Node(T v, Node n) {
                val = v;
                next = n;
            }

            public Node(T v ) {
                val = v;
                next = null;
            }
        }

        private volatile Node head; 
        // needs to be volatile in order to observe updated values 

        public LFStack(T item) {
            head = new Node(item, null);
        }

        public LFStack() {
            // empty stack
            head = null;
        }

        
        public void Push(T item) {
            Node n = new Node(item), h;
            do {
                h = head; // observe current head
                n.next = h;
            }
            while (Interlocked.CompareExchange(ref head, n, h) != h);
        }

        public bool Pop(out T item) {
            do {
                Node h = head;
                if (h == null) {
                    // empty stack, return failure
                    item = default(T);
                    return false;
                }
                if (Interlocked.CompareExchange(ref head, h.next, h) == h) {
                    // CAS succeeded, an item is returned
                    item = h.val;
                    return true;
                }
            }
            while (true);
        }
    }
}

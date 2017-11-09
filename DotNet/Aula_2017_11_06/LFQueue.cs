 
using System.Threading;

namespace Aula_2017_11_06 {
    class LFQueue<T> {
        private class Node {
            internal T value;
            internal  Node next;

            public Node() {
                value = default(T);
                next = null;
            }
            public Node(T val) {
                value = val;
                next = null;
            }
        }

        private volatile Node head;
        private volatile Node tail;

        public LFQueue() {
            head = tail = new Node();
        }

        public void Put(T item) {
           // to implement!
        }

        public bool Get(out T val) {
            // to implement!
            val = default(T);
            return false;
        }
    }
}

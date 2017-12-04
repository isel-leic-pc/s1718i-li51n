using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aula_2017_11_30 {
    public sealed class UnsafeCLHLock {
        public class CLHNode {
            internal bool succMustWait = true; // The default is to wait for a lock
        }
        private CLHNode tail; // the tail of wait queue; when null the lock is free
        public CLHNode Lock() {
            CLHNode myNode = new CLHNode();
            CLHNode predNode = tail; // insert myNode at tail of queue and get my predecessor
            tail = myNode;
            // If there is a predecessor spin until the lock is free; otherwise we got
            // the lock.
            if (predNode != null) {
                SpinWait sw = new SpinWait();
                while (predNode.succMustWait)
                    sw.SpinOnce();
            }
            return myNode;
        }

        public void Unlock(CLHNode myNode) {
            // If we are the last node on the queue, then try to set tail to null.
            if (tail == myNode)
                tail = null;
            else
                myNode.succMustWait = false; // Grant access to the successor thread .
        }
    }

    public sealed class SafeCLHLock {
        public class CLHNode {
            internal volatile bool succMustWait = true; // The default is to wait for a lock
        }
        private volatile CLHNode tail; // the tail of wait queue; when null the lock is free
        public CLHNode Lock() {
            CLHNode myNode = new CLHNode();
            CLHNode predNode;
            do {
                predNode = tail; // insert myNode at tail of queue and get my predecessor

                if (Interlocked.CompareExchange(ref tail, myNode, predNode)== predNode) break;

            }
            while (true);
              // If there is a predecessor spin until the lock is free; otherwise we got
            // the lock.
            if (predNode != null) {
                SpinWait sw = new SpinWait();
                while (predNode.succMustWait)
                    sw.SpinOnce();
            }
            return myNode;
        }

        public void Unlock(CLHNode myNode) {
            // If we are the last node on the queue, then try to set tail to null.
            if (Interlocked.CompareExchange(ref tail, null, myNode) != myNode)
                myNode.succMustWait = false; // Grant access to the successor thread .
        }
    }
}

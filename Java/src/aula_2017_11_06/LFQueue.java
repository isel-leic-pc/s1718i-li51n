package aula_2017_11_06;

import java.util.concurrent.atomic.AtomicReference;

/**
 *
 * A lock free queue based on Michael-Scott algorithm
 * @param <T>
 */
public class LFQueue<T> {

    private class Node {
        T item;
        AtomicReference<Node> next;

        public Node() {
            next = new AtomicReference<>(null);
        }

        public Node(T item) {
            this();
            this.item = item;
        }
    }

    private AtomicReference<Node> head;
    private AtomicReference<Node> tail;

    public LFQueue() {
        // we must mantain a sentinel node in order
        // that "Put" operations never need to update "head"
        // and "Get" operations need need to update tail!
        Node n = new Node();
        head =  new AtomicReference<>(n);
        tail =  new AtomicReference<>(n);
    }

    public void put(T item) {

        Node newNode = new Node(item);
        do {
            Node currTail = tail.get();
            Node tailNext = currTail.next.get();
            // A last check to verify if the observed state is still valid
            if (currTail == tail.get()) {
                if (tailNext != null) // intermediate state
                    tail.compareAndSet(currTail, tailNext);
                else { // quiescent state
                    if (currTail.next.compareAndSet(
                            tailNext, newNode)) {
                        // new node insertion done!
                        // try update tail
                        tail.compareAndSet(currTail, tailNext);
                        return;
                    }
                }
            }

        } while(true);
    }

    T get() {
        do {
            Node h = head.get();
            AtomicReference<Node> hNextRef = h.next;
            Node hNext = hNextRef.get();

            if (hNext  == null)
                return null;

            // this is the correct solution
            // we advance the head, changing the sentinel node, but what's important is the
            // we have a sentinel node, not the same sentinel node!
            if (head.compareAndSet(h, hNext ))
                return hNext.item;

            // in the class room we write the next commented
            // this was terrible wrong since we remove the last node
            // the queue state gets inconsistent, since the tail doesn't refer the same sentinel node
            // as the head!
            //
            // if (hNextRef.compareAndSet(hNext, hNext.next.get() ))
            //     return hNext.item;
        }
        while(true);
    }


}

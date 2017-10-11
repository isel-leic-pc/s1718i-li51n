package exercicios1;

import java.util.LinkedList;
import java.util.List;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

/**
 * Created by jmartins on 09/10/2017.
 */
public class Combiner<L,R>  {
    private Object monitor = new Object(); // the monitor object

    private List<Pair> pairs; // queue for incompleted pairs
    private List<Object> waiters; // queue for pair waiters

    /**
     * auxiliary function to identify a completed pair
     * @return true if the front pair is completed, false otherwise
     */
    private boolean pairCompleted() {
        return  pairs.size() > 0    && pairs.get(0).isCompleted();
    }

    public Combiner() {
        pairs = new LinkedList<Pair>();
        waiters = new LinkedList<Object>();
    }

    /**
     * Inner class for pair holding
     */
    private class Pair {
        private L left;
        private R right;

        private Pair addLeft(L  left ){ this.left = left; return this; }
        private Pair addRight(R  left ){ this.right = right; return this; }

        private boolean isCompleted() {
            return left != null && right != null;
        }

        public L getLeft() { return left; }
        public R getRight() { return right; }
    }


    public void PutLeft(L left) {
        synchronized(monitor) {
            Pair p;
            // check if the front pair can be completed!
            if (pairs.size() > 0) {
                p = pairs.get(0);
                if (p.left == null) {
                    p.left = left;
                    monitor.notifyAll(); // the front pair is completed!
                    return;
                }
            }

            // if not, insert a new pair on the list
            pairs.add(new Pair().addLeft(left));

        }
    }

    public void PutRight(R right) {
        synchronized(monitor)
        {
            // check if the front pair can be completed!
            if (pairs.size() > 0)  {
                Pair p = pairs.get(0);
                if (p.right == null)
                {
                    p.right = right;
                    monitor.notifyAll(); // the front pair is completed!
                    return;
                }
            }

            // insert a new pair on the list
            pairs.add(new Pair().addRight(right));
        }
    }


    public Pair Take(long timeout)throws  InterruptedException
    {
        synchronized(monitor)
        {
            // fast path
            if (waiters.size() == 0 /* avoid barging */ && pairCompleted() )
                return  pairs.remove(0);
            if (timeout == 0)
                return null;

            // prepare wait
            Object req = new Object();
            waiters.add(req);
            do
            {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (waiters.get(0) == req && pairCompleted())
                        return pairs.remove(0);

                    timeout =  remainingTimeout(refTime, timeout);
                    if (timeout == 0)  {
                        // abort wait
                        waiters.remove(req);
                        return null;
                    }
                }
                catch( InterruptedException e) {
                    // abort wait
                    waiters.remove(req);
                    throw e;
                }
            }
            while (true);
        }
    }
}
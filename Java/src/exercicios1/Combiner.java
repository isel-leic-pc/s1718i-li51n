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

    private List<L> lOffers;    // queue for L offers
    private List<R> rOffers;    // queue for R offers

    private List<Object> waiters;   // pair Waiters
    private List<Pair> pairs;       // completed pairs

    public Combiner() {
        lOffers = new LinkedList<>();
        rOffers = new LinkedList<>();
        waiters = new LinkedList<>();
        pairs =   new LinkedList<>();
    }

    /**
     * Inner class for pair holding
     */
    public class Pair {
        public final L left;
        public final R right;

        private Pair(L left, R right) {
            this.left = left; this.right = right;
        }

    }

    public void PutLeft(L left) {

        synchronized(monitor) {
            // check if the there are rOffers!
            if (rOffers.size() > 0) {
                R r  = rOffers.remove(0);
                pairs.add(new Pair(left, r));
                if (waiters.size() > 0) {
                    monitor.notifyAll(); // the front requested pair is completed!

                }
            }
            else {
                lOffers.add(left);
            }
        }
    }

    public void PutRight(R right) {
        synchronized(monitor) {
            // check if the there are lOffers!
            if (lOffers.size() > 0) {
                L l = lOffers.remove(0);
                pairs.add(new Pair(l, right));
                if (waiters.size() > 0) {
                    monitor.notifyAll(); // the front requested pair is completed!
                    return;
                }
                // if not, insert a new pair on the list

            }
            else {
                rOffers.add(right);
            }
        }
    }


    public Pair Take(long timeout)throws  InterruptedException
    {
        synchronized(monitor)
        {
            // fast path
            if (waiters.size() == 0 /* avoid barging */ && pairs.size() > 0 )
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
                    if (waiters.get(0) == req && pairs.size() > 0)
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
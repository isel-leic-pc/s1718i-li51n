package exercicios1;

import java.util.concurrent.TimeoutException;


import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class Exchanger<T> {
    private Object monitor = new Object(); // the monitor object

    // to specify the exchange state
    private enum State { INITIAL, STARTED, DONE, INTERRUPTED, TIMEOUT}

    // inner class to support teh exchange process
    private  class ExchangePoint {
        public T first, second;
        public State state;

        public ExchangePoint() {
            state = State.INITIAL;
        }

        /**
         * machine state for the exchange process
         * @param with
         * @return
         * @throws TimeoutException
         * @throws InterruptedException
         */
        public T pairing(T with) throws TimeoutException, InterruptedException{
            switch(state) {
                case INITIAL:
                    first = with;
                    state = State.STARTED;
                    return null;
                case STARTED:
                    second = with;
                    state = State.DONE;
                    return first;
                case TIMEOUT:
                case INTERRUPTED:
                default:
                    restart();
                    throw new TimeoutException();
            }
        }

        /**
         * recycle current exchange point for next exchange (in case of error)
         */
        public void restart() {
            state = State.INITIAL;
            first = second = null;
        }

    }

    ExchangePoint exchanger = new ExchangePoint(); // the exchange point

    public T Exchange(T mine, long timeout) throws TimeoutException, InterruptedException {
        synchronized(monitor) {
            T other;
            ExchangePoint current = exchanger;
            if ((other = current.pairing(mine)) != null) {
                exchanger = new ExchangePoint();
                monitor.notifyAll();
                return other;
            }
            if (timeout == 0) {
                current.restart();
                throw new TimeoutException();
            }
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE) {
                        monitor.wait();
                    } else {
                        monitor.wait(timeout);
                    }
                    if (current.state == State.DONE) {
                        return current.second;
                    }
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        current.state = State.TIMEOUT;
                        throw new TimeoutException();
                    }
                }
                catch(InterruptedException e) {
                    if (current.state == State.DONE ) {
                        Thread.currentThread().interrupt();
                        return current.second;
                    }
                    current.state = State.INTERRUPTED;
                    throw e;
                }
            }
            while(true);
        }
    }
}

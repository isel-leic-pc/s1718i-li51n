using System;
using System.Threading;
using Utils;

namespace Exercicios1 {
    public class Exchanger<T> where T : class {
        private Object monitor = new Object();

        private enum State { INITIAL, STARTED, DONE, INTERRUPTED, TIMEOUT }


        private class ExchangePoint {
            public T first, second;
            public State state;

            public ExchangePoint() { state = State.INITIAL; }

            public T pairing(T with) { // throws TimeoutException, InterruptedException{
                switch (state) {
                    case State.INITIAL:
                        first = with;
                        state = State.STARTED;
                        return null;
                    case State.STARTED:
                        second = with;
                        state = State.DONE;
                        return first;
                    case State.TIMEOUT:
                    case State.INTERRUPTED:
                    default:
                        restart();
                        throw new TimeoutException();
                }
            }

            public void restart() {
                state = State.INITIAL;
                first = second = null;
            }

        }

        private ExchangePoint exchanger = new ExchangePoint();

        public T Exchange(T mine, int timeout) { // throws TimeoutException, InterruptedException 
            lock (monitor) {
                T other;
                ExchangePoint current = exchanger;
                if ((other = current.pairing(mine)) != null) {
                    exchanger = new ExchangePoint();
                    Monitor.PulseAll(monitor);
                    return other;
                }
                if (timeout == 0) {
                    current.restart();
                    throw new TimeoutException();
                }
                do {
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor);
                        if (current.state == State.DONE) {
                            return current.second;
                        }
                        timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                        if (timeout == 0) {
                            current.state = State.TIMEOUT;
                            throw new TimeoutException();
                        }
                    }
                    catch (ThreadInterruptedException e) {
                        if (current.state == State.DONE) {
                            Thread.CurrentThread.Interrupt();
                            return current.second;
                        }
                        current.state = State.INTERRUPTED;
                        throw e;
                    }
                }
                while (true);
            }
        }
    } // end Class Exchanger
} // end namespace

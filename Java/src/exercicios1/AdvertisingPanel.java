package exercicios1;

import java.time.Instant;
import java.util.Date;
import java.util.LinkedList;
import java.util.List;
import java.util.function.Supplier;

import static utils.SynchUtils.remainingTimeout;

class AdvertisingPanel<M>  {

    private Object monitor = new Object();
    private List<MessageWaiter> waiters;
    private MessageHolder msgHolder;

    private class MessageHolder {
        private  M message;
        private Date endValidationDate;


        public boolean isValidMessage() {
            return message != null &&  new Date().before(endValidationDate);

        }

        public void Invalidate() {
            message = null;
        }

        public M getMessage() {
            return message;

        }

        public void newMessage(M message, long exposureTime) {
            this.message = message;
            endValidationDate = Date.from(Instant.now().plusMillis(exposureTime));
        }

    }

    private class MessageWaiter {
        public M message;
    }

    public AdvertisingPanel() {
        waiters = new LinkedList<MessageWaiter>();
        msgHolder = new MessageHolder();
    }


    private void sendMessageToWaiters(M message) {
        while (waiters.size() > 0) {
            MessageWaiter m = waiters.remove(0);
            m.message = message;
        }
        monitor.notifyAll();
    }

    public void Publish(Supplier<M> supplier, int exposureTime)
    {
        M message = supplier.get();
        synchronized(monitor) {
            sendMessageToWaiters(message);
            if (exposureTime == 0) {
                msgHolder.Invalidate();
            }
            else {
                msgHolder.newMessage(message, exposureTime);
            }
            monitor.notifyAll();
        }
    }

    public M Consume(long timeout) throws InterruptedException  {
        synchronized (monitor) {
            if ( msgHolder.isValidMessage())
                return msgHolder.getMessage();

            if (timeout == 0)
                return null;

            MessageWaiter w = new MessageWaiter();
            waiters.add(w);
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    monitor.wait(timeout);
                    if (w.message != null)
                        return w.message;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        waiters.remove(w);
                        return null;
                    }
                }
                catch(InterruptedException e) {
                    if (w.message != null) {
                      Thread.currentThread().interrupt();
                      return w.message;
                    }
                    waiters.remove(w);
                    throw e;
                }
            } while(true);
        }
    }
}
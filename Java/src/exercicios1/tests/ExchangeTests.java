package exercicios1.tests;

import exercicios1.Exchanger;
import org.junit.Assert;
import org.junit.Test;

import java.util.concurrent.TimeoutException;


/**
 * Created by jmartins on 05/10/2017.
 */
public class ExchangeTests {
    private static class Result {
        boolean ok1, ok2;
        public Result(boolean initial1, boolean initial2) {
            ok1 = initial1; ok2 = initial2;
        }

        public Result( ) {
            this(false,false);
        }
    }

    @Test
    public void SimpleExchange() {
        final Exchanger<String> ex = new Exchanger<>();

        final Result res = new Result();

        Thread t1 = new Thread(() -> {
            try {
                String o2 = ex.Exchange("t1", 5000);
                res.ok1 = o2.equals("t2");
            }
            catch(Exception e) {
                res.ok1 = false;
            }
        });
        t1.start();

        try {
            String o2 = ex.Exchange("t2", 5000);
            res.ok2 = o2.equals("t1");
        }
        catch(Exception e) {
            res.ok2 = false;
        }

        Assert.assertTrue(res.ok1 && res.ok2);



    }

    @Test
    public void ExchangeFailOnTimeout() {
        final Exchanger<String> ex = new Exchanger();

        final Result res = new Result();


        try {
            String o2 = ex.Exchange("t2", 3000);
        }
        catch(TimeoutException e) {
            res.ok1 = true;
        }
        catch(Exception e) {
            res.ok1 = false;
        }

        Assert.assertTrue(res.ok1);

    }

    @Test
    public void ExchangeFailOnInterruption() {
        final Exchanger<String> ex = new Exchanger<>();

        final Result res = new Result();

        Thread t1 = new Thread(() -> {
            try {
                String o2 = ex.Exchange("t1", 10000);
                res.ok2 = false;
            }
            catch(InterruptedException e) {
                res.ok1 = true;
            }
            catch(TimeoutException e) {
                res.ok1 = false;
            }
        });
        t1.start();

        try {
            try { Thread.sleep(3000); } catch(InterruptedException e) {}
            t1.interrupt();
            try { Thread.sleep(3000); } catch(InterruptedException e) {}
            String o2 = ex.Exchange("t2", 5000);
            res.ok2 = false;
        }
        catch(TimeoutException e) {
            res.ok2 = true;
        }
        catch(InterruptedException e) {
            res.ok2 = false;
        }

        Assert.assertTrue(res.ok1 && res.ok2);

    }

    @Test
    public void MultipleExchangeCheck() {
        final int NTRIES = 1000*1000;
        final Exchanger<Integer> ex = new Exchanger<>();

        final Result res = new Result(true, true);

        Thread t1 = new Thread(() -> {
            int val = 0;
            for (int i = 0; i < NTRIES; ++i) {
                try {
                    int newVal = ex.Exchange(val, 100);
                    res.ok1 = val  == newVal;
                    val++;
                } catch (InterruptedException e) {
                    res.ok1 = false;
                } catch (TimeoutException e) {
                    res.ok1 = false;
                }
            }
        });
        t1.start();


        int val = 0;
        for (int i = 0; i < NTRIES; ++i) {
            try {
                int newVal = ex.Exchange(val, 100);
                res.ok2 = val == newVal;
                val++;
            } catch (InterruptedException e) {
                res.ok2 = false;
            } catch (TimeoutException e) {
                res.ok2 = false;
            }
        }

        try {
            t1.join();
        }
        catch (InterruptedException e)
        {
            res.ok2 = false;
        }

        Assert.assertTrue(res.ok1 && res.ok2);

    }
}

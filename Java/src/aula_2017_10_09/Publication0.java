package aula_2017_10_09;

/**
 * Created by jmartins on 13/10/2017.
 */
public class Publication0 {
    static   boolean done;
    public static void main(String[] args) {

        Thread t = new Thread(() -> {
            boolean toggle = false;
            while (!done) toggle = !toggle;
        });
        t.start();
        try {
            Thread.sleep(1000);
        }
        catch(InterruptedException e) {}

        done = true;
        try {
            t.join();        // Blocks indefinitely
        }
        catch(InterruptedException e) {}
        System.out.println("Test Done!");
    }
}

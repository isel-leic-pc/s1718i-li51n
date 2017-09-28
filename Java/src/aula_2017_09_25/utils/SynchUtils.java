package aula_2017_09_25.utils;

/**
 * Created by jmartins on 27/09/2017.
 */
public class SynchUtils {
    public static final long INFINITE = -1;

    public static long remainTimeout(long refTime, long timeout) {
        if (timeout == INFINITE) return timeout;
        return Math.max(0, timeout - (System.currentTimeMillis() - refTime));
    }


}
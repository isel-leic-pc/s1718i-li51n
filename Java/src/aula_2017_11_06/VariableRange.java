package aula_2017_11_06;

import java.util.concurrent.atomic.AtomicReference;

public class VariableRange {

    // An immutable holder is what we need to mantain
    // the two fields consistent in all cases
    private class IntPair {
        private final int lower;
        private final int upper;

        private IntPair() {
            lower = upper = 0;
        }


        private IntPair(int l, int u) {
            lower = l; upper = u;
        }

    }

    private AtomicReference<IntPair> range =
            new AtomicReference<>(new IntPair());


    public void setLower(int l) {
        do {
            IntPair observed = range.get();
            if (l > observed.upper)
                throw new IllegalStateException("lower greater than upper!");
            if (range.compareAndSet(observed, new IntPair(l, observed.upper)))
                return;
        }
        while(true);

    }

    public void setUpper(int u) {
        do {
            IntPair observed = range.get();
            if (u < observed.lower)
                throw new IllegalStateException("upper less than lower!");
            if (range.compareAndSet(observed, new IntPair(observed.lower, u)))
                return;
        }
        while(true);
    }
}

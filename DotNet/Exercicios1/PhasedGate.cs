using System;
using System.Threading;
using Utils;

namespace Exercicios1 {
    public class PhasedGate {
        private Object monitor = new Object();

        private int remaining;

        public PhasedGate(int parts) {
            remaining = parts;
        }

        /**
         * Assumed to run inside monitor
         * @return
         */
        private bool CheckLastPart() {
            if (remaining == 0) throw new InvalidOperationException();
            if (--remaining == 0) {
                Monitor.PulseAll(monitor);
                return true;
            }
            return false;
        }

        public void Wait() { // throws IllegalStateException, InterruptedException 
            lock(monitor) {
            if (CheckLastPart()) return;

            while (remaining > 0) {
                Monitor.Wait(monitor);
            }
        }
    }

    public void RemoveParticipant() {
        lock(monitor) {
            CheckLastPart();
        }
    }
}
}

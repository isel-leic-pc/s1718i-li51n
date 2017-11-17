
using System.Threading;

namespace ExerciciosLockFree_ExamesAntigos {
    class E2_2016v {
        /**    
            Esta implementação reflete a semântica de uma message box contendo no máximo 
            uma mensagem que pode ser consumida múltiplas vezes, contudo não é thread-safe. 
            Implemente em Java ou em C#, sem utilizar locks, uma versão thread-safe deste 
            sincronizador.
         */
        public class UnsafeMessageBox<M> where M : class {
            private class MsgHolder {
                internal readonly M msg;
                internal int lives;
            }
            private MsgHolder msgHolder = null;
            public void Publish(M m, int lvs) { msgHolder = new MsgHolder { msg = m, lives = lvs }; }
            public M TryConsume() {
                if (msgHolder != null && msgHolder.lives > 0) {
                    msgHolder.lives -= 1;
                    return msgHolder.msg;
                }
                return null;
            }
        }

        /**
         * Nesta implementação é criada uma instância de MsgHolder
         * por cada novo consumo
         */
        public class SafeMessageBox<M> where M : class {
            /*
             * As instâncias de MsgHolder são imutáveis
             */
            private class MsgHolder {
                internal readonly M msg;
                internal readonly int lives;

                public MsgHolder(M msg, int lives) {
                    this.msg = msg; this.lives = lives;
                }

            }

            private volatile MsgHolder msgHolder = null;
            public void Publish(M m, int lvs) {
                msgHolder = new MsgHolder(m, lvs);
            }

            public M TryConsume() {
                do {
                    MsgHolder curr = msgHolder;
                    if (curr == null || curr.lives == 0) return null;
                    if (Interlocked.CompareExchange(
                        ref msgHolder,
                        new MsgHolder(curr.msg, curr.lives - 1),
                        curr) == curr)
                        return curr.msg;
                }
                while (true);
            }
        }

        /*
         * Nesta versão é actualizado o campo lives
         * do holder corrente
         */
        public class SafeMessageBox2<M> where M : class {

            private class MsgHolder {
                internal readonly M msg;
                internal int lives;

                public MsgHolder(M msg, int lives) {
                    this.msg = msg; this.lives = lives;
                }

            }

            private volatile MsgHolder msgHolder = null;
            public void Publish(M m, int lvs) {
                msgHolder = new MsgHolder(m, lvs);
            }


            public M TryConsume() {
                do {
                    MsgHolder curr = msgHolder;
                    if (curr == null || curr.lives == 0) return null;
                    int lives = curr.lives;
                    if (Interlocked.CompareExchange(
                        ref curr.lives,
                        curr.lives - 1,
                        curr.lives) == curr.lives)
                        return curr.msg;
                }
                while (true);
            }

        }
    }
}

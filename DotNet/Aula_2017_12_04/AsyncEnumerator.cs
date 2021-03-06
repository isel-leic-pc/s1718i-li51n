﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aula_2017_12_04 {
    public static class AsyncEnumerator {
        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> toComplete,
                                                    Task task) {
            if (task == null) {
                toComplete.TrySetResult(default(TResult));
                return;
            }

            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    toComplete.TrySetResult(task is Task<TResult> ? ((Task<TResult>)task).Result : default(TResult));
                    break;
                case TaskStatus.Faulted:
                    toComplete.TrySetException(task.Exception.InnerExceptions);
                    break;
                case TaskStatus.Canceled:
                    toComplete.TrySetCanceled();
                    break;
            }
        }
        private static Task<T> run_internal<T>(IEnumerator<Task> tseq) {
            Action<Task> cont = null;
            TaskCompletionSource<T> tproxy = new TaskCompletionSource<T>();

            cont = (t) => {
                if (t != null && t.Status == TaskStatus.Faulted || !tseq.MoveNext()) {

                    tproxy.TrySetFromTask(t);
                    return;
                }
               
                Task t1 = tseq.Current;
                if (t1.Status == TaskStatus.Created)
                    t1.Start();
                t1.ContinueWith(cont);

            };
            cont(null);
            return tproxy.Task;
        }
        public static Task<T> Run<T>(this IEnumerator<Task<T>> tseq) {
            return run_internal<T>(tseq);
        }

        public static Task<T> Run<T>(this IEnumerable<Task<T>> tseq) {
            return run_internal<T>(tseq.GetEnumerator());
        }

        public static Task  Run (this IEnumerator<Task> tseq) {
            return run_internal<object>(tseq);
        }

        public static Task Run(this IEnumerable<Task> tseq) {
            return run_internal<object>(tseq.GetEnumerator());
        }
    }
}

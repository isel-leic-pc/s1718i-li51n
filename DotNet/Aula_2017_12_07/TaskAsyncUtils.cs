using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Aula_2017_12_07 {
    public static class TaskAsyncUtils {
        public static IEnumerable<Task<T>> OrderByCompletion<T>(
            this IEnumerable<Task<T>> tasks) {
          
            List<Task<T>> allTasks = tasks.ToList();
            int promiseIndex = -1;
            var completions = new TaskCompletionSource<T>[allTasks.Count];
            var promiseTasks = new Task<T>[allTasks.Count];
            for (int i = 0; i < allTasks.Count; ++i) {
                completions[i] = new TaskCompletionSource<T>();
                promiseTasks[i] = completions[i].Task;
                allTasks[i].ContinueWith(ant => {
                    int idx = Interlocked.Increment(ref promiseIndex);
                    switch (ant.Status) {
                        case TaskStatus.Faulted:
                            completions[idx].TrySetException(ant.Exception.InnerException);
                            break;
                        case TaskStatus.Canceled:
                            completions[idx].TrySetCanceled();
                            break;
                        case TaskStatus.RanToCompletion:
                            completions[idx].TrySetResult(ant.Result);
                            break;
                    }

                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            return promiseTasks;
        }
    }
}

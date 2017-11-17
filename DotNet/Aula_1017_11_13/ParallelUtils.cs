using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aula_1017_11_13 {
    public class ParallelUtils {

        public static int ParallelCount<T>(T[] vals, T vref) {
            int nChunks = Environment.ProcessorCount;
            int chunkSize = vals.Length / nChunks;
            int total = 0;
            CountdownEvent done = new CountdownEvent(nChunks);
            for(int i=0; i < nChunks; ++i) {
                ThreadPool.QueueUserWorkItem((o) => {
                    int partialCount = 0;
                    int idx = (int)o;
                    int start = idx * chunkSize;
                    int end = (idx == nChunks) ? vals.Length :
                            start + chunkSize;
                    for (int p = start; p < end; ++p)
                        if (vals[p].Equals(vref)) partialCount++;

                    Interlocked.Add(ref total, partialCount);
                    done.Signal();
                }, i);
            }

            done.Wait();
            return total;
        }

    }
}

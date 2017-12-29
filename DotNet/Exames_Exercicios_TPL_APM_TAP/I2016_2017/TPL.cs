using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace I2016_2017 {
    class TPL {
        public static List<O> MapSelectedItems<I, O>(IEnumerable<I> items, 
            Predicate<I> selector,
            Func<I, O> mapper, 
            CancellationToken ctoken) {
            var result = new List<O>();
            foreach (I item in items) {
                ctoken.ThrowIfCancellationRequested();
                if (selector(item)) result.Add(mapper(item));
            }
            return result;
        }

        public static List<O> MapSelectedItemsPar<I, O>(IEnumerable<I> items,
            Predicate<I> selector,
            Func<I, O> mapper,
            CancellationToken ctoken) {
                object monitor = new object();
                ParallelOptions options = new ParallelOptions { CancellationToken = ctoken };
                List<O> result = new List<O>();
                Parallel.ForEach(
                    items,
                    options,
                    () => new List<O>(),
                    (i, s, r) => {
                        if (selector(i)) r.Add(mapper(i));
                        return r;
                    },
                    r => {
                        lock (monitor)
                            result.AddRange(r);
                    });
                return result;
        }
    }
}

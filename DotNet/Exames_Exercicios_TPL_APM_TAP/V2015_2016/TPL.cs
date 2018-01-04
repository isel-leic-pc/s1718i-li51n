using System.Collections.Generic;
using System.Threading.Tasks;

namespace V2015_2016 {
    class TPL {
        public static Result MapAggregate(IEnumerable<Data> data) {
            Result result = new Result();
            foreach (var datum in data) {
                result = Aggregate(result, Map(datum));
                if (result.Equals(Result.OVERFLOW)) break;
            }
            return result;
        }

        public class Result   {
            public static Result OVERFLOW = new Result();
        }

        public class Data {

        }

        public static Result Aggregate(Result r1, Result r2) {

            return null;
        }

        public static Result Map(Data d) {
            return null;
        }

        public static Result MapAggregatePar(IEnumerable<Data> data) {
            Result result = new Result();
            bool stop = false;
            object _lock = new object();
            ParallelLoopResult lr=   Parallel.ForEach<Data,Result>(
                data, 
                () => new Result(),  
                (d, state, res) => {

                   res = Aggregate(res, Map(d));

                   if (res.Equals(Result.OVERFLOW)) {
                       state.Stop();
                       return new Result();
                   }
                   else
                        return res;
                },
                r => {
                    // guarantes atomic "result" aggregation and 
                    // "stop" publication
                    lock(_lock) {
                        result = Aggregate(result, r);
                        if (result.Equals(Result.OVERFLOW)) {
                            stop = true;
                        }
                    }
                });
            // note the lock is used here as a fresh read guarantee 
            // for result and stop variables       
            lock(_lock) {
                if (!lr.IsCompleted || stop)
                    return new Result();
                else
                    return result;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace V2016_2017 {
    class Program {
        static void Main(string[] args) {
            int[] vals = { 7, 3, 5, 1, 9, 11, 13, 15, 17, 19, 4, 6};
            try {
                int res = TPL.SearchItem(vals, (i) => i % 2 == 0, 3000);
                Console.WriteLine("result={0}", res);
            }
            catch (TimeoutException e) {
                Console.WriteLine(e);
            }
           
        }
    }
}

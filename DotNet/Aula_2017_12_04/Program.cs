using Aula_2017_11_27;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2017_12_04 {
    class Program {
        public static IEnumerable<Task> TickTack() {
            for (int i = 0; i < 10; ++i) {
                yield return Task.Delay(500);
                Console.WriteLine("Tick");
                yield return Task.Delay(500);
                Console.WriteLine("Tack");
            }
        }

        public static async Task TickTackAsync() {
            try {
                for (int i = 0; i < 10; ++i) {
                    await Task.Delay(500);
                    Console.WriteLine("Tick");
                    await Task.Delay(500);
                    Console.WriteLine("Tack");
                }
            }
            catch (Exception e) {
            }
        }

        static async Task<int> XptoAsync() {
            await TickTackAsync();
            Console.WriteLine("ok");
            return 10;
        }

        static void Main(string[] args) {
            /*
            var t = TickTackAsync().ContinueWith(ant=> {
                Console.WriteLine(ant.Result);
            });
            */
            Task t = XptoAsync();
            Console.WriteLine("Return of XptoAsync");

            Console.WriteLine("Press Enter to Terminate...");
            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class Instrumentation {
        public const bool ACTIVE = true;
        public static System.Collections.Generic.Dictionary<string,System.Int64> counters = new System.Collections.Generic.Dictionary<string, System.Int64>();
        public static void count(string counter) {
            if (counters.ContainsKey(counter)) {
                Saxon.Impl.Helpers.MapUtils.Put(counters, counter, (Saxon.Impl.Helpers.MapUtils.Get(counters, counter)+1));
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(counters, counter, 1L);
            }
        }
        public static void count(string counter, long increment) {
            if (counters.ContainsKey(counter)) {
                Saxon.Impl.Helpers.MapUtils.Put(counters, counter, (Saxon.Impl.Helpers.MapUtils.Get(counters, counter)+increment));
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(counters, counter, increment);
            }
        }
        public static void report() {
            if ((ACTIVE && !(counters.Count==0))) {
                System.Console.Error.WriteLine("COUNTERS");
                foreach (System.Collections.Generic.KeyValuePair<string,System.Int64> c in counters) {
                    System.Console.Error.WriteLine(((c.Key+" = ")+c.Value));
                }
            }
        }
        public static void reset() {
            counters.Clear();
        }
    
    }
}
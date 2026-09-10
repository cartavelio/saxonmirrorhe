using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class Timer {
        private static readonly Saxon.Hej.value.DayTimeDurationValue milliSecond = new Saxon.Hej.value.DayTimeDurationValue(1, 0, 0, 0, 0, 1000);
        private readonly long start;
        private long prev;
        public Timer()  {
            start = Saxon.Ejava.lang.System.nanoTime();
            prev = start;
        }
        public static string showExecutionTimeNano(long nanosecs) {
            if ((nanosecs<1e9)) {
                return (((nanosecs/1e6))+"ms");
            } else {
                try {
                    double millisecs = (nanosecs/1e6);
                    Saxon.Hej.value.DayTimeDurationValue d = (Saxon.Hej.value.DayTimeDurationValue)(milliSecond.multiply(millisecs));
                    long days = ((Saxon.Hej.value.NumericValue)(d.getComponent(Saxon.Hej.functions.AccessorFn.Component.DAY))).longValue();
                    long hours = ((Saxon.Hej.value.NumericValue)(d.getComponent(Saxon.Hej.functions.AccessorFn.Component.HOURS))).longValue();
                    long minutes = ((Saxon.Hej.value.NumericValue)(d.getComponent(Saxon.Hej.functions.AccessorFn.Component.MINUTES))).longValue();
                    Singulink.Numerics.BigDecimal seconds = ((Saxon.Hej.value.NumericValue)(d.getComponent(Saxon.Hej.functions.AccessorFn.Component.SECONDS))).getDecimalValue();
                    Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                    if ((days>0)) {
                        fsb.append((days+"days "));
                    }
                    if ((hours>0)) {
                        fsb.append((hours+"h "));
                    }
                    if ((minutes>0)) {
                        fsb.append((minutes+"m "));
                    }
                    fsb.append((seconds+"s"));
                    return (((fsb.ToString()+" (")+(nanosecs/1e6))+"ms)");
                } catch (Saxon.Hej.trans.XPathException e) {
                    return ((nanosecs/1e6)+"ms");
                }
            }
        }
        public static string showMemoryUsed() {
            long value = (Saxon.Ejava.lang.Runtime.getRuntime().totalMemory()-Saxon.Ejava.lang.Runtime.getRuntime().freeMemory());
            return (("Memory used: "+((value/1_000_000)))+"Mb");
        }
        public void report(string label) {
            long time = Saxon.Ejava.lang.System.nanoTime();
            System.Console.Error.WriteLine((((label+" ")+(((time-prev))/1e6))+"ms"));
            prev = time;
        }
        public void reportCumulative(string label) {
            long time = Saxon.Ejava.lang.System.nanoTime();
            System.Console.Error.WriteLine((((label+" ")+(((time-start))/1e6))+"ms"));
            prev = time;
        }
    
    }
}
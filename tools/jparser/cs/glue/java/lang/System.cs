using Sys = System;

namespace java.lang
{
    public class System
    {
        public static void arraycopy<T>(T[] from, int fromStart, T[] to, int toStart, int len)
        {
            throw new Sys.NotImplementedException();
        }

        public static string getProperty(string name) {
            return Sys.Environment.GetEnvironmentVariable(name);
        }

        public static int identityHashCode(object o) {
            return Sys.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
        }
        
        public static long nanoTime() {
            long nano = 10000L * Sys.Diagnostics.Stopwatch.GetTimestamp();
            nano /= Sys.TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
        
        public static long currentTimeMillis() {
            return nanoTime() / 1_000_000L;
        }

        public static java.io.PrintStream @out = null;
        public static java.io.PrintStream err = null;

    }
}
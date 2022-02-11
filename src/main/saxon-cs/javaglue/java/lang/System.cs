using java.util;
using Sys = System;
using SysColl = System.Collections;
using SysCollGen = System.Collections.Generic;

namespace java.lang
{
    public class System
    {
        public static void arraycopy<T>(T[] from, int fromStart, T[] to, int toStart, int len)
        {
            Sys.Array.Copy(from, fromStart, to, toStart, len);
        }
        
        public static void exit(int exitCode)
        {
            Sys.Console.WriteLine("Exiting with code " + exitCode);
        }

        public static string getProperty(string name) {
            return Sys.Environment.GetEnvironmentVariable(name);
        }

        public static java.util.Properties getProperties() {
            throw new Sys.NotImplementedException();
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

        public static SysCollGen.IDictionary<string,string> getenv() {
            SysCollGen.Dictionary<string, string> target = new SysCollGen.Dictionary<string, string>();
            foreach (SysColl.DictionaryEntry kvp in Sys.Environment.GetEnvironmentVariables()) {
                target[kvp.Key.ToString()] = kvp.Value.ToString();
            }
            return target;
        }

        public static string getenv(string name) {
            return Sys.Environment.GetEnvironmentVariable(name);
        }

        public static java.io.PrintStream @out = null;
        public static java.io.PrintStream err = null;

    }
}
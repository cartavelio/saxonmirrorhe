using System.Diagnostics;
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
            if (fromStart < 0 || toStart < 0 || len < 0 || fromStart + len > from.Length || toStart + len > to.Length) {
                throw new IndexOutOfBoundsException($"arraycopy({from.Length},{fromStart},{to.Length},{toStart},{len})");
            }
            if (len > 0) {
                Sys.Array.Copy(from, fromStart, to, toStart, len);
            }
        }
        
        public static void exit(int exitCode)
        {
            Sys.Console.WriteLine("Exiting with code " + exitCode);
        }

        public static string getProperty(string name) {
            if (name == "user.dir") {
                return Sys.Environment.CurrentDirectory;
            }
            return Sys.Environment.GetEnvironmentVariable(name);
        }

        public static java.util.Properties getProperties() {
            return new java.util.Properties();
        }

        public static int identityHashCode(object o) {
            return Sys.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
        }
        
        public static long nanoTime() {
            long ticks = Sys.Diagnostics.Stopwatch.GetTimestamp();
            return ticks * (1_000_000_000L / Stopwatch.Frequency);
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
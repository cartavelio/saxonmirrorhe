using System.Diagnostics;
using Saxon.Ejava.util;
using Saxon.Hej.functions;
using Sys = System;
using SysIO = System.IO;
using SysColl = System.Collections;
using SysCollGen = System.Collections.Generic;

namespace Saxon.Ejava.lang
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
            Sys.Environment.Exit(exitCode);
        }

        public static string getProperty(string name) {
            if (name == "user.dir") {
                return Sys.Environment.CurrentDirectory;
            } else if (name == "line.separator") {
                return Sys.Environment.NewLine;
            } else if (name == "java.io.tmpdir") {
                return SysIO.Path.GetTempPath();
            }
            return Sys.Environment.GetEnvironmentVariable(name);
        }

        public static Saxon.Ejava.util.Properties getProperties() {
            return new Saxon.Ejava.util.Properties();
        }

        public static int identityHashCode(object o) {
            return Sys.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(o);
        }
        
        public static long nanoTime() {
            long ticks = Sys.Diagnostics.Stopwatch.GetTimestamp();
            return ticks * (1_000_000_000L / Stopwatch.Frequency);
        }
        
        public static long currentTimeMillis() {
            return Sys.DateTimeOffset.Now.ToUnixTimeMilliseconds();
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

        public static Saxon.Ejava.io.PrintStream @out = null;
        public static Saxon.Ejava.io.PrintStream err = null;

    }
}
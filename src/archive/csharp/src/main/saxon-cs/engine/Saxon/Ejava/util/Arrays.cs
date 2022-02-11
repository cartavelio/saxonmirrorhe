using System;
using System.Linq;
using Saxon.Ejava.lang;
using Exception = System.Exception;
using Math = System.Math;
using SysArray = System.Array;
using SysColl = System.Collections;
using SysCollGen = System.Collections.Generic;

namespace Saxon.Ejava.util
{
    public class Arrays
    {
        public static void fill<T>(T[] array, T value)
        {
            SysArray.Fill<T>(array, value);
        }
        
        public static void fill<T>(T[] array, int start, int end, T value)
        {
            SysArray.Fill<T>(array, value, start, end - start);
        }

        public static T[] copyOf<T>(T[] source, int length) {
            T[] dest = new T[length];
            SysArray.Copy(source, 0, dest, 0, Math.Min(length, source.Length));
            return dest;
        }
        
        public static T[] copyOfRange<T>(T[] source, int start, int end) {
            T[] dest = new T[end - start];
            SysArray.Copy(source, start, dest, 0, end - start);
            return dest;
        }

        public static SysCollGen.IList<T> asList<T>(params T[] array) {
            return new SysCollGen.List<T>(array);
        }

        public static void sort<T>(T[] array) {
            SysArray.Sort(array);
        }
        
        public static void sort<T>(T[] array, SysColl.IComparer comparer) {
            SysArray.Sort(array, comparer);
        }
        
        public static void sort<T>(T[] array, int start, int end, SysColl.IComparer comparer) {
            try {
                SysArray.Sort(array, start, end, comparer);
            }
            catch (Exception e) {
                throw new System.InvalidCastException(e.Message);
            }
        }

        public static int binarySearch<T>(T[] array, T value) {
            return SysArray.BinarySearch(array, value, StringComparer.Ordinal);
        }
        public static int binarySearch<T>(T[] array, int start, int end, T value) {
            return SysArray.BinarySearch(array, start, end, value, StringComparer.Ordinal);
        }

        public static string ToString<T>(T[] array) {
            var sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (T t in array) {
                if (first) {
                    first = false;
                }
                else {
                    sb.Append(' ');
                }

                sb.Append(t.ToString());
            }

            return sb.ToString();
        }

        public static int hashCode<T>(T[] array) {
            int h = 33862;
            foreach (T member in array) {
                h ^= member.GetHashCode();
            }

            return h;
        }
        
        public static bool Equals<T>(T[] first, T[] second)
        {
            // TODO: there are apparently much faster approaches, especially for byte arrays
            return Enumerable.SequenceEqual(first, second);
        }
    }
}
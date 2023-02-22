using System;
using SysArray = System.Array;
using SysCollGen = System.Collections.Generic;

namespace java.util
{
    public class Arrays
    {
        public static void fill<T>(T[] array, T value)
        {
            //SysArray.Fill<T>(array, value);  // Needs .NET 5.
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }
        
        public static void fill<T>(T[] array, int start, int end, T value)
        {
            //SysArray.Fill<T>(array, value);  // Needs .NET 5.
            for (int i = start; i < end; i++)
            {
                array[i] = value;
            }
        }

        public static T[] copyOf<T>(T[] source, int length) {
            T[] dest = new T[length];
            SysArray.Copy(source, 0, dest, 0, length);
            return dest;
        }
        
        public static T[] copyOfRange<T>(T[] source, int start, int end) {
            throw new NotImplementedException();
        }

        public static java.util.List<T> asList<T>(params T[] array) {
            return new ArrayList<T>(new SysCollGen.List<T>(array));
        }

        public static void sort<T>(T[] array) {
            SysArray.Sort(array);
        }

        public static int binarySearch<T>(T[] array, T value) {
            return SysArray.BinarySearch(array, value);
        }
        public static int binarySearch<T>(T[] array, int start, int end, T value) {
            return SysArray.BinarySearch(array, start, end, value);
        }

        public static string ToString<T>(T[] array) {
            return "Arrays.ToString not implemented";
        }

        public static int hashCode(byte[] array) {
            throw new NotImplementedException();
        }
    }
}
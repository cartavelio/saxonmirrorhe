
using SysCollGen = System.Collections.Generic;
using System.Linq;
using java.lang;

namespace Saxon.Impl.Helpers {
    public class ListUtils {
        // Get a sublist of a list, as per the Java List.subList(start, end) method
        public static SysCollGen.IList<T> SubList<T>(SysCollGen.IList<T> input, int start, int end) {
            if (input is SysCollGen.List<T>) {
                // GetRange is available only with List<T>
                return ((SysCollGen.List<T>) input).GetRange(start, end - start);
            }
            else {
                // Use Linq
                return input.Skip(start).Take(end - start).ToList();
            }
        }
        
        // // Remove the item at position n, returning the item that was removed,
        // // as per Java's List.remove(n).
        //
        // public static T RemoveAt<T>(SysCollGen.IList<T> input, int pos) {
        //     T existing = input[pos];
        //     input.RemoveAt(pos);
        //     return existing;
        // }
        
        // Overloaded remove() method, following the Java List.remove() semantics
        
        public static T Remove<T>(SysCollGen.IList<T> input, object arg) {
            if (arg is int i) {
                T existing = input[i];
                input.RemoveAt(i);
                return existing;
            } else if (arg is T item) {
                input.Remove(item);
                return item;
            } else {
                throw new ClassCastException();
            }
        }
        
        public static bool AddAll<T>(SysCollGen.IList<T> input, SysCollGen.ICollection<T> extra) {
            if (input is SysCollGen.List<T>) {
                // AddRange is available only with List<T>, not with other kinds of IList
                ((SysCollGen.List<T>) input).AddRange(extra);
                return extra.Count > 0;
            } else {
                foreach (T item in extra) {
                    input.Add(item);
                }
                return extra.Count > 0;
            }
        }
        
        public static void TrimToSize<T>(SysCollGen.IList<T> input) {
            if (input is SysCollGen.List<T>) {
                // TrimExcess is available only with List<T>
                ((SysCollGen.List<T>) input).TrimExcess();
            }
        }
    }
}
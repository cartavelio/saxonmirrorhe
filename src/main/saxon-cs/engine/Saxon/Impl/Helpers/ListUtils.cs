
using SysCollGen = System.Collections.Generic;
using System.Linq;
using Saxon.Ejava.lang;

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

        public static T PollFirst<T>(SysCollGen.LinkedList<T> input) where T : class {
            SysCollGen.LinkedListNode<T> first = input.First;
            if (first == null) {
                return null;
            }
            input.RemoveFirst();
            return first.Value;
        }

        public static bool EqualLists<T>(SysCollGen.IList<T> one, SysCollGen.IList<T> two) {
            if (one.Count != two.Count) {
                return false;
            }

            SysCollGen.IEnumerator<T> iter1 = one.GetEnumerator();
            SysCollGen.IEnumerator<T> iter2 = two.GetEnumerator();
            while (iter1.MoveNext() && iter2.MoveNext()) {
                if (iter1.Current != null && !iter1.Current.Equals(iter2.Current)) {
                    iter1.Dispose();
                    iter2.Dispose();
                    return false;
                }
            }
            iter1.Dispose();
            iter2.Dispose();
            return true;
        }

        public static int ListHashCode<T>(SysCollGen.IList<T> list) {
            int h = 531642;
            foreach (T t in list) {
                h ^= t.GetHashCode();
            }
            return h;
        }
    }


}

// Copyright (c) 2021 Saxonica Limited
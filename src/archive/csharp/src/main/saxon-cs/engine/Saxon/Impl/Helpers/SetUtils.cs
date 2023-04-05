using System.Linq;
using Saxon.Ejava.lang;
using Saxon.Hej.functions;

namespace Saxon.Impl.Helpers {
    public class SetUtils {
        public static bool AddAll<T>(System.Collections.Generic.ISet<T> input,
            System.Collections.Generic.ICollection<T> extra) {
            foreach (T item in extra) {
                input.Add(item);
            }

            return extra.Count > 0;
        }
        
        public static bool ContainsAll<T>(
                System.Collections.Generic.ISet<T> input, 
                System.Collections.Generic.ISet<T> subset) {
            return subset.All(input.Contains);
        }
        
        public static T PollFirst<T>(System.Collections.Generic.SortedSet<T> input) {
            System.Collections.Generic.IEnumerator<T> iter = input.GetEnumerator();
            if (iter.MoveNext()) {
                T first = iter.Current;
                iter.Dispose();
                input.Remove(first);
                return first;
            }
            else {
                return default(T);
            } 
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
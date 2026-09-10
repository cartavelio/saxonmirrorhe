using System.Linq;
using Saxon.Ejava.lang;

namespace Saxon.Impl.Helpers {
    public class LinkedListUtils {
        
        
        public static bool AddAll<T>(System.Collections.Generic.LinkedList<T> input, System.Collections.Generic.ICollection<T> extra) {
            foreach (T item in extra) {
                input.AddLast(item);
            }
            return extra.Count > 0;
        }
       

        public static T PollFirst<T>(System.Collections.Generic.LinkedList<T> input) where T : class {
            System.Collections.Generic.LinkedListNode<T> first = input.First;
            if (first == null) {
                return null;
            }
            input.RemoveFirst();
            return first.Value;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
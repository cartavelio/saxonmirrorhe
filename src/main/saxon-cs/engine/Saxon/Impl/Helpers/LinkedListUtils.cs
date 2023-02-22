using System.Linq;
using Saxon.Ejava.lang;
using Saxon.Hej.om;

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

        public static SequenceIterator GetSequenceIterator<T>(System.Collections.Generic.LinkedList<T> input) where T : Item {
            return new LinkedListIterator<T>(input);
        }
    }
    
    internal class LinkedListIterator<T> : SequenceIterator where T : Item {
        private readonly System.Collections.Generic.IEnumerator<T> enumerator;
        
        public LinkedListIterator(System.Collections.Generic.LinkedList<T> input) {
            enumerator = input.GetEnumerator();
        }
        public void close() {
            // no action
        }

        public Item next() {
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }
            return null;
        }

        public void discharge() {
            // no action
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
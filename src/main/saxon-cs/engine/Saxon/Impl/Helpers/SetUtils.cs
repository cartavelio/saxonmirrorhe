using System.Linq;
using java.lang;
using net.sf.saxon.functions;

namespace Saxon.Impl.Helpers {
    public class SetUtils {
        public static bool AddAll<T>(System.Collections.Generic.ISet<T> input,
            System.Collections.Generic.ICollection<T> extra) {
            foreach (T item in extra) {
                input.Add(item);
            }

            return extra.Count > 0;
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
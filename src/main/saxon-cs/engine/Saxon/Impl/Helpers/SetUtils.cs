using System.Linq;
using java.lang;

namespace Saxon.Impl.Helpers {
    public class SetUtils {
        public static bool AddAll<T>(System.Collections.Generic.ISet<T> input,
            System.Collections.Generic.ICollection<T> extra) {
            foreach (T item in extra) {
                input.Add(item);
            }

            return extra.Count > 0;
        }
    }
}
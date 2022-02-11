using System.Collections;
using System.Collections.Generic;
using java.util;
using SysCollGen = System.Collections.Generic;
using System.Linq;
namespace Saxon.Impl.Helpers {
    
    /**
     * Supplies implementation of GetEnumerator() to a class that only supplies iterator()
     */
    public abstract class AbstractIterable<T> : java.lang.Iterable<T> {
        IEnumerator IEnumerable.GetEnumerator() {
            return new EnumeratorFromIterator<T>(iterator());
        }

        public IEnumerator<T> GetEnumerator() {
            return new EnumeratorFromIterator<T>(iterator());
        }

        public abstract Iterator<T> iterator();

        public static SysCollGen.IList<T> subList<T>(SysCollGen.IList<T> input, int start, int end) {
            if (input is SysCollGen.List<T>) {
                return ((SysCollGen.List<T>) input).GetRange(start, end - start);
            }
            else {
                return input.Skip(start).Take(end - start).ToList();
            }
        }
    }
}
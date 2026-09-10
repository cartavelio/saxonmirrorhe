
using SysCollGen = System.Collections.Generic;
using System.Linq;

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
        
        public static void TrimToSize<T>(SysCollGen.IList<T> input) {
            if (input is SysCollGen.List<T>) {
                // TrimToSize is available only with List<T>
                ((SysCollGen.List<T>) input).TrimExcess();
            }
        }
    }
}
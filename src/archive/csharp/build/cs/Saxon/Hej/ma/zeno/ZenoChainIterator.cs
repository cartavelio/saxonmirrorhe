using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.zeno {
    public class ZenoChainIterator<U>  : System.Collections.Generic.IEnumerator<U> {
        private int majorIndex = 0;
        private int minorIndex = 0;
        private readonly System.Collections.Generic.List<System.Collections.Generic.List<U>> masterList;
        public ZenoChainIterator(System.Collections.Generic.List<System.Collections.Generic.List<U>> masterList)  {
            this.masterList = masterList;
        }
        public bool hasNext() {
            return ((majorIndex<masterList.Count) && (minorIndex<masterList[majorIndex].Count));
        }
        public U next() {
            System.Collections.Generic.List<U> currentSegment = masterList[majorIndex];
            U result = currentSegment[minorIndex];
            if ((++minorIndex>=currentSegment.Count)) {
                majorIndex++;
                minorIndex = 0;
            }
            return result;
        }
        
    /* Add methods to implement IEnumerable */

        object System.Collections.IEnumerator.Current => Current;
        public U Current { get; private set; }
        public bool MoveNext() {
            if (hasNext()) {Current = next(); return true;} else return false;
        }
        public void Reset() {throw new NotImplementedException();}
        public void Dispose() {}
      
    }
}
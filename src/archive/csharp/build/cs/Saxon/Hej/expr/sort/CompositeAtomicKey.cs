using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class CompositeAtomicKey {
        private System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> keys;
        public CompositeAtomicKey(System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> keys)  {
            this.keys = keys;
        }
        public override bool Equals(System.Object obj) {
            return obj is CompositeAtomicKey && Saxon.Impl.Helpers.ListUtils.EqualLists(keys, ((CompositeAtomicKey)obj).keys);
        }
        public override int GetHashCode() {
            return Saxon.Impl.Helpers.ListUtils.ListHashCode(keys);
        }
    
    }
}
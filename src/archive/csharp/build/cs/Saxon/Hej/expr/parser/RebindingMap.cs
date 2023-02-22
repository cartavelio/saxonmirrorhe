using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class RebindingMap {
        private System.Collections.Generic.IDictionary<Saxon.Hej.expr.Binding,Saxon.Hej.expr.Binding> map = null;
        public void put(Saxon.Hej.expr.Binding oldBinding, Saxon.Hej.expr.Binding newBinding) {
            if ((map == null)) {
                map = new Saxon.Ejava.util.IdentityHashMap<Saxon.Hej.expr.Binding,Saxon.Hej.expr.Binding>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(map, oldBinding, newBinding);
        }
        public Saxon.Hej.expr.Binding get(Saxon.Hej.expr.Binding oldBinding) {
            return ((map == null) ? null : Saxon.Impl.Helpers.MapUtils.Get(map, oldBinding));
        }
    
    }
}
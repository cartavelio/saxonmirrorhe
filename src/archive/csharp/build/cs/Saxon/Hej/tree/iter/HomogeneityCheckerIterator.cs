using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class HomogeneityCheckerIterator : Saxon.Hej.om.SequenceIterator {
        internal Saxon.Hej.om.SequenceIterator @base = null;
        internal Saxon.Hej.s9api.Location loc;
        internal int state;
        public HomogeneityCheckerIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.s9api.Location loc)  {
            this.@base = @base;
            this.loc = loc;
            state = 0;
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
        private Saxon.Hej.trans.UncheckedXPathException reportMixedItems() {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot mix nodes and atomic values in the result of a path expression");
            err.setErrorCode("XPTY0018");
            err.setLocator(loc);
            return new Saxon.Hej.trans.UncheckedXPathException(err);
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item item = @base.next();
            if ((item == null)) {
                return null;
            }
            if ((state == 0)) {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> nodes = new System.Collections.Generic.List<Saxon.Hej.om.Item>(50);
                    nodes.Add(item);
                    while (((item = @base.next()) != null)) {
                        if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                            throw reportMixedItems();
                        } else {
                            nodes.Add(item);
                        }
                    }
                    @base = new Saxon.Hej.expr.sort.DocumentOrderIterator(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(nodes), Saxon.Hej.expr.sort.GlobalOrderComparer.getInstance());
                    state = 1;
                    return @base.next();
                } else {
                    state = -1;
                }
            } else {
                if (((state == -1) && (item is Saxon.Hej.om.NodeInfo))) {
                    throw reportMixedItems();
                }
            }
            return item;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class AdjacentTextNodeMergingIterator : Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Ejava.io.Closeable {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private Saxon.Hej.om.Item _next;
        public AdjacentTextNodeMergingIterator(Saxon.Hej.om.SequenceIterator @base)  {
            try {
                this.@base = @base;
                _next = @base.next();
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
        }
        public bool hasNext() {
            return (_next != null);
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item current = _next;
            if ((current == null)) {
                return null;
            }
            _next = @base.next();
            if (Saxon.Hej.expr.AdjacentTextNodeMerger.isTextNode(current)) {
                Saxon.Hej.str.UnicodeBuilder ub = new Saxon.Hej.str.UnicodeBuilder();
                ub.accept(current.getUnicodeStringValue());
                while (Saxon.Hej.expr.AdjacentTextNodeMerger.isTextNode(_next)) {
                    ub.accept(_next.getUnicodeStringValue());
                    _next = @base.next();
                }
                if (ub.isEmpty()) {
                    return next();
                } else {
                    Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(((Saxon.Hej.om.NodeInfo)(current)).getConfiguration());
                    o.setNodeKind(Saxon.Hej.type.Type.TEXT);
                    o.setStringValue(ub.toUnicodeString());
                    current = o;
                    return current;
                }
            } else {
                return current;
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
    
    }
}
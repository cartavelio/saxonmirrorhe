using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class UntypedAtomizingIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        public UntypedAtomizingIterator(Saxon.Hej.om.SequenceIterator @base)  {
            this.@base = @base;
        }
        public Saxon.Hej.om.Item next() {
            try {
                Saxon.Hej.om.Item nextSource = @base.next();
                if ((nextSource == null)) {
                    return null;
                } else {
                    return (Saxon.Hej.value.AtomicValue)(nextSource.atomize());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            @base.close();
        }
        public bool supportsGetLength() {
            return Saxon.Hej.om.SequenceTool.supportsGetLength(@base);
        }
        public int getLength() {
            return Saxon.Hej.om.SequenceTool.getLength(@base);
        }
        public bool supportsHasNext() {
            return ((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext());
        }
        public bool hasNext() {
            return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
        }
    
    }
}
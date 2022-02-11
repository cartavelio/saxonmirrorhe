using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemCheckingIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.LastPositionFinder {
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private readonly Saxon.Ejava.util.function.Consumer<Saxon.Hej.om.Item> action;
        public ItemCheckingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Ejava.util.function.Consumer<Saxon.Hej.om.Item> action)  {
            this.@base = @base;
            this.action = action;
        }
        public bool supportsHasNext() {
            return ((@base is Saxon.Hej.tree.iter.LookaheadIterator) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext());
        }
        public bool hasNext() {
            return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item nextSource = @base.next();
            if ((nextSource == null)) {
                return null;
            }
            action(nextSource);
            return nextSource;
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
        public bool supportsGetLength() {
            return Saxon.Hej.om.SequenceTool.supportsGetLength(@base);
        }
        public int getLength() {
            return Saxon.Hej.om.SequenceTool.getLength(@base);
        }
    
    }
}
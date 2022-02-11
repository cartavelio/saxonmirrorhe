using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemMappingIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.LastPositionFinder {
    // Injecting C# code
        public ItemMappingIterator(Saxon.Hej.om.SequenceIterator first, Saxon.Hej.expr.ItemMapper.Lambda action) : this(first, Saxon.Hej.expr.ItemMapper.of(action)) {}
        public ItemMappingIterator(Saxon.Hej.om.SequenceIterator first, Saxon.Hej.expr.ItemMapper.Lambda action, bool oneToOne) : this(first, Saxon.Hej.expr.ItemMapper.of(action), oneToOne) {}
    // End of injected C# code
        private readonly Saxon.Hej.om.SequenceIterator @base;
        private readonly Saxon.Hej.expr.ItemMappingFunction action;
        private bool oneToOne = false;
        public ItemMappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMappingFunction action)  {
            this.@base = @base;
            this.action = action;
        }
        public static Saxon.Hej.expr.ItemMappingIterator map(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMapper.Lambda mappingExpression) {
            return new Saxon.Hej.expr.ItemMappingIterator(@base, Saxon.Hej.expr.ItemMapper.of(mappingExpression));
        }
        public static Saxon.Hej.expr.ItemMappingIterator filter(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemFilter.Lambda filterExpression) {
            return new Saxon.Hej.expr.ItemMappingIterator(@base, Saxon.Hej.expr.ItemFilter.of(filterExpression));
        }
        public ItemMappingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.expr.ItemMappingFunction action, bool oneToOne)  {
            this.@base = @base;
            this.action = action;
            this.oneToOne = oneToOne;
        }
        public void setOneToOne(bool oneToOne) {
            this.oneToOne = oneToOne;
        }
        public bool isOneToOne() {
            return oneToOne;
        }
        protected internal Saxon.Hej.om.SequenceIterator getBaseIterator() {
            return @base;
        }
        protected internal Saxon.Hej.expr.ItemMappingFunction getMappingFunction() {
            return action;
        }
        public bool supportsHasNext() {
            return ((oneToOne && (@base is Saxon.Hej.tree.iter.LookaheadIterator)) && ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).supportsHasNext());
        }
        public bool hasNext() {
            return ((Saxon.Hej.tree.iter.LookaheadIterator)(@base)).hasNext();
        }
        public virtual  /* REPLACING public */ Saxon.Hej.om.Item next() {
            try {
                while (true) {
                    Saxon.Hej.om.Item nextSource = @base.next();
                    if ((nextSource == null)) {
                        return null;
                    }
                    Saxon.Hej.om.Item current = action.mapItem(nextSource);
                    if ((current != null)) {
                        return current;
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public virtual  /* REPLACING public */ void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
        public bool supportsGetLength() {
            return (oneToOne && Saxon.Hej.om.SequenceTool.supportsGetLength(@base));
        }
        public int getLength() {
            return Saxon.Hej.om.SequenceTool.getLength(@base);
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class MergeIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.om.SequenceIterator e1;
        private readonly Saxon.Hej.om.SequenceIterator e2;
        private Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> nextItem1 = null;
        private Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> nextItem2 = null;
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> comparer;
        public MergeIterator(Saxon.Hej.om.SequenceIterator p1, Saxon.Hej.om.SequenceIterator p2, System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> comparer)  {
            this.e1 = p1;
            this.e2 = p2;
            this.comparer = comparer;
            nextItem1 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e1.next());
            nextItem2 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e2.next());
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return ((nextItem1 != null) || (nextItem2 != null));
        }
        public Saxon.Hej.om.Item next() {
            if (((nextItem1 != null) && (nextItem2 != null))) {
                int c;
                try {
                    c = comparer.Compare(nextItem1, nextItem2);
                } catch (System.InvalidCastException e) {
                    Saxon.Hej.expr.sort.ItemWithMergeKeys i1 = nextItem1.getObject();
                    Saxon.Hej.expr.sort.ItemWithMergeKeys i2 = nextItem2.getObject();
                    Saxon.Hej.value.AtomicValue a1 = i1.sortKeyValues[0];
                    Saxon.Hej.value.AtomicValue a2 = i2.sortKeyValues[0];
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Merge key values are of non-comparable types ("+Saxon.Hej.type.Type.displayTypeName(a1))+" and ")+Saxon.Hej.type.Type.displayTypeName(a2))+")"), "XTTE2230");
                    err.setIsTypeError(true);
                    throw new Saxon.Hej.trans.UncheckedXPathException(err);
                }
                if ((c<=0)) {
                    Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> current = nextItem1;
                    nextItem1 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e1.next());
                    return current;
                } else {
                    Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> current = nextItem2;
                    nextItem2 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e2.next());
                    return current;
                }
            }
            if ((nextItem1 != null)) {
                Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> current = nextItem1;
                nextItem1 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e1.next());
                return current;
            }
            if ((nextItem2 != null)) {
                Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> current = nextItem2;
                nextItem2 = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(e2.next());
                return current;
            }
            return null;
        }
        public void close() {
            e1.close();
            e2.close();
        }
        public void discharge() {
            e1.discharge();
            e2.discharge();
        }
    
    }
}
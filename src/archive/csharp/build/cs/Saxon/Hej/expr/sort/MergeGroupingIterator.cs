using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class MergeGroupingIterator : Saxon.Hej.expr.sort.GroupIterator, Saxon.Hej.tree.iter.LookaheadIterator, Saxon.Hej.expr.LastPositionFinder {
        private readonly Saxon.Hej.om.SequenceIterator baseItr;
        private Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> currenti = null;
        private Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> nextItem;
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers;
        private System.Collections.Generic.IDictionary<string,System.Collections.Generic.IList<Saxon.Hej.om.Item>> currentSourceMembers;
        private readonly System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> comparer;
        private int position = 0;
        internal System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> compositeMergeKey;
        private readonly Saxon.Hej.expr.LastPositionFinder lastPositionFinder;
        public MergeGroupingIterator(Saxon.Hej.om.SequenceIterator p1, System.Collections.Generic.Comparer<Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>> comp, Saxon.Hej.expr.LastPositionFinder lpf)  {
            this.baseItr = p1;
            nextItem = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(p1.next());
            if ((nextItem != null)) {
                compositeMergeKey = nextItem.getObject().sortKeyValues;
            }
            this.comparer = comp;
            this.lastPositionFinder = lpf;
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.MergeGroupingIterator.ManualMergeGroupingIterator(currentMembers, getCurrentGroupingKey(), position, currenti);
        }
        private void advance() {
            currentMembers = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            currentSourceMembers = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IList<Saxon.Hej.om.Item>>(20);
            Saxon.Hej.om.Item currentItem = currenti.getObject().baseItem;
            string source = currenti.getObject().sourceName;
            currentMembers.Add(currentItem);
            if ((source != null)) {
                System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                list.Add(currentItem);
                Saxon.Impl.Helpers.MapUtils.Put(currentSourceMembers, source, list);
            }
            while (true) {
                Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> nextCandidate = (Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys>)(baseItr.next());
                if ((nextCandidate == null)) {
                    nextItem = null;
                    return ;
                }
                try {
                    int c = comparer.Compare(currenti, nextCandidate);
                    if ((c == 0)) {
                        currentItem = nextCandidate.getObject().baseItem;
                        source = nextCandidate.getObject().sourceName;
                        currentMembers.Add(currentItem);
                        if ((source != null)) {
                            System.Collections.Generic.IList<Saxon.Hej.om.Item> list = Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(currentSourceMembers, source, k => new System.Collections.Generic.List<Saxon.Hej.om.Item>());
                            list.Add(currentItem);
                        }
                    } else {
                        if ((c>0)) {
                            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> keys = nextCandidate.getObject().sortKeyValues;
                            throw new Saxon.Hej.trans.XPathException(((("Merge input for source "+source)+" is not ordered according to merge key, detected at key value: ")+Saxon.Ejava.util.Arrays.ToString(keys.ToArray())), "XTDE2220");
                        } else {
                            nextItem = nextCandidate;
                            return ;
                        }
                    }
                } catch (System.InvalidCastException e) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Merge key values are of non-comparable types ("+Saxon.Hej.type.Type.displayTypeName(currentItem))+" and ")+Saxon.Hej.type.Type.displayTypeName(nextCandidate.getObject().baseItem))+')'), "XTTE2230");
                    err.setIsTypeError(true);
                    throw err;
                }
            }
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (nextItem != null);
        }
        public Saxon.Hej.om.Item next() {
            try {
                if ((nextItem == null)) {
                    currenti = null;
                    position = -1;
                    return null;
                }
                currenti = nextItem;
                position++;
                compositeMergeKey = nextItem.getObject().sortKeyValues;
                advance();
                return currenti.getObject().baseItem;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            baseItr.close();
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return lastPositionFinder.getLength();
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return new Saxon.Hej.om.AtomicArray(compositeMergeKey);
        }
        public Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentMembers);
        }
        public Saxon.Hej.om.SequenceIterator iterateCurrentGroup(string source) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> sourceMembers = Saxon.Impl.Helpers.MapUtils.Get(currentSourceMembers, source);
            if ((sourceMembers == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.emptyIterator();
            } else {
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(sourceMembers);
            }
        }
        public class ManualMergeGroupingIterator : Saxon.Eej.ee.stream.ManualGroupIterator {
            internal System.Collections.Generic.IList<Saxon.Hej.om.Item> currentGroup;
            internal Saxon.Hej.om.AtomicSequence currentGroupingKey;
            internal Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> currenti;
            public ManualMergeGroupingIterator(System.Collections.Generic.IList<Saxon.Hej.om.Item> currentGroup, Saxon.Hej.om.AtomicSequence currentGroupingKey, int position, Saxon.Hej.value.ObjectValue<Saxon.Hej.expr.sort.ItemWithMergeKeys> currenti) : base (currenti.getObject().baseItem, position) {
                this.currentGroup = currentGroup;
                this.currentGroupingKey = currentGroupingKey;
                this.currenti = currenti;
                setLengthFinder(() => currentGroup.Count);
            }
            public override Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentGroup);
            }
            public override Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
                return currentGroupingKey;
            }
            public override int getLength() {
                return currentGroup.Count;
            }
        
        }
    
    }
}
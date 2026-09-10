using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupAdjacentIterator : Saxon.Hej.expr.sort.GroupIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        private readonly Saxon.Hej.expr.Expression select;
        private readonly Saxon.Hej.om.FocusIterator population;
        private readonly Saxon.Hej.expr.Expression keyExpression;
        private readonly Saxon.Hej.lib.StringCollator collator;
        private readonly Saxon.Hej.expr.XPathContext baseContext;
        private readonly Saxon.Hej.expr.XPathContext runningContext;
        private Saxon.Hej.expr.sort.CompositeAtomicKey currentComparisonKey;
        private Saxon.Hej.om.AtomicSequence currentKey;
        private System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers;
        private Saxon.Hej.expr.sort.CompositeAtomicKey nextComparisonKey;
        private System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> nextKey = null;
        private Saxon.Hej.om.Item nextItem;
        private Saxon.Hej.om.Item current = null;
        private int position = 0;
        private bool composite = false;
        public GroupAdjacentIterator(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression keyExpression, Saxon.Hej.expr.XPathContext baseContext, Saxon.Hej.lib.StringCollator collator, bool composite)  {
            this.select = select;
            this.keyExpression = keyExpression;
            this.baseContext = baseContext;
            this.runningContext = baseContext.newMinorContext();
            this.population = runningContext.trackFocus(select.iterate(baseContext));
            this.collator = collator;
            this.composite = composite;
            nextItem = population.next();
            if ((nextItem != null)) {
                nextKey = getKey(runningContext);
                nextComparisonKey = getComparisonKey(nextKey, baseContext);
            }
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            try {
                Saxon.Hej.expr.sort.GroupAdjacentIterator another = new Saxon.Hej.expr.sort.GroupAdjacentIterator(select, keyExpression, baseContext, collator, composite);
                return Saxon.Hej.functions.Count.steppingCount(another);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        private System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> getKey(Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> key = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            Saxon.Hej.om.SequenceIterator iter = keyExpression.iterate(context);
            while (true) {
                Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(iter.next());
                if ((val == null)) {
                    break ;
                }
                key.Add(val);
            }
            return key;
        }
        private Saxon.Hej.expr.sort.CompositeAtomicKey getComparisonKey(System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> key, Saxon.Hej.expr.XPathContext keyContext) {
            System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> ckey = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>(key.Count);
            foreach (Saxon.Hej.value.AtomicValue aKey in key) {
                Saxon.Hej.expr.sort.AtomicMatchKey comparisonKey;
                if (aKey.isNaN()) {
                    comparisonKey = Saxon.Hej.functions.DistinctValues.NaN_MATCH_KEY;
                } else {
                    comparisonKey = aKey.getXPathMatchKey(collator, keyContext.getImplicitTimezone());
                }
                ckey.Add(comparisonKey);
            }
            return new Saxon.Hej.expr.sort.CompositeAtomicKey(ckey);
        }
        private void advance() {
            currentMembers = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            currentMembers.Add(current);
            while (true) {
                Saxon.Hej.om.Item nextCandidate = population.next();
                if ((nextCandidate == null)) {
                    break ;
                }
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> newKey = getKey(runningContext);
                Saxon.Hej.expr.sort.CompositeAtomicKey newComparisonKey = getComparisonKey(newKey, baseContext);
                try {
                    if (newComparisonKey.Equals(currentComparisonKey)) {
                        currentMembers.Add(nextCandidate);
                    } else {
                        nextItem = nextCandidate;
                        nextComparisonKey = newComparisonKey;
                        nextKey = newKey;
                        return ;
                    }
                } catch (System.InvalidCastException e) {
                    string message = "Grouping key values are of non-comparable types";
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                    err.setIsTypeError(true);
                    err.setXPathContext(runningContext);
                    throw err;
                }
            }
            nextItem = null;
            nextKey = null;
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() {
            return currentKey;
        }
        public Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentMembers);
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
                    current = null;
                    position = -1;
                    return null;
                }
                current = nextItem;
                if ((nextKey.Count == 1)) {
                    currentKey = nextKey[0];
                } else {
                    currentKey = new Saxon.Hej.om.AtomicArray(nextKey);
                }
                currentComparisonKey = nextComparisonKey;
                position++;
                advance();
                return current;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            population.close();
        }
        public void discharge() {
            population.discharge();
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.sort.GroupAdjacentIterator.ManualGroupAdjacentIterator(current, position, currentMembers, currentKey, runningContext.newMinorContext());
        }
        private class ManualGroupAdjacentIterator : Saxon.Eej.ee.stream.ManualGroupIterator {
            private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers;
            private readonly Saxon.Hej.expr.XPathContext savedcontext;
            internal ManualGroupAdjacentIterator(Saxon.Hej.om.Item current, int position, System.Collections.Generic.IList<Saxon.Hej.om.Item> currentMembers, Saxon.Hej.om.AtomicSequence currentKey, Saxon.Hej.expr.XPathContext savedContext) : base (current, position) {
                this.currentMembers = currentMembers;
                this.savedcontext = savedContext;
                setCurrentGroupingKey(currentKey);
                setLengthFinder(() => savedcontext.getLast());
            }
            public override Saxon.Hej.om.SequenceIterator iterateCurrentGroup() {
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(currentMembers);
            }
        
        }
    
    }
}
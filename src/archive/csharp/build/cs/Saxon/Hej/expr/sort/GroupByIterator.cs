using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class GroupByIterator : Saxon.Hej.expr.sort.GroupIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
        private Saxon.Hej.om.SequenceIterator population;
        protected internal Saxon.Hej.expr.Expression keyExpression;
        private Saxon.Hej.lib.StringCollator collator;
        private Saxon.Hej.expr.XPathContext keyContext;
        private int position = 0;
        protected internal System.Collections.Generic.IList<System.Collections.Generic.IList<Saxon.Hej.om.Item>> groups = new System.Collections.Generic.List<System.Collections.Generic.IList<Saxon.Hej.om.Item>>(40);
        protected internal System.Collections.Generic.IList<Saxon.Hej.om.AtomicSequence> groupKeys = new System.Collections.Generic.List<Saxon.Hej.om.AtomicSequence>(40);
        protected internal bool composite;
        public GroupByIterator(Saxon.Hej.om.SequenceIterator population, Saxon.Hej.expr.Expression keyExpression, Saxon.Hej.expr.XPathContext keyContext, Saxon.Hej.lib.StringCollator collator, bool composite)  {
            this.population = population;
            this.keyExpression = keyExpression;
            this.keyContext = keyContext;
            this.collator = collator;
            this.composite = composite;
            if (composite) {
                buildIndexedGroupsComposite();
            } else {
                buildIndexedGroups();
            }
        }
        public GroupByIterator()  {
        }
        private void buildIndexedGroups() {
            System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Collections.Generic.IList<Saxon.Hej.om.Item>> index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, System.Collections.Generic.IList<Saxon.Hej.om.Item>>(40);
            Saxon.Hej.expr.XPathContext c2 = keyContext.newMinorContext();
            Saxon.Hej.om.FocusIterator focus = c2.trackFocus(population);
            int implicitTimezone = c2.getImplicitTimezone();
            Saxon.Hej.om.Item item;
            while (((item = focus.next()) != null)) {
                Saxon.Hej.om.SequenceIterator keys = keyExpression.iterate(c2);
                bool firstKey = true;
                while (true) {
                    Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(keys.next());
                    if ((key == null)) {
                        break ;
                    }
                    Saxon.Hej.expr.sort.AtomicMatchKey comparisonKey;
                    if (key.isNaN()) {
                        comparisonKey = Saxon.Hej.functions.DistinctValues.NaN_MATCH_KEY;
                    } else {
                        comparisonKey = key.getXPathMatchKey(collator, implicitTimezone);
                    }
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> g = Saxon.Impl.Helpers.MapUtils.Get(index, comparisonKey);
                    if ((g == null)) {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> newGroup = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
                        newGroup.Add(item);
                        groups.Add(newGroup);
                        groupKeys.Add(key);
                        Saxon.Impl.Helpers.MapUtils.Put(index, comparisonKey, newGroup);
                    } else {
                        if (firstKey) {
                            g.Add(item);
                        } else {
                            if ((g[(g.Count-1)] != item)) {
                                g.Add(item);
                            }
                        }
                    }
                    firstKey = false;
                }
            }
        }
        private void buildIndexedGroupsComposite() {
            System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.CompositeAtomicKey,System.Collections.Generic.IList<Saxon.Hej.om.Item>> index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.CompositeAtomicKey, System.Collections.Generic.IList<Saxon.Hej.om.Item>>(40);
            Saxon.Hej.expr.XPathContext c2 = keyContext.newMinorContext();
            Saxon.Hej.om.FocusIterator focus = c2.trackFocus(population);
            int implicitTimezone = c2.getImplicitTimezone();
            Saxon.Hej.om.Item item;
            while (((item = focus.next()) != null)) {
                Saxon.Hej.om.SequenceIterator keys = keyExpression.iterate(c2);
                System.Collections.Generic.IList<Saxon.Hej.expr.sort.AtomicMatchKey> ckList = new System.Collections.Generic.List<Saxon.Hej.expr.sort.AtomicMatchKey>();
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> compositeKey = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                while (true) {
                    Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(keys.next());
                    if ((key == null)) {
                        break ;
                    }
                    compositeKey.Add(key);
                    Saxon.Hej.expr.sort.AtomicMatchKey comparisonKey;
                    if (key.isNaN()) {
                        comparisonKey = Saxon.Hej.functions.DistinctValues.NaN_MATCH_KEY;
                    } else {
                        comparisonKey = key.getXPathMatchKey(collator, implicitTimezone);
                    }
                    ckList.Add(comparisonKey);
                }
                Saxon.Hej.expr.sort.CompositeAtomicKey cak = new Saxon.Hej.expr.sort.CompositeAtomicKey(ckList);
                System.Collections.Generic.IList<Saxon.Hej.om.Item> g = Saxon.Impl.Helpers.MapUtils.Get(index, cak);
                if ((g == null)) {
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> newGroup = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
                    newGroup.Add(item);
                    groups.Add(newGroup);
                    groupKeys.Add(new Saxon.Hej.om.AtomicArray(compositeKey));
                    Saxon.Impl.Helpers.MapUtils.Put(index, cak, newGroup);
                } else {
                    g.Add(item);
                }
            }
        }
        public Saxon.Hej.om.AtomicSequence getCurrentGroupingKey() { lock(this) {
            Saxon.Hej.om.AtomicSequence val = groupKeys[(position-1)];
            if ((val == null)) {
                return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
            } else {
                return val;
            }
        }}
        public Saxon.Hej.om.GroundedValue currentGroup() {
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(groups[(position-1)]);
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.Item> getCurrentGroup() {
            return groups[(position-1)];
        }
        public bool supportsHasNext() {
            return true;
        }
        public bool hasNext() {
            return (position<groups.Count);
        }
        public Saxon.Hej.om.Item next() {
            if (((position>=0) && (position<groups.Count))) {
                position++;
                return current();
            } else {
                position = -1;
                return null;
            }
        }
        private Saxon.Hej.om.Item current() {
            if ((position<1)) {
                return null;
            }
            return groups[(position-1)][0];
        }
        public bool supportsGetLength() {
            return true;
        }
        public int getLength() {
            return groups.Count;
        }
        public Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.ManualGroupIterator iter = new Saxon.Eej.ee.stream.ManualGroupIterator(groups[(position-1)][0], position);
            iter.setCurrentGroup(Saxon.Hej.value.SequenceExtent.makeSequenceExtent(groups[(position-1)]));
            iter.setCurrentGroupingKey(groupKeys[(position-1)]);
            iter.setLengthFinder(() => groups.Count);
            return iter;
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

    }
}
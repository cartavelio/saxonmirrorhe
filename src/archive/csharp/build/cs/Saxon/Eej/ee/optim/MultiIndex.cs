using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class MultiIndex {
        private readonly Saxon.Hej.om.GroundedValue indexedValue;
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.optim.MultiIndex.ValueIndex> indexList = new System.Collections.Generic.List<Saxon.Eej.ee.optim.MultiIndex.ValueIndex>(4);
        public MultiIndex(Saxon.Hej.om.GroundedValue indexedValue)  {
            this.indexedValue = indexedValue;
        }
        private Saxon.Eej.ee.optim.MultiIndex.ValueIndex buildIndex(Saxon.Hej.expr.Expression use, Saxon.Hej.type.BuiltInAtomicType untypedType, Saxon.Eej.ee.optim.MultiIndex.PrimitiveTypeTester tester, Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.StringCollator collator) {
            System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.z.MonotonicIntSet> index = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey, Saxon.Hej.z.MonotonicIntSet>(100);
            Saxon.Eej.ee.optim.MultiIndex.ValueIndex vi = new Saxon.Eej.ee.optim.MultiIndex.ValueIndex(untypedType, index);
            Saxon.Hej.z.IntHashSet typesPresent = new Saxon.Hej.z.IntHashSet(4);
            Saxon.Hej.expr.XPathContext c2 = context.newContext();
            Saxon.Hej.om.FocusIterator iter = c2.trackFocus(indexedValue.iterate());
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            Saxon.Hej.type.StringConverter untypedConverter = untypedType.getStringConverter(rules);
            int implicitTimezone = context.getImplicitTimezone();
            int pos = 0;
            while ((iter.next() != null)) {
                Saxon.Hej.om.SequenceIterator vals = use.iterate(c2);
                Saxon.Hej.value.AtomicValue val;
                while (((val = (Saxon.Hej.value.AtomicValue)(vals.next())) != null)) {
                    Saxon.Hej.value.AtomicValue val2 = val;
                    if (val.isUntypedAtomic()) {
                        Saxon.Hej.type.ConversionResult result = untypedConverter.convert(val);
                        if ((result is Saxon.Hej.type.ValidationFailure)) {
                            string msg = ((("Failure converting untypedAtomic value to "+untypedType.getDisplayName())+" during comparison operation. ")+((Saxon.Hej.type.ValidationFailure)(result)).getMessage());
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(msg);
                            de.setErrorCode("FORG0001");
                            de.setXPathContext(context);
                            throw de;
                        } else {
                            val2 = (Saxon.Hej.value.AtomicValue)(result);
                        }
                    }
                    bool added = typesPresent.add(val2.getPrimitiveType().getFingerprint());
                    if (added) {
                        /*Delegate.test()*/tester(val.getPrimitiveType());
                    }
                    Saxon.Hej.expr.sort.AtomicMatchKey x = val2.getXPathMatchKey(collator, implicitTimezone);
                    Saxon.Hej.z.MonotonicIntSet list = Saxon.Impl.Helpers.MapUtils.Get(index, x);
                    if ((list == null)) {
                        list = new Saxon.Hej.z.MonotonicIntSet();
                        Saxon.Impl.Helpers.MapUtils.Put(index, x, list);
                    }
                    list.add(pos);
                }
                pos++;
            }
            vi.typesPresent = new Saxon.Hej.z.IntArraySet(typesPresent);
            return vi;
        }
        private Saxon.Eej.ee.optim.MultiIndex.ValueIndex findIndex(Saxon.Hej.type.BuiltInAtomicType soughtType) {
            for (int i = (indexList.Count-1); (i>=0); i--) {
                Saxon.Eej.ee.optim.MultiIndex.ValueIndex vi = indexList[i];
                if (vi.untypedType.Equals(soughtType)) {
                    return vi;
                }
            }
            return null;
        }
        public Saxon.Hej.om.SequenceIterator findItems(Saxon.Hej.expr.Expression use, Saxon.Hej.om.SequenceIterator values, bool convertUntypedToOther, bool firstOnly, Saxon.Hej.lib.StringCollator collator, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.z.IntSet selectedItems = null;
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            int implicitTimezone = context.getImplicitTimezone();
            Saxon.Hej.value.AtomicValue value;
            while (((value = (Saxon.Hej.value.AtomicValue)(values.next())) != null)) {
                Saxon.Hej.type.BuiltInAtomicType prim;
                if (convertUntypedToOther) {
                    prim = value.getPrimitiveType();
                    if (prim.Equals(Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                        prim = Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
                    }
                } else {
                    prim = Saxon.Hej.type.BuiltInAtomicType.STRING;
                }
                Saxon.Hej.type.BuiltInAtomicType soughtType = value.getPrimitiveType();
                Saxon.Eej.ee.optim.MultiIndex.ValueIndex vi = findIndex(prim);
                if ((vi == null)) {
                    Saxon.Eej.ee.optim.MultiIndex.PrimitiveTypeTester tester;
                    if (convertUntypedToOther) {
                        tester = type => {
                            if (((type == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || (soughtType == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                                return ;
                            }
                            if (!(Saxon.Hej.type.Type.isGuaranteedComparable(type, soughtType, false))) {
                                string message = ((("General comparison: cannot compare "+type)+" to ")+soughtType);
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                                err.setErrorCode("XPTY0004");
                                err.setIsTypeError(true);
                                throw err;
                            }
                        } ;
                    } else {
                        tester = type => {
                            Saxon.Hej.type.BuiltInAtomicType t0 = type;
                            if ((t0 == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                t0 = Saxon.Hej.type.BuiltInAtomicType.STRING;
                            }
                            Saxon.Hej.type.BuiltInAtomicType t1 = soughtType;
                            if ((t1 == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                t1 = Saxon.Hej.type.BuiltInAtomicType.STRING;
                            }
                            if (!(Saxon.Hej.type.Type.isGuaranteedComparable(t0, t1, false))) {
                                string message = ((("Value comparison: cannot compare "+type)+" to ")+soughtType);
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                                err.setErrorCode("XPTY0004");
                                err.setIsTypeError(true);
                                throw err;
                            }
                        } ;
                    }
                    vi = buildIndex(use, prim, tester, context, collator);
                    indexList.Add(vi);
                } else {
                    if (((soughtType != Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && !(vi.typesChecked.contains(soughtType.getFingerprint())))) {
                        Saxon.Hej.z.IntIterator ii = vi.typesPresent.iterator();
                        while (ii.hasNext()) {
                            int type = ii.next();
                            Saxon.Hej.type.BuiltInAtomicType indexedType = (Saxon.Hej.type.BuiltInAtomicType)(Saxon.Hej.type.BuiltInType.getSchemaType(type));
                            if (!(Saxon.Hej.type.Type.isGuaranteedComparable(soughtType, indexedType, false))) {
                                string message = ((("Cannot compare "+indexedType)+" to ")+soughtType);
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                                err.setErrorCode("XPTY0004");
                                err.setIsTypeError(true);
                                throw err;
                            }
                        }
                        vi.typesChecked.add(soughtType.getFingerprint());
                    }
                }
                if (prim.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    Saxon.Hej.z.IntIterator ii = vi.typesPresent.iterator();
                    while (ii.hasNext()) {
                        int type = ii.next();
                        Saxon.Hej.type.BuiltInAtomicType requiredType = (Saxon.Hej.type.BuiltInAtomicType)(Saxon.Hej.type.BuiltInType.getSchemaType(type));
                        Saxon.Hej.value.AtomicValue conv;
                        Saxon.Hej.type.ConversionResult result = Saxon.Hej.type.Converter.convert(value, requiredType, rules);
                        if ((result is Saxon.Hej.type.ValidationFailure)) {
                            string msg = ((("Failure converting untypedAtomic value to "+requiredType.getDisplayName())+" during comparison operation. ")+((Saxon.Hej.type.ValidationFailure)(result)).getMessage());
                            Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException(msg);
                            de.setErrorCode("FORG0001");
                            de.setXPathContext(context);
                            throw de;
                        } else {
                            conv = (Saxon.Hej.value.AtomicValue)(result);
                        }
                        Saxon.Hej.z.IntSet list = Saxon.Impl.Helpers.MapUtils.Get(vi.index, conv.getXPathMatchKey(collator, implicitTimezone));
                        if ((list != null)) {
                            if ((selectedItems == null)) {
                                selectedItems = list.copy();
                            } else {
                                selectedItems = selectedItems.union(list);
                            }
                        }
                    }
                } else {
                    Saxon.Hej.z.IntSet list = Saxon.Impl.Helpers.MapUtils.Get(vi.index, value.getXPathMatchKey(collator, implicitTimezone));
                    if ((list != null)) {
                        if ((selectedItems == null)) {
                            selectedItems = list.copy();
                        } else {
                            selectedItems = selectedItems.union(list);
                        }
                    }
                }
                if (((firstOnly && (selectedItems != null)) && !(selectedItems.isEmpty()))) {
                    Saxon.Hej.z.IntIterator it = selectedItems.iterator();
                    if (it.hasNext()) {
                        return singletonIterator(it.next());
                    }
                }
            }
            if ((selectedItems == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            }
            int size = selectedItems.size();
            if ((size == 0)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            } else {
                if ((size == 1)) {
                    Saxon.Hej.z.IntIterator it = selectedItems.iterator();
                    if (it.hasNext()) {
                        return singletonIterator(it.next());
                    }
                }
            }
            return new Saxon.Eej.ee.optim.MultiIndex.SelectedItemsIterator(indexedValue, selectedItems);
        }
        private Saxon.Hej.om.SequenceIterator singletonIterator(int pos) {
            return ((/*1430*/Saxon.Hej.tree.iter.SingletonIterator)Saxon.Hej.om.SequenceTool.itemAt(indexedValue, pos).iterate());
        }
        public class SelectedItemsIterator : Saxon.Hej.om.SequenceIterator, Saxon.Hej.expr.LastPositionFinder, Saxon.Hej.tree.iter.LookaheadIterator {
            internal Saxon.Hej.z.IntSet selection;
            internal Saxon.Hej.z.IntIterator iter;
            internal int current = 0;
            internal Saxon.Hej.om.GroundedValue indexedValue;
            public SelectedItemsIterator(Saxon.Hej.om.GroundedValue indexedValue, Saxon.Hej.z.IntSet selection)  {
                this.indexedValue = indexedValue;
                this.selection = selection;
                iter = selection.iterator();
            }
            public bool supportsHasNext() {
                return true;
            }
            public bool hasNext() {
                return iter.hasNext();
            }
            public Saxon.Hej.om.Item next() {
                if (iter.hasNext()) {
                    current = iter.next();
                    return Saxon.Hej.om.SequenceTool.itemAt(indexedValue, current);
                } else {
                    return null;
                }
            }
            public bool supportsGetLength() {
                return true;
            }
            public int getLength() {
                return selection.size();
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

        }
        private class ValueIndex {
            public Saxon.Hej.type.BuiltInAtomicType untypedType;
            public Saxon.Hej.z.IntSet typesPresent;
            public Saxon.Hej.z.IntSet typesChecked;
            public System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.z.MonotonicIntSet> index;
            public ValueIndex(Saxon.Hej.type.BuiltInAtomicType untypedType, System.Collections.Generic.Dictionary<Saxon.Hej.expr.sort.AtomicMatchKey,Saxon.Hej.z.MonotonicIntSet> index)  {
                this.untypedType = untypedType;
                typesPresent = new Saxon.Hej.z.IntArraySet();
                typesChecked = new Saxon.Hej.z.IntArraySet();
                this.index = index;
            }
        
        }
        private delegate void PrimitiveTypeTester (Saxon.Hej.type.BuiltInAtomicType type);
    
    }
}
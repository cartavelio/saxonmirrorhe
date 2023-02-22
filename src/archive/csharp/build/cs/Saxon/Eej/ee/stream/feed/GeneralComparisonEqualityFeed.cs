using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class GeneralComparisonEqualityFeed : Saxon.Eej.ee.stream.feed.AtomicItemFeed {
        private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private readonly Saxon.Hej.lib.StringCollator collator;
        private Saxon.Hej.z.IntHashMap<System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey>> lookupTable;
        private System.Collections.Generic.IList<Saxon.Hej.value.StringValue> untypedValues;
        private bool done = false;
        private readonly Saxon.Hej.expr.Expression pullOperand;
        private readonly int implicitTimezone;
        private bool closed;
        public GeneralComparisonEqualityFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression expr, int pushedArg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            this.watchManager = watchManager;
            Saxon.Hej.expr.GeneralComparison gc = (Saxon.Hej.expr.GeneralComparison)(getExpression());
            collator = gc.getStringCollator();
            pullOperand = ((pushedArg == 0) ? gc.getRhsExpression() : gc.getLhsExpression());
            implicitTimezone = context.getImplicitTimezone();
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            done = false;
            lookupTable = null;
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(done)) {
                Saxon.Hej.expr.XPathContext context = getContext();
                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                Saxon.Hej.value.AtomicValue next = (Saxon.Hej.value.AtomicValue)(item);
                Saxon.Hej.expr.sort.AtomicMatchKey k1 = next.getXPathMatchKey(collator, implicitTimezone);
                Saxon.Hej.type.BuiltInAtomicType matchType;
                if ((next is Saxon.Hej.value.NumericValue)) {
                    matchType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                } else {
                    matchType = ((Saxon.Hej.value.AtomicValue)(item)).getPrimitiveType();
                }
                if ((lookupTable == null)) {
                    lookupTable = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey>>(4);
                    untypedValues = new System.Collections.Generic.List<Saxon.Hej.value.StringValue>(4);
                    System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> strings = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(10);
                    lookupTable.put(Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC, strings);
                    System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> typed = null;
                    if (!((next.isUntypedAtomic()))) {
                        typed = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(10);
                        lookupTable.put(matchType.getFingerprint(), typed);
                    }
                    Saxon.Hej.om.SequenceIterator comparand = pullOperand.iterate(context);
                    Saxon.Hej.value.AtomicValue val;
                    while (((val = (Saxon.Hej.value.AtomicValue)(comparand.next())) != null)) {
                        if (val.isUntypedAtomic()) {
                            untypedValues.Add((Saxon.Hej.value.StringValue)(val));
                            strings.Add(((Saxon.Hej.value.StringValue)(val)).getXPathMatchKey(collator, implicitTimezone));
                            if ((typed != null)) {
                                Saxon.Hej.value.AtomicValue cval = Saxon.Hej.type.Converter.convert(val, matchType, rules);
                                typed.Add(cval.getXPathMatchKey(collator, implicitTimezone));
                                if ((matchType != Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                    strings.Add(val.getXPathMatchKey(collator, implicitTimezone));
                                }
                            }
                        } else {
                            Saxon.Hej.type.BuiltInAtomicType prim;
                            if ((val is Saxon.Hej.value.NumericValue)) {
                                prim = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                            } else {
                                prim = val.getPrimitiveType();
                            }
                            System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> keys = lookupTable.get(prim.getFingerprint());
                            if ((keys == null)) {
                                keys = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(10);
                                lookupTable.put(prim.getFingerprint(), keys);
                            }
                            keys.Add(val.getXPathMatchKey(collator, implicitTimezone));
                        }
                    }
                    foreach (Saxon.Hej.value.StringValue untypedVal in untypedValues) {
                        Saxon.Hej.z.IntIterator types = lookupTable.keyIterator();
                        while (types.hasNext()) {
                            int type = types.next();
                            System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> keys = lookupTable.get(type);
                            Saxon.Hej.type.BuiltInAtomicType atomicType = (Saxon.Hej.type.BuiltInAtomicType)(Saxon.Hej.type.BuiltInType.getSchemaType(type));
                            keys.Add(Saxon.Hej.type.Converter.convert(untypedVal, atomicType, rules).getXPathMatchKey(collator, implicitTimezone));
                        }
                    }
                    if (((lookupTable.size() == 0) && untypedValues.Count==0)) {
                        Saxon.Hej.@event.Outputter @out = getNextOutputter();
                        done = true;
                        @out.append(Saxon.Hej.value.BooleanValue.FALSE);
                        lookupTable = null;
                        if (watchManager.allowsEarlyExit()) {
                            closed = true;
                            @out.close();
                            getTerminator().terminate();
                        }
                    }
                } else {
                    System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> typed = lookupTable.get(matchType.getFingerprint());
                    if ((typed == null)) {
                        typed = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(10);
                        lookupTable.put(matchType.getFingerprint(), typed);
                        foreach (Saxon.Hej.value.StringValue val in untypedValues) {
                            Saxon.Hej.value.AtomicValue cval = Saxon.Hej.type.Converter.convert(val, matchType, rules);
                            typed.Add(cval.getXPathMatchKey(collator, implicitTimezone));
                        }
                    }
                }
                System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> lookup = lookupTable.get(matchType.getFingerprint());
                if (next.isUntypedAtomic()) {
                    Saxon.Hej.z.IntIterator types = lookupTable.keyIterator();
                    while ((types.hasNext() && !(done))) {
                        int t = types.next();
                        Saxon.Hej.value.AtomicValue converted = Saxon.Hej.type.Converter.convert(next, (Saxon.Hej.type.AtomicType)(Saxon.Hej.type.BuiltInType.getSchemaType(t)), rules);
                        lookup = lookupTable.get(t);
                        if ((lookup != null)) {
                            testValue(lookup, converted.getXPathMatchKey(collator, implicitTimezone));
                        }
                    }
                } else {
                    Saxon.Hej.z.IntIterator types = lookupTable.keyIterator();
                    while ((types.hasNext() && !(done))) {
                        int t = types.next();
                        if ((t != Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                            Saxon.Hej.type.BuiltInAtomicType u = (Saxon.Hej.type.BuiltInAtomicType)(Saxon.Hej.type.BuiltInType.getSchemaType(t));
                            if (((matchType != u) && !(Saxon.Hej.type.Type.isGuaranteedComparable(matchType, u, false)))) {
                                throw new Saxon.Hej.trans.XPathException((((("Comparison between "+matchType.getDisplayName())+" and ")+u.getDisplayName())+" is not allowed"), "XPTY0004", getExpression().getLocation());
                            }
                        }
                    }
                    testValue(lookup, k1);
                }
            }
        }
        private void testValue(System.Collections.Generic.ISet<Saxon.Hej.expr.sort.AtomicMatchKey> lookup, Saxon.Hej.expr.sort.AtomicMatchKey k) {
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            if (lookup.Contains(k)) {
                done = true;
                @out.append(Saxon.Hej.value.BooleanValue.TRUE);
                lookupTable = null;
                if (watchManager.allowsEarlyExit()) {
                    closed = true;
                    @out.close();
                    getTerminator().terminate();
                }
            }
        }
        public override void close() {
            if (!(done)) {
                getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
            }
            if (!(closed)) {
                base.close();
                lookupTable = null;
            }
        }
    
    }
}
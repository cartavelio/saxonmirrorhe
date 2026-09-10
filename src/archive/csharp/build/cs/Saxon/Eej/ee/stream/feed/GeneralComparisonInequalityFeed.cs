using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class GeneralComparisonInequalityFeed : Saxon.Eej.ee.stream.feed.AtomicItemFeed {
        private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private readonly Saxon.Hej.lib.StringCollator collator;
        private Saxon.Hej.expr.sort.AtomicMatchKey firstStreamed = null;
        private Saxon.Hej.value.StringValue firstUntypedStreamed = null;
        private bool done = false;
        private bool closed = false;
        private readonly Saxon.Hej.expr.Expression pullOperand;
        private readonly int implicitTimezone;
        public GeneralComparisonInequalityFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.Expression expr, int pushedArg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            this.watchManager = watchManager;
            Saxon.Hej.expr.GeneralComparison gc = (Saxon.Hej.expr.GeneralComparison)(getExpression());
            collator = gc.getStringCollator();
            pullOperand = ((pushedArg == 0) ? gc.getRhsExpression() : gc.getLhsExpression());
            implicitTimezone = context.getImplicitTimezone();
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            done = false;
            firstStreamed = null;
            firstUntypedStreamed = null;
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(done)) {
                Saxon.Hej.expr.XPathContext context = getContext();
                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                Saxon.Hej.value.AtomicValue next = (Saxon.Hej.value.AtomicValue)(item);
                Saxon.Hej.expr.sort.AtomicMatchKey ks1 = next.getXPathMatchKey(collator, implicitTimezone);
                Saxon.Hej.type.BuiltInAtomicType matchType;
                if ((next is Saxon.Hej.value.NumericValue)) {
                    matchType = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
                } else {
                    matchType = ((Saxon.Hej.value.AtomicValue)(item)).getPrimitiveType();
                }
                if ((firstStreamed == null)) {
                    firstStreamed = ks1;
                    if (next.isUntypedAtomic()) {
                        firstUntypedStreamed = (Saxon.Hej.value.StringValue)(next);
                    }
                    Saxon.Hej.om.SequenceIterator comparand = pullOperand.iterate(context);
                    Saxon.Hej.value.AtomicValue val;
                    bool existsUnstreamed = false;
                    while (((val = (Saxon.Hej.value.AtomicValue)(comparand.next())) != null)) {
                        existsUnstreamed = true;
                        if (!(Saxon.Hej.type.Type.isGenerallyComparable(matchType, val.getPrimitiveType(), false))) {
                            throw new Saxon.Hej.trans.XPathException((((("Comparison between "+matchType)+" and ")+val.getPrimitiveType())+" is not allowed"), "XPTY0004", getExpression().getLocation());
                        }
                        if (val.isUntypedAtomic()) {
                            Saxon.Hej.value.AtomicValue cval = Saxon.Hej.type.Converter.convert(firstUntypedStreamed, matchType, rules);
                            Saxon.Hej.expr.sort.AtomicMatchKey ku1 = cval.getXPathMatchKey(collator, implicitTimezone);
                            if (!(ku1.Equals(ks1))) {
                                reportDone(true);
                                break ;
                            }
                        } else {
                            Saxon.Hej.expr.sort.AtomicMatchKey ku2 = val.getXPathMatchKey(collator, implicitTimezone);
                            if (!(ks1.Equals(ku2))) {
                                reportDone(true);
                                break ;
                            }
                            if ((firstUntypedStreamed != null)) {
                                Saxon.Hej.value.AtomicValue cval = Saxon.Hej.type.Converter.convert(firstUntypedStreamed, matchType, rules);
                                Saxon.Hej.expr.sort.AtomicMatchKey ks2 = cval.getXPathMatchKey(collator, implicitTimezone);
                                if ((ku2 != ks2)) {
                                    reportDone(true);
                                    break ;
                                }
                            }
                        }
                    }
                    if (!(existsUnstreamed)) {
                        reportDone(false);
                    }
                } else {
                    if ((ks1 != firstStreamed)) {
                        reportDone(true);
                    }
                }
            }
        }
        private void reportDone(bool result) {
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            done = true;
            @out.append(Saxon.Hej.value.BooleanValue.get(result));
            if (watchManager.allowsEarlyExit()) {
                closed = true;
                @out.close();
                getTerminator().terminate();
            }
        }
        public override void close() {
            if (!(done)) {
                getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
            }
            if (!(closed)) {
                base.close();
                firstStreamed = null;
                firstUntypedStreamed = null;
            }
        }
    
    }
}
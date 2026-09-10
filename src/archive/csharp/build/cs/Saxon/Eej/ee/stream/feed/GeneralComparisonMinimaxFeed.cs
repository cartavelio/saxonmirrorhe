using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class GeneralComparisonMinimaxFeed : Saxon.Eej.ee.stream.feed.AtomicItemFeed {
        private Saxon.Hej.expr.sort.AtomicComparer comparer;
        private readonly Saxon.Hej.lib.StringCollator collator;
        internal Saxon.Hej.type.BuiltInAtomicType matchType;
        private System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> unstreamedValues;
        private Saxon.Hej.value.NumericValue minUnstreamedNumeric;
        private Saxon.Hej.value.NumericValue maxUnstreamedNumeric;
        private int @operator;
        private bool done = false;
        private readonly Saxon.Hej.expr.Expression pullOperand;
        private readonly int implicitTimezone;
        private System.Predicate<int> predicate;
        public GeneralComparisonMinimaxFeed(Saxon.Hej.expr.Expression expr, int pushedArg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            Saxon.Hej.expr.GeneralComparison gc = (Saxon.Hej.expr.GeneralComparison)(getExpression());
            comparer = gc.getAtomicComparer();
            collator = gc.getAtomicComparer().getCollator();
            @operator = gc.getSingletonOperator();
            if ((pushedArg == 1)) {
                @operator = Saxon.Hej.expr.parser.Token.inverse(@operator);
            }
            pullOperand = ((pushedArg == 0) ? gc.getRhsExpression() : gc.getLhsExpression());
            implicitTimezone = context.getImplicitTimezone();
            predicate = makeTest(@operator);
        }
        private static System.Predicate<int> makeTest(int @operator) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.FLT:
                return i => (i<0);
              case Saxon.Hej.expr.parser.Token.FLE:
                return i => (i<=0);
              case Saxon.Hej.expr.parser.Token.FGT:
                return i => (i>0);
              case Saxon.Hej.expr.parser.Token.FGE:
                return i => (i>0);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            base.open(terminator);
            done = false;
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(done)) {
                Saxon.Hej.expr.XPathContext context = getContext();
                if ((unstreamedValues == null)) {
                    unstreamedValues = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                    Saxon.Hej.om.SequenceIterator unstreamed = pullOperand.iterate(context);
                    Saxon.Hej.om.Item it;
                    while (((it = unstreamed.next()) != null)) {
                        Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(it);
                        unstreamedValues.Add(val);
                        if ((val is Saxon.Hej.value.UntypedAtomicValue)) {
                            string str = Saxon.Hej.value.Whitespace.trim(val.getStringValue());
                            if (System.Text.RegularExpressions.Regex.IsMatch(str, "^[-+0-9\\.]*$")) {
                                Saxon.Hej.value.NumericValue nv = Saxon.Hej.value.NumericValue.parseNumber(str);
                                if (!(nv.isNaN())) {
                                    val = nv;
                                }
                            }
                        }
                        if ((val is Saxon.Hej.value.NumericValue)) {
                            if ((minUnstreamedNumeric == null)) {
                                minUnstreamedNumeric = (Saxon.Hej.value.NumericValue)(val);
                                maxUnstreamedNumeric = (Saxon.Hej.value.NumericValue)(val);
                            } else {
                                Saxon.Hej.expr.sort.NumericComparer comp = Saxon.Hej.expr.sort.NumericComparer11.getInstance();
                                if ((comp.compareAtomicValues(val, minUnstreamedNumeric)<0)) {
                                    minUnstreamedNumeric = (Saxon.Hej.value.NumericValue)(val);
                                }
                                if ((comp.compareAtomicValues(val, maxUnstreamedNumeric)>0)) {
                                    maxUnstreamedNumeric = (Saxon.Hej.value.NumericValue)(val);
                                }
                            }
                        }
                    }
                    if (unstreamedValues.Count==0) {
                        reportDone(false);
                        return ;
                    }
                }
                Saxon.Hej.value.AtomicValue streamedVal = (Saxon.Hej.value.AtomicValue)(item);
                if (((streamedVal is Saxon.Hej.value.UntypedAtomicValue) && (minUnstreamedNumeric != null))) {
                    streamedVal = new Saxon.Hej.value.DoubleValue(Saxon.Hej.value.StringToDouble11.getInstance().stringToNumber(streamedVal.getUnicodeStringValue()));
                }
                if ((streamedVal is Saxon.Hej.value.NumericValue)) {
                    Saxon.Hej.expr.sort.NumericComparer comparer = Saxon.Hej.expr.sort.NumericComparer11.getInstance();
                    switch (@operator) {
                      case Saxon.Hej.expr.parser.Token.FLE:
                        if ((comparer.compareAtomicValues(streamedVal, maxUnstreamedNumeric)<=0)) {
                            reportDone(true);
                            return ;
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.FLT:
                        if ((comparer.compareAtomicValues(streamedVal, maxUnstreamedNumeric)<0)) {
                            reportDone(true);
                            return ;
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.FGE:
                        if ((comparer.compareAtomicValues(streamedVal, minUnstreamedNumeric)>=0)) {
                            reportDone(true);
                            return ;
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.FGT:
                        if ((comparer.compareAtomicValues(streamedVal, minUnstreamedNumeric)>0)) {
                            reportDone(true);
                            return ;
                        }
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                } else {
                    foreach (Saxon.Hej.value.AtomicValue unstreamedVal in unstreamedValues) {
                        int comp = comparer.compareAtomicValues(streamedVal, unstreamedVal);
                        if (predicate(comp)) {
                            reportDone(true);
                            return ;
                        }
                    }
                }
            }
        }
        private void reportDone(bool result) {
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            done = true;
            @out.append(Saxon.Hej.value.BooleanValue.get(result));
            @out.close();
            getTerminator().terminate();
        }
        public override void close() {
            if (!(done)) {
                getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
                base.close();
                minUnstreamedNumeric = null;
                maxUnstreamedNumeric = null;
                done = false;
            }
        }
    
    }
}
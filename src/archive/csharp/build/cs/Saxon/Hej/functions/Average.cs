using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Average : Saxon.Hej.functions.FoldingFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            if (!(Saxon.Hej.value.Cardinality.allowsZero(arguments[0].getCardinality()))) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return base.getCardinality(arguments);
            }
        }
        public override Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  additionalArguments) {
            return new Saxon.Hej.functions.Average.AverageFold(context);
        }
        private class AverageFold : Saxon.Hej.functions.Fold {
            private readonly Saxon.Hej.expr.XPathContext context;
            private Saxon.Hej.value.AtomicValue data;
            private bool atStart = true;
            private readonly Saxon.Hej.lib.ConversionRules rules;
            private readonly Saxon.Hej.type.StringConverter toDouble;
            private int count = 0;
            public AverageFold(Saxon.Hej.expr.XPathContext context)  {
                this.context = context;
                this.rules = context.getConfiguration().getConversionRules();
                this.toDouble = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(rules);
            }
            public void processItem(Saxon.Hej.om.Item item) {
                Saxon.Hej.value.AtomicValue next = (Saxon.Hej.value.AtomicValue)(item);
                if (next.isUntypedAtomic()) {
                    next = toDouble.convert(next).asAtomic();
                }
                count++;
                if (atStart) {
                    if ((((next is Saxon.Hej.value.NumericValue) || (next is Saxon.Hej.value.DayTimeDurationValue)) || (next is Saxon.Hej.value.YearMonthDurationValue))) {
                        data = next;
                        atStart = false;
                    } else {
                        if ((next is Saxon.Hej.value.DurationValue)) {
                            throw new Saxon.Hej.trans.XPathException((("Input to avg() contains a duration ("+Saxon.Hej.trans.Err.depict(next))+") that is neither an xs:dayTimeDuration nor an xs:yearMonthDuration"), "FORG0006");
                        } else {
                            throw new Saxon.Hej.trans.XPathException((("Input to avg() contains a value ("+Saxon.Hej.trans.Err.depict(next))+") that is neither numeric, nor a duration"), "FORG0006");
                        }
                    }
                } else {
                    if ((data is Saxon.Hej.value.NumericValue)) {
                        if (!(((next is Saxon.Hej.value.NumericValue)))) {
                            throw new Saxon.Hej.trans.XPathException("Input to avg() contains a mix of numeric and non-numeric values", "FORG0006");
                        }
                        data = Saxon.Hej.expr.ArithmeticExpression.compute(data, Saxon.Hej.expr.Calculator.PLUS, next, context);
                    } else {
                        if ((data is Saxon.Hej.value.DurationValue)) {
                            if (!(((next is Saxon.Hej.value.DurationValue)))) {
                                throw new Saxon.Hej.trans.XPathException("Input to avg() contains a mix of duration and non-duration values", "FORG0006");
                            }
                            try {
                                data = ((Saxon.Hej.value.DurationValue)(data)).add((Saxon.Hej.value.DurationValue)(next));
                            } catch (Saxon.Hej.trans.XPathException e) {
                                if ("XPTY0004".Equals(e.getErrorCodeLocalPart())) {
                                    e.setErrorCode("FORG0006");
                                }
                                throw;
                            }
                        } else {
                            throw new Saxon.Hej.trans.XPathException((("Input to avg() contains a value ("+Saxon.Hej.trans.Err.depict(data))+") that is neither numeric, nor a duration"), "FORG0006");
                        }
                    }
                }
            }
            public bool isFinished() {
                return ((data is Saxon.Hej.value.DoubleValue) && data.isNaN());
            }
            public Saxon.Hej.om.Sequence result() {
                if (atStart) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    if ((data is Saxon.Hej.value.NumericValue)) {
                        return Saxon.Hej.expr.ArithmeticExpression.compute(data, Saxon.Hej.expr.Calculator.DIV, new Saxon.Hej.value.Int64Value(count), context);
                    } else {
                        return ((Saxon.Hej.value.DurationValue)(data)).divide(count);
                    }
                }
            }
        
        }
    
    }
}
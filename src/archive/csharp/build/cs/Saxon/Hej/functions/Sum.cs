using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Sum : Saxon.Hej.functions.FoldingFunction {
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.TypeHierarchy th = getRetainedStaticContext().getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType @base = Saxon.Hej.expr.Atomizer.getAtomizedItemType(args[0], false, th);
            if (@base.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                @base = Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
            if (Saxon.Hej.value.Cardinality.allowsZero(args[0].getCardinality())) {
                if ((getArity() == 1)) {
                    return Saxon.Hej.type.Type.getCommonSuperType(@base, Saxon.Hej.type.BuiltInAtomicType.INTEGER, th);
                } else {
                    return Saxon.Hej.type.Type.getCommonSuperType(@base, args[1].getItemType(), th);
                }
            } else {
                return @base.getPrimitiveItemType();
            }
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            if (((getArity() == 1) || (arguments[1].getCardinality() == 1))) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
        }
        public override Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  additionalArguments) {
            if ((additionalArguments.Length>0)) {
                Saxon.Hej.value.AtomicValue z = (Saxon.Hej.value.AtomicValue)(additionalArguments[0].head());
                return new Saxon.Hej.functions.Sum.SumFold(context, z);
            } else {
                return new Saxon.Hej.functions.Sum.SumFold(context, Saxon.Hej.value.Int64Value.ZERO);
            }
        }
        public static Saxon.Hej.value.AtomicValue total(Saxon.Hej.om.SequenceIterator @in, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locator) {
            try {
                Saxon.Hej.functions.Sum.SumFold fold = new Saxon.Hej.functions.Sum.SumFold(context, null);
                Saxon.Hej.om.SequenceTool.supply(@in, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => fold.processItem(it) /* converted from instance method reference */));
                return (Saxon.Hej.value.AtomicValue)(fold.result().head());
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(locator);
                throw;
            }
        }
        private class SumFold : Saxon.Hej.functions.Fold {
            private readonly Saxon.Hej.expr.XPathContext context;
            private readonly Saxon.Hej.value.AtomicValue zeroValue;
            private Saxon.Hej.value.AtomicValue data;
            private bool atStart = true;
            private readonly Saxon.Hej.lib.ConversionRules rules;
            private readonly Saxon.Hej.type.StringConverter toDouble;
            public SumFold(Saxon.Hej.expr.XPathContext context, Saxon.Hej.value.AtomicValue zeroValue)  {
                this.context = context;
                this.zeroValue = zeroValue;
                this.rules = context.getConfiguration().getConversionRules();
                this.toDouble = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(rules);
            }
            public void processItem(Saxon.Hej.om.Item item) {
                Saxon.Hej.value.AtomicValue next = (Saxon.Hej.value.AtomicValue)(item);
                if (atStart) {
                    atStart = false;
                    if (next.isUntypedAtomic()) {
                        data = toDouble.convert(next).asAtomic();
                        return ;
                    } else {
                        if ((((next is Saxon.Hej.value.NumericValue) || (next is Saxon.Hej.value.DayTimeDurationValue)) || (next is Saxon.Hej.value.YearMonthDurationValue))) {
                            data = next;
                            return ;
                        } else {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Input to sum() contains a value of type "+next.getPrimitiveType().getDisplayName())+" which is neither numeric, nor a duration"));
                            err.setXPathContext(context);
                            err.setErrorCode("FORG0006");
                            throw err;
                        }
                    }
                }
                if ((data is Saxon.Hej.value.NumericValue)) {
                    if (next.isUntypedAtomic()) {
                        next = toDouble.convert(next).asAtomic();
                    } else {
                        if (!(((next is Saxon.Hej.value.NumericValue)))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Input to sum() contains a mix of numeric and non-numeric values");
                            err.setXPathContext(context);
                            err.setErrorCode("FORG0006");
                            throw err;
                        }
                    }
                    data = Saxon.Hej.expr.ArithmeticExpression.compute(data, Saxon.Hej.expr.Calculator.PLUS, next, context);
                } else {
                    if ((data is Saxon.Hej.value.DurationValue)) {
                        if (!(((((data is Saxon.Hej.value.DayTimeDurationValue)) || ((data is Saxon.Hej.value.YearMonthDurationValue)))))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Input to sum() contains a duration that is neither a dayTimeDuration nor a yearMonthDuration");
                            err.setXPathContext(context);
                            err.setErrorCode("FORG0006");
                            throw err;
                        }
                        if (!(((next is Saxon.Hej.value.DurationValue)))) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Input to sum() contains a mix of duration and non-duration values");
                            err.setXPathContext(context);
                            err.setErrorCode("FORG0006");
                            throw err;
                        }
                        data = ((Saxon.Hej.value.DurationValue)(data)).add((Saxon.Hej.value.DurationValue)(next));
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Input to sum() contains a value of type "+data.getPrimitiveType().getDisplayName())+" which is neither numeric, nor a duration"));
                        err.setXPathContext(context);
                        err.setErrorCode("FORG0006");
                        throw err;
                    }
                }
            }
            public bool isFinished() {
                return ((data is Saxon.Hej.value.DoubleValue) && data.isNaN());
            }
            public Saxon.Hej.om.Sequence result() {
                if (atStart) {
                    return Saxon.Hej.om.SequenceTool.itemOrEmpty(zeroValue);
                } else {
                    return data;
                }
            }
        
        }
        public override string getCompilerName() {
            return "SumCompiler";
        }
    
    }
}
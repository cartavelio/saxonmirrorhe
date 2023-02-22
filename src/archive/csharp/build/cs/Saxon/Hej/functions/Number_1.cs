using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Number_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return toNumber((Saxon.Hej.value.AtomicValue)(arg));
        }
        public override Saxon.Hej.value.DoubleValue resultWhenEmpty() {
            return Saxon.Hej.value.DoubleValue.NaN;
        }
        public override Saxon.Hej.expr.Expression typeCheckCaller(Saxon.Hej.expr.FunctionCall caller, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if (caller.getArg(0).isCallOn(typeof(Saxon.Hej.functions.Number_1))) {
                caller.setArg(0, ((Saxon.Hej.expr.FunctionCall)(caller.getArg(0))).getArg(0));
            }
            return caller;
        }
        public static Saxon.Hej.value.DoubleValue toNumber(Saxon.Hej.value.AtomicValue arg0) {
            if ((arg0 is Saxon.Hej.value.BooleanValue)) {
                return Saxon.Hej.type.Converter.BooleanToDouble.INSTANCE.convert(arg0);
            } else {
                if ((arg0 is Saxon.Hej.value.NumericValue)) {
                    return (Saxon.Hej.value.DoubleValue)(Saxon.Hej.type.Converter.NumericToDouble.INSTANCE.convert(arg0).asAtomic());
                } else {
                    if (((arg0 is Saxon.Hej.value.StringValue) && !(((arg0 is Saxon.Hej.value.AnyURIValue))))) {
                        Saxon.Hej.type.ConversionResult cr = Saxon.Hej.value.StringToDouble11.getInstance().convert(arg0);
                        if ((cr is Saxon.Hej.type.ValidationFailure)) {
                            return Saxon.Hej.value.DoubleValue.NaN;
                        } else {
                            return (Saxon.Hej.value.DoubleValue)(cr);
                        }
                    } else {
                        return Saxon.Hej.value.DoubleValue.NaN;
                    }
                }
            }
        }
        public static Saxon.Hej.value.DoubleValue convert(Saxon.Hej.value.AtomicValue value, Saxon.Hej.Configuration config) {
            try {
                if ((value == null)) {
                    return Saxon.Hej.value.DoubleValue.NaN;
                }
                if ((value is Saxon.Hej.value.BooleanValue)) {
                    return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.BooleanValue)(value)).getBooleanValue() ? 1.0e0 : 0.0e0));
                }
                if ((value is Saxon.Hej.value.DoubleValue)) {
                    return (Saxon.Hej.value.DoubleValue)(value);
                }
                if ((value is Saxon.Hej.value.NumericValue)) {
                    return new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(value)).getDoubleValue());
                }
                if (((value is Saxon.Hej.value.StringValue) && !(((value is Saxon.Hej.value.AnyURIValue))))) {
                    double d = config.getConversionRules().getStringToDoubleConverter().stringToNumber(value.getUnicodeStringValue());
                    return new Saxon.Hej.value.DoubleValue(d);
                }
                return Saxon.Hej.value.DoubleValue.NaN;
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                return Saxon.Hej.value.DoubleValue.NaN;
            }
        }
        public override string getCompilerName() {
            return "NumberFnCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Number_1.NumberFnElaborator();
        }
        public class NumberFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = fnc.getArg(0).makeElaborator().elaborateForItem();
                return context => toNumber((Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/argEval(context)));
            }
        
        }
    
    }
}
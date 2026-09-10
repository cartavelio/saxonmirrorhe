using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Substring : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.expr.Expression typeCheckCaller(Saxon.Hej.expr.FunctionCall caller, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.Expression e2 = base.typeCheckCaller(caller, visitor, contextInfo);
            if ((e2 != caller)) {
                return e2;
            }
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if (caller.getArg(1).isCallOn(typeof(Saxon.Hej.functions.Number_1))) {
                Saxon.Hej.expr.Expression a1 = ((Saxon.Hej.expr.StaticFunctionCall)(caller.getArg(1))).getArg(0);
                if (th.isSubType(a1.getItemType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                    caller.setArg(1, a1);
                }
            }
            if (((getArity()>2) && caller.getArg(2).isCallOn(typeof(Saxon.Hej.functions.Number_1)))) {
                Saxon.Hej.expr.Expression a2 = ((Saxon.Hej.expr.StaticFunctionCall)(caller.getArg(2))).getArg(0);
                if (th.isSubType(a2.getItemType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                    caller.setArg(2, a2);
                }
            }
            return caller;
        }
        public static Saxon.Hej.value.StringValue substring(Saxon.Hej.value.StringValue sv, Saxon.Hej.value.NumericValue start) {
            long slength = sv.length();
            long lstart;
            if ((start is Saxon.Hej.value.Int64Value)) {
                lstart = ((Saxon.Hej.value.Int64Value)(start)).longValue();
                if ((lstart>slength)) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                } else {
                    if ((lstart<=0)) {
                        lstart = 1;
                    }
                }
            } else {
                if (start.isNaN()) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                } else {
                    if ((start.signum()<=0)) {
                        return sv;
                    } else {
                        if ((start.compareTo(slength)>0)) {
                            return Saxon.Hej.value.StringValue.EMPTY_STRING;
                        } else {
                            lstart = (long)System.Math.Round(start.getDoubleValue(), System.MidpointRounding.AwayFromZero);
                        }
                    }
                }
            }
            if ((lstart>slength)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            return new Saxon.Hej.value.StringValue(sv.getContent().substring(((int)(lstart)-1), slength));
        }
        public static Saxon.Hej.value.StringValue substring(Saxon.Hej.value.StringValue sv, Saxon.Hej.value.NumericValue start, Saxon.Hej.value.NumericValue len) {
            long slength = sv.length();
            long lstart;
            if ((start is Saxon.Hej.value.Int64Value)) {
                lstart = ((Saxon.Hej.value.Int64Value)(start)).longValue();
                if ((lstart>slength)) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                }
            } else {
                if (start.isNaN()) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                } else {
                    if ((start.compareTo(slength)>0)) {
                        return Saxon.Hej.value.StringValue.EMPTY_STRING;
                    } else {
                        double dstart = start.getDoubleValue();
                        lstart = (System.Double.IsInfinity(dstart) ? -(System.Int32.MaxValue) : (long)System.Math.Round(dstart, System.MidpointRounding.AwayFromZero));
                    }
                }
            }
            long llen;
            if ((len is Saxon.Hej.value.Int64Value)) {
                llen = ((Saxon.Hej.value.Int64Value)(len)).longValue();
                if ((llen<=0)) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                }
            } else {
                if (len.isNaN()) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                }
                if ((len.signum()<=0)) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                }
                double dlen = len.getDoubleValue();
                if (System.Double.IsInfinity(dlen)) {
                    llen = System.Int32.MaxValue;
                } else {
                    llen = (long)System.Math.Round(len.getDoubleValue(), System.MidpointRounding.AwayFromZero);
                }
            }
            long lend = (lstart+llen);
            if ((lend<lstart)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            int a1 = ((int)(lstart)-1);
            if ((a1>=slength)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            long a2 = System.Math.Min(slength, ((int)(lend)-1));
            if ((a1<0)) {
                if ((a2<0)) {
                    return Saxon.Hej.value.StringValue.EMPTY_STRING;
                } else {
                    a1 = 0;
                }
            }
            return new Saxon.Hej.value.StringValue(sv.getContent().substring(a1, a2));
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue arg0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((arg0 == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            Saxon.Hej.value.NumericValue arg1 = (Saxon.Hej.value.NumericValue)(arguments[1].head());
            if ((arguments.Length == 2)) {
                return substring(arg0, arg1);
            } else {
                Saxon.Hej.value.NumericValue arg2 = (Saxon.Hej.value.NumericValue)(arguments[2].head());
                return substring(arg0, arg1, arg2);
            }
        }
        public override string getCompilerName() {
            return "SubstringCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Substring.SubstringFnElaborator();
        }
        public class SubstringFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(fnc.getArg(0).getCardinality());
                if ((fnc.getArity() == 2)) {
                    return context => {
                        Saxon.Hej.value.StringValue sv = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg0Eval(context));
                        if ((nullable && (sv == null))) {
                            return Saxon.Hej.value.StringValue.EMPTY_STRING;
                        }
                        Saxon.Hej.value.NumericValue start = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg1Eval(context));
                        return substring(sv, start);
                    } ;
                } else {
                    Saxon.Hej.expr.elab.ItemEvaluator arg2Eval = fnc.getArg(2).makeElaborator().elaborateForItem();
                    return context => {
                        Saxon.Hej.value.StringValue sv = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg0Eval(context));
                        if ((nullable && (sv == null))) {
                            return Saxon.Hej.value.StringValue.EMPTY_STRING;
                        }
                        Saxon.Hej.value.NumericValue start = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg1Eval(context));
                        Saxon.Hej.value.NumericValue len = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg2Eval(context));
                        return substring(sv, start, len);
                    } ;
                }
            }
        
        }
    
    }
}
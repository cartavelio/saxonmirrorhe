using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class RoundHalfToEven : Saxon.Hej.functions.SystemFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.NumericValue val0 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            if ((val0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            int scale = 0;
            if ((arguments.Length == 2)) {
                Saxon.Hej.value.NumericValue scaleVal = (Saxon.Hej.value.NumericValue)(arguments[1].head());
                if ((scaleVal.compareTo(System.Int32.MaxValue)>0)) {
                    return val0;
                } else {
                    if ((scaleVal.compareTo(System.Int32.MinValue)<0)) {
                        scale = System.Int32.MinValue;
                    } else {
                        scale = (int)(scaleVal.longValue());
                    }
                }
            }
            return val0.roundHalfToEven(scale);
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.RoundHalfToEven.RoundHalfToEvenElaborator();
        }
        public class RoundHalfToEvenElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator arg0eval = fnc.getArg(0).makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(fnc.getArg(0).getCardinality());
                if ((fnc.getArity() == 1)) {
                    if (nullable) {
                        return context => {
                            Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg0eval(context));
                            if ((result == null)) {
                                return null;
                            }
                            return result.roundHalfToEven(0);
                        } ;
                    } else {
                        return context => ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg0eval(context))).roundHalfToEven(0);
                    }
                } else {
                    if (((fnc.getArg(1) is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(fnc.getArg(1))).getGroundedValue() is Saxon.Hej.value.NumericValue))) {
                        Saxon.Hej.value.NumericValue scaleVal = (Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(fnc.getArg(1))).getGroundedValue());
                        if ((scaleVal.compareTo(System.Int32.MaxValue)>0)) {
                            return arg0eval;
                        } else {
                            try {
                                int scale = ((scaleVal.compareTo(System.Int32.MinValue)<0) ? System.Int32.MinValue : (int)(scaleVal.longValue()));
                                return context => {
                                    Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg0eval(context));
                                    if ((result == null)) {
                                        return null;
                                    }
                                    return result.roundHalfToEven(scale);
                                } ;
                            } catch (Saxon.Hej.trans.XPathException e) {
                                return context => {
                                    throw e;
                                } ;
                            }
                        }
                    } else {
                        Saxon.Hej.expr.elab.ItemEvaluator scaleArg = fnc.getArg(1).makeElaborator().elaborateForItem();
                        return context => {
                            Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/arg0eval(context));
                            if ((result == null)) {
                                return null;
                            }
                            Saxon.Hej.value.NumericValue scaleVal = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/scaleArg(context));
                            int scale;
                            if ((scaleVal.compareTo(System.Int32.MaxValue)>0)) {
                                return result;
                            } else {
                                if ((scaleVal.compareTo(System.Int32.MinValue)<0)) {
                                    scale = System.Int32.MinValue;
                                } else {
                                    scale = (int)(scaleVal.longValue());
                                }
                            }
                            return result.roundHalfToEven(scale);
                        } ;
                    }
                }
            }
        
        }
    
    }
}
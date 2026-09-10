using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class Round : Saxon.Hej.functions.SystemFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.NumericValue val0 = (Saxon.Hej.value.NumericValue)(arguments[0].head());
            if ((val0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            int scaleRnd = 0;
            if ((arguments.Length == 2)) {
                Saxon.Hej.value.NumericValue scaleVal = (Saxon.Hej.value.NumericValue)(arguments[1].head());
                scaleRnd = (int)(scaleVal.longValue());
            }
            return val0.round(scaleRnd);
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Round.RoundElaborator();
        }
        public class RoundElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = fnc.getArg(0).makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(fnc.getArg(0).getCardinality());
                if ((fnc.getArity() == 1)) {
                    if (nullable) {
                        return context => {
                            Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context));
                            if ((result == null)) {
                                return null;
                            }
                            return result.round(0);
                        } ;
                    } else {
                        return context => ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context))).round(0);
                    }
                } else {
                    Saxon.Hej.expr.elab.ItemEvaluator scaleArg = fnc.getArg(1).makeElaborator().elaborateForItem();
                    return context => {
                        Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context));
                        if ((result == null)) {
                            return null;
                        }
                        int scale = (int)(((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/scaleArg(context))).longValue());
                        return result.round(scale);
                    } ;
                }
            }
        
        }
    
    }
}
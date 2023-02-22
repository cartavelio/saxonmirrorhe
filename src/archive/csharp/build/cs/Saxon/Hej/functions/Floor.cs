using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public sealed class Floor : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return ((Saxon.Hej.value.NumericValue)(arg)).floor();
        }
        public override string getCompilerName() {
            return "RoundingCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Floor.FloorElaborator();
        }
        public class FloorElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = fnc.getArg(0).makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(fnc.getArg(0).getCardinality());
                if (nullable) {
                    return context => {
                        Saxon.Hej.value.NumericValue result = (Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context));
                        if ((result == null)) {
                            return null;
                        }
                        return result.floor();
                    } ;
                } else {
                    return context => ((Saxon.Hej.value.NumericValue)(/*Delegate.eval()*/argEval(context))).floor();
                }
            }
        
        }
    
    }
}
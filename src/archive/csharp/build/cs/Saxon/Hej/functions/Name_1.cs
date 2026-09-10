using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Name_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.StringValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.StringValue.makeStringValue(((Saxon.Hej.om.NodeInfo)(item)).getDisplayName());
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override string getCompilerName() {
            return "NameCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Name_1.NameElaborator();
        }
        public class NameElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthIfAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                if (nullable) {
                    return context => {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                        if ((node == null)) {
                            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                        }
                        return Saxon.Hej.str.StringView.of(node.getDisplayName());
                    } ;
                } else {
                    return context => Saxon.Hej.str.StringView.of(((Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context))).getDisplayName());
                }
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthIfAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                if (nullable) {
                    return context => {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                        if ((node == null)) {
                            return "";
                        }
                        return node.getDisplayName();
                    } ;
                } else {
                    return context => ((Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context))).getDisplayName();
                }
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class LocalName_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.StringValue.makeStringValue(((Saxon.Hej.om.NodeInfo)(item)).getLocalPart());
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override string getCompilerName() {
            return "LocalNameCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.LocalName_1.LocalNameElaborator();
        }
        public class LocalNameElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthIfAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                    if ((nullable && (node == null))) {
                        return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                    }
                    return Saxon.Hej.str.StringView.of(node.getLocalPart());
                } ;
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthIfAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                    if ((nullable && (node == null))) {
                        return "";
                    }
                    return node.getLocalPart();
                } ;
            }
        
        }
    
    }
}
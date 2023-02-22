using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NamespaceUriFn_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NamespaceUri uri = ((Saxon.Hej.om.NodeInfo)(item)).getNamespaceUri();
            return new Saxon.Hej.value.AnyURIValue(uri.toUnicodeString());
        }
        public override Saxon.Hej.value.AnyURIValue resultWhenEmpty() {
            return new Saxon.Hej.value.AnyURIValue("");
        }
        public override string getCompilerName() {
            return "NamespaceUriFnCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.NamespaceUriFn_1.NamespaceUriFnElaborator();
        }
        public class NamespaceUriFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                    Saxon.Hej.om.NamespaceUri uri = ((nullable && (node == null)) ? Saxon.Hej.om.NamespaceUri.NULL : node.getNamespaceUri());
                    return new Saxon.Hej.value.AnyURIValue(uri.toUnicodeString());
                } ;
            }
        
        }
    
    }
}
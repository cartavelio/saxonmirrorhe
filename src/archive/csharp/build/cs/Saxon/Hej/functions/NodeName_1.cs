using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NodeName_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return nodeName((Saxon.Hej.om.NodeInfo)(item));
        }
        public static Saxon.Hej.value.QNameValue nodeName(Saxon.Hej.om.NodeInfo node) {
            if (node.getLocalPart().Length==0) {
                return null;
            }
            return new Saxon.Hej.value.QNameValue(node.getPrefix(), node.getNamespaceUri(), node.getLocalPart(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
        }
        public override string getCompilerName() {
            return "NodeNameFnCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.NodeName_1.NodeNameFnElaborator();
        }
        public class NodeNameFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                    if (((node == null) || node.getLocalPart().Length==0)) {
                        return null;
                    }
                    return new Saxon.Hej.value.QNameValue(node.getPrefix(), node.getNamespaceUri(), node.getLocalPart(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
                } ;
            }
        
        }
    
    }
}
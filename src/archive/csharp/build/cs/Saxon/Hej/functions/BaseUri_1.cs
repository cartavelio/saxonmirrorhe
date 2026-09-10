using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class BaseUri_1 : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((node == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string s = node.getBaseURI();
            if ((s == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return new Saxon.Hej.value.AnyURIValue(s);
        }
        public override string getCompilerName() {
            return "BaseURICompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.BaseUri_1.BaseUriFnElaborator();
        }
        public class BaseUriFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(arg.getCardinality());
                Saxon.Hej.expr.elab.ItemEvaluator argEval = arg.makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/argEval(context));
                    if ((nullable && (node == null))) {
                        return null;
                    }
                    string s = node.getBaseURI();
                    if ((s == null)) {
                        return null;
                    }
                    return new Saxon.Hej.value.AnyURIValue(s);
                } ;
            }
        
        }
    
    }
}
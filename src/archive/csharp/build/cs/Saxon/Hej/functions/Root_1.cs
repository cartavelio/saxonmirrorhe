using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Root_1 : Saxon.Hej.functions.SystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int prop = ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            if ((((getArity() == 0)) || (((arguments[0].getSpecialProperties()&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0))) {
                prop |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            return prop;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((node == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            } else {
                return node.getRoot();
            }
        }
        public override string getStreamerName() {
            return "Root";
        }
        public override string getCompilerName() {
            return "RootFunctionCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Root_1.RootFnElaborator();
        }
        public class RootFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForItem();
                return context => {
                    Saxon.Hej.om.NodeInfo focus = (Saxon.Hej.om.NodeInfo)(/*Delegate.eval()*/arg0Eval(context));
                    return ((focus == null) ? null : focus.getRoot());
                } ;
            }
        
        }
    
    }
}
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
    
    }
}
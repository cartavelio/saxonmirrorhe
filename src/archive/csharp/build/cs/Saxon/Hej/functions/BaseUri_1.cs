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
    
    }
}
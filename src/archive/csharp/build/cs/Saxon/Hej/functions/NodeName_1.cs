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
            return new Saxon.Hej.value.QNameValue(node.getPrefix(), node.getURI(), node.getLocalPart(), Saxon.Hej.type.BuiltInAtomicType.QNAME);
        }
        public override string getCompilerName() {
            return "NodeNameFnCompiler";
        }
    
    }
}
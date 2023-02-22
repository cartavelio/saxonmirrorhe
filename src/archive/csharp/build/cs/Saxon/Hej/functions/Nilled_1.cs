using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Nilled_1 : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        private static Saxon.Hej.value.BooleanValue getNilledProperty(Saxon.Hej.om.NodeInfo node) {
            if (((node == null) || (node.getNodeKind() != Saxon.Hej.type.Type.ELEMENT))) {
                return null;
            }
            return Saxon.Hej.value.BooleanValue.get(node.isNilled());
        }
        public static bool isNilled(Saxon.Hej.om.NodeInfo node) {
            Saxon.Hej.value.BooleanValue b = getNilledProperty(node);
            return ((b != null) && b.getBooleanValue());
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if (((node == null) || (node.getNodeKind() != Saxon.Hej.type.Type.ELEMENT))) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            return Saxon.Hej.value.BooleanValue.get(isNilled(node));
        }
    
    }
}
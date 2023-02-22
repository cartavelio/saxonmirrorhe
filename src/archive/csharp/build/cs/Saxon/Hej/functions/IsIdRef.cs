using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class IsIdRef : Saxon.Hej.lib.ExtensionFunctionDefinition {
        private static readonly Saxon.Hej.om.StructuredQName qName = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.SAXON, "is-idref");
        public override Saxon.Hej.om.StructuredQName getFunctionQName() {
            return qName;
        }
        public override int getMinimumNumberOfArguments() {
            return 0;
        }
        public override int getMaximumNumberOfArguments() {
            return 0;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return new Saxon.Hej.value.SequenceType[]{};
        }
        public override Saxon.Hej.value.SequenceType getResultType(Saxon.Hej.value.SequenceType[] suppliedArgumentTypes) {
            return Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN;
        }
        public override Saxon.Hej.lib.ExtensionFunctionCall makeCallExpression() {
            return new Saxon.Hej.functions.IsIdRef.IsIdRefCall();
        }
        private class IsIdRefCall : Saxon.Hej.lib.ExtensionFunctionCall {
            public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Item contextItem = context.getContextItem();
                return Saxon.Hej.value.BooleanValue.get(((contextItem is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(contextItem)).isIdref()));
            }
            public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.Item contextItem = context.getContextItem();
                return ((contextItem is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(contextItem)).isIdref());
            }
        
        }
    
    }
}
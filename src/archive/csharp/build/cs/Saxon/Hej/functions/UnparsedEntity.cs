using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class UnparsedEntity : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public static int URI = 0;
        public static int PUBLIC_ID = 1;
        public abstract int getOp();
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            int operation = getOp();
            string arg0 = arguments[0].head().getStringValue();
            Saxon.Hej.om.NodeInfo doc = null;
            if ((getArity() == 1)) {
                Saxon.Hej.om.Item it = context.getContextItem();
                if ((it is Saxon.Hej.om.NodeInfo)) {
                    doc = ((Saxon.Hej.om.NodeInfo)(it)).getRoot();
                }
                if (((doc == null) || (doc.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                    string code = ((operation == URI) ? "XTDE1370" : "XTDE1380");
                    throw new Saxon.Hej.trans.XPathException((("In function "+getFunctionName().getDisplayName())+", the context item must be a node in a tree whose root is a document node"), code, context);
                }
            } else {
                doc = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
                if ((doc != null)) {
                    doc = doc.getRoot();
                }
                if (((doc == null) || (doc.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                    string code = ((operation == URI) ? "XTDE1370" : "XTDE1380");
                    throw new Saxon.Hej.trans.XPathException((("In function "+getFunctionName().getDisplayName())+", the second argument must be a document node"), code, context);
                }
            }
            string[] ids = doc.getTreeInfo().getUnparsedEntity(arg0);
            string result = ((ids == null) ? "" : ids[operation]);
            if ((result == null)) {
                result = "";
            }
            return ((operation == URI) ? new Saxon.Hej.value.AnyURIValue(result) : new Saxon.Hej.value.StringValue(result));
        }
        public class UnparsedEntityUri : Saxon.Hej.functions.UnparsedEntity {
            public override int getOp() {
                return URI;
            }
        
        }
        public class UnparsedEntityPublicId : Saxon.Hej.functions.UnparsedEntity {
            public override int getOp() {
                return PUBLIC_ID;
            }
        
        }
    
    }
}
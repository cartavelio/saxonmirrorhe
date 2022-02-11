using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DocumentUri_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return getDocumentURI((Saxon.Hej.om.NodeInfo)(item), context);
        }
        public static Saxon.Hej.value.AnyURIValue getDocumentURI(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.XPathContext c) {
            if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                Saxon.Hej.Controller controller = c.getController();
                System.Diagnostics.Debug.Assert((controller != null));
                Saxon.Hej.om.DocumentPool pool = controller.getDocumentPool();
                string docURI = pool.getDocumentURI(node);
                if ((docURI == null)) {
                    return null;
                } else {
                    if ("".Equals(docURI)) {
                        return null;
                    } else {
                        return new Saxon.Hej.value.AnyURIValue(docURI);
                    }
                }
            } else {
                return null;
            }
        }
    
    }
}
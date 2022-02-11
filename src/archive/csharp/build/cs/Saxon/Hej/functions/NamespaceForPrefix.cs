using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NamespaceForPrefix : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AnyURIValue result = namespaceUriForPrefix((Saxon.Hej.value.StringValue)(arguments[0].head()), (Saxon.Hej.om.NodeInfo)(arguments[1].head()));
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(result);
        }
        private static Saxon.Hej.value.AnyURIValue namespaceUriForPrefix(Saxon.Hej.value.StringValue p, Saxon.Hej.om.NodeInfo element) {
            string prefix;
            if ((p == null)) {
                prefix = "";
            } else {
                prefix = p.getStringValue();
            }
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            string uri = resolver.getURIForPrefix(prefix, true);
            if (((uri == null) || uri.Length==0)) {
                return null;
            }
            return new Saxon.Hej.value.AnyURIValue(uri);
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewAttribute : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
            Saxon.Hej.value.AtomicValue nameArg = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            Saxon.Hej.om.StructuredQName name;
            if ((nameArg is Saxon.Hej.value.QNameValue)) {
                name = ((Saxon.Hej.value.QNameValue)(arguments[0].head())).getStructuredQName();
            } else {
                string s = nameArg.getStringValue();
                name = Saxon.Hej.om.StructuredQName.fromLexicalQName(s, false, true, getRetainedStaticContext());
            }
            string prefix = name.getPrefix();
            string local = name.getLocalPart();
            string uri = name.getURI();
            if ((local.Equals("xmlns") && uri.Length==0)) {
                throw new Saxon.Hej.trans.XPathException("Invalid attribute name: xmlns", "XQDY0044");
            }
            if ((uri.Length==0 && !(prefix.Length==0))) {
                prefix = "";
            }
            if (prefix.Equals("xmlns")) {
                prefix = "";
            }
            if ((prefix.Length==0 && !(uri.Length==0))) {
                prefix = context.getNamePool().suggestPrefixForURI(uri);
                if ((prefix == null)) {
                    prefix = "ns0";
                }
            }
            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML) != "xml".Equals(prefix))) {
                throw new Saxon.Hej.trans.XPathException("The xml prefix and namespace can only be used together", "XTDE0835");
            }
            name = new Saxon.Hej.om.StructuredQName(prefix, uri, local);
            orphan.setNodeName(new Saxon.Hej.om.FingerprintedQName(name, context.getNamePool()));
            Saxon.Hej.om.Item first = arguments[1].head();
            orphan.setStringValue(((first == null) ? (Saxon.Hej.str.UnicodeString)(Saxon.Hej.str.EmptyUnicodeString.getInstance()) : first.getUnicodeStringValue()));
            return orphan;
        }
    
    }
}
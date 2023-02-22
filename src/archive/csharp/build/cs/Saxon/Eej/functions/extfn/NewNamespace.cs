using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewNamespace : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.NAMESPACE);
            string name = arguments[0].head().getStringValue();
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(name)))) {
                throw new Saxon.Hej.trans.XPathException(("Name of namespace node must be a valid NCName: "+name), "FORG0001");
            }
            orphan.setNodeName(new Saxon.Hej.om.FingerprintedQName("", "", name));
            orphan.setStringValue(arguments[1].head().getUnicodeStringValue());
            return orphan;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewText : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.TEXT);
            Saxon.Hej.om.Item first = arguments[0].head();
            orphan.setStringValue(((first == null) ? (Saxon.Hej.str.UnicodeString)(Saxon.Hej.str.EmptyUnicodeString.getInstance()) : first.getUnicodeStringValue()));
            return orphan;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class NewProcessingInstruction : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.NodeInfo call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(context.getConfiguration());
            orphan.setNodeKind(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
            string name = arguments[0].head().getStringValue();
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(name)))) {
                throw new Saxon.Hej.trans.XPathException(("Name of processing instruction must be a valid NCName: "+name), "XTDE0890");
            }
            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(name, "xml")) {
                throw new Saxon.Hej.trans.XPathException(("Name of processing instruction must must not match [Xx][Mm][Ll]: "+name), "XTDE0890");
            }
            Saxon.Hej.value.StringValue first = (Saxon.Hej.value.StringValue)(arguments[1].head());
            string value = ((first == null) ? "" : first.getStringValue());
            value = System.Text.RegularExpressions.Regex.Replace(value, "\\?>", "? >");
            orphan.setNodeName(new Saxon.Hej.om.FingerprintedQName("", "", name));
            orphan.setStringValue(Saxon.Hej.str.StringView.tidy(value));
            return orphan;
        }
    
    }
}
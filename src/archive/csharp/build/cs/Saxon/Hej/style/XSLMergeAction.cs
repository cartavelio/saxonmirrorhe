using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMergeAction : Saxon.Hej.style.StyleElement {
        public override bool isInstruction() {
            return false;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            return content;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLMerge)))) {
                compileError("xsl:merge-action may appear only as a child of xsl:merge", "XTSE0010");
            }
        }
    
    }
}
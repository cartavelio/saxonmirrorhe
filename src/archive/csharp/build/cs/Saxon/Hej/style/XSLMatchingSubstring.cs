using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMatchingSubstring : Saxon.Hej.style.StyleElement {
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                checkUnknownAttribute(attName);
            }
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLAnalyzeString)))) {
                compileError((getDisplayName()+" must be immediately within xsl:analyze-string"), "XTSE0010");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("XSLMatchingSubstring#compile() should not be called");
        }
    
    }
}
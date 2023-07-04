using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMatchingSubstring : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select = null;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    if (requireXslt40Attribute("select")) {
                        select = makeExpression(value, att);
                    }
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return select;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLAnalyzeString)))) {
                compileError((getDisplayName()+" must be immediately within xsl:analyze-string"), "XTSE0010");
            }
            if ((select != null)) {
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if (!(((child is Saxon.Hej.style.XSLFallback)))) {
                        if ((select != null)) {
                            compileError((("An "+getDisplayName())+" element with a select attribute must be empty"), "XTSE3185");
                        }
                        break ;
                    }
                }
                select = typeCheck("select", select);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("XSLMatchingSubstring#compile() should not be called");
        }
    
    }
}
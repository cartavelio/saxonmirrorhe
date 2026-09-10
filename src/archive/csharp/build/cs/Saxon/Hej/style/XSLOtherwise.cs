using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOtherwise : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    requireXslt40("select");
                    select = makeExpression(att.getValue(), att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLChooseOrSwitch)))) {
                compileError("xsl:otherwise must be immediately within xsl:choose or xsl:switch", "XTSE0010");
            }
            if (((select != null) && hasChildNodes())) {
                compileError("xsl:otherwise element must be empty if @select is present", "XTSE0010");
            }
        }
        protected internal override bool markTailCalls() {
            Saxon.Hej.style.StyleElement last = getLastChildInstruction();
            return ((last != null) && last.markTailCalls());
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("XSLOtherwise#compile() should not be called");
        }
        public override Saxon.Hej.expr.Expression compileSequenceConstructor(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, bool includeParams) {
            if ((select == null)) {
                return base.compileSequenceConstructor(compilation, decl, includeParams);
            } else {
                return select;
            }
        }
    
    }
}
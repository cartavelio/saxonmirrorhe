using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLWhen : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression test;
        private Saxon.Hej.expr.Expression select;
        public Saxon.Hej.expr.Expression getCondition() {
            return test;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("test")) {
                    test = makeExpression(att.getValue(), att);
                } else {
                    if (f.Equals("select")) {
                        requireXslt40("select");
                        select = makeExpression(att.getValue(), att);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((test == null)) {
                reportAbsence("test");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLChooseOrSwitch)))) {
                compileError("xsl:when must be immediately within xsl:choose or xsl:switch", "XTSE0010");
            }
            test = typeCheck("test", test);
            if (((select != null) && hasChildNodes())) {
                compileError("xsl:when element must be empty if @select is present", "XTSE0010");
            }
        }
        protected internal override bool markTailCalls() {
            Saxon.Hej.style.StyleElement last = getLastChildInstruction();
            return ((last != null) && last.markTailCalls());
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
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
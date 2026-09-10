using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOnCompletion : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.StyleElement parent = (Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()));
            if (!(((parent is Saxon.Hej.style.XSLIterate)))) {
                compileError(("xsl:on-completion is not allowed as a child of "+parent.getDisplayName()), "XTSE0010");
            }
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo sib;
            while (((sib = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                if (!((((sib is Saxon.Hej.style.XSLFallback) || (sib is Saxon.Hej.style.XSLLocalParam))))) {
                    compileError("xsl:on-completion must be the first child of xsl:iterate after the xsl:param elements", "XTSE0010");
                }
            }
            if (((select != null) && (((/*1430*/Saxon.Hej.om.NodeInfo)iterateAxis(Saxon.Hej.om.AxisInfo.CHILD).next()) != null))) {
                compileError("An xsl:on-completion element with a select attribute must be empty", "XTSE3125");
            }
            select = typeCheck("select", select);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                return compileSequenceConstructor(exec, decl, true);
            } else {
                return select;
            }
        }
    
    }
}
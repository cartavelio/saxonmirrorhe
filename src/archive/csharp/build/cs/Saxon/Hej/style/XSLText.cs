using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLText : Saxon.Hej.style.XSLLeafNodeConstructor {
        private bool disable = false;
        private Saxon.Hej.value.StringValue value;
        protected internal override void prepareAttributes() {
            string disableAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("disable-output-escaping")) {
                    disableAtt = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((disableAtt != null)) {
                disable = processBooleanAttribute("disable-output-escaping", disableAtt);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            value = Saxon.Hej.value.StringValue.EMPTY_STRING;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.StyleElement)) {
                    ((Saxon.Hej.style.StyleElement)(child)).compileError("xsl:text must not contain child elements", "XTSE0010");
                    return ;
                } else {
                    value = new Saxon.Hej.value.StringValue(child.getUnicodeStringValue());
                }
            }
            base.validate(decl);
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return null;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (isExpandingText()) {
                Saxon.Hej.tree.linked.TextImpl child = (Saxon.Hej.tree.linked.TextImpl)(((/*1430*/Saxon.Hej.om.NodeInfo)iterateAxis(Saxon.Hej.om.AxisInfo.CHILD).next()));
                if ((child != null)) {
                    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> contents = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
                    compileContentValueTemplate(child, contents);
                    Saxon.Hej.expr.Expression block = Saxon.Hej.expr.instruct.Block.makeBlock(contents);
                    block.setLocation(allocateLocation());
                    return block;
                } else {
                    return new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING), disable, false);
                }
            } else {
                return new Saxon.Hej.expr.instruct.ValueOf(Saxon.Hej.expr.Literal.makeLiteral(value), disable, false);
            }
        }
    
    }
}
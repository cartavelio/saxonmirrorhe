using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class TextValueTemplateNode : Saxon.Hej.tree.linked.TextImpl {
        private Saxon.Hej.expr.Expression contentExp;
        private Saxon.Hej.style.TextValueTemplateContext staticContext;
        public TextValueTemplateNode(Saxon.Hej.str.UnicodeString value) : base (value) {
        }
        public Saxon.Hej.expr.Expression getContentExpression() {
            return contentExp;
        }
        public Saxon.Hej.style.TextValueTemplateContext getStaticContext() {
            if ((staticContext == null)) {
                staticContext = new Saxon.Hej.style.TextValueTemplateContext((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent())), this);
            }
            return staticContext;
        }
        public void parse() {
            bool disable = false;
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if (((parent is Saxon.Hej.style.XSLText) && Saxon.Hej.style.StyleElement.isYes(Saxon.Hej.value.Whitespace.trim(parent.getAttributeValue("", "disable-output-escaping"))))) {
                disable = true;
            }
            try {
                contentExp = Saxon.Hej.style.AttributeValueTemplate.make(getUnicodeStringValue().ToString(), getStaticContext());
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setLocation(this);
                System.Diagnostics.Debug.Assert((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.StyleElement));
                ((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).compileError(e);
                contentExp = new Saxon.Hej.expr.StringLiteral(getStringValue());
            }
            contentExp = new Saxon.Hej.expr.instruct.ValueOf(contentExp, disable, false);
            System.Diagnostics.Debug.Assert((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) != null));
            contentExp.setRetainedStaticContext(((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).makeRetainedStaticContext());
        }
        public void validate() {
            System.Diagnostics.Debug.Assert((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) != null));
            contentExp = ((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).typeCheck("tvt", contentExp);
        }
    
    }
}
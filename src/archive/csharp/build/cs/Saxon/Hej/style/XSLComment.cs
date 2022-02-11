using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLComment : Saxon.Hej.style.XSLLeafNodeConstructor {
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    select = makeExpression(value, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            base.validate(decl);
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return "XTSE0940";
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.Comment inst = new Saxon.Hej.expr.instruct.Comment();
            compileContent(exec, decl, inst, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE));
            return inst;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLValueOf : Saxon.Hej.style.XSLLeafNodeConstructor {
        private bool disable = false;
        private Saxon.Hej.expr.Expression separator;
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string disableAtt = null;
            string separatorAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "disable-output-escaping":
                    disableAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "separator":
                    separatorAtt = value;
                    separator = makeAttributeValueTemplate(separatorAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((disableAtt != null)) {
                disable = processBooleanAttribute("disable-output-escaping", disableAtt);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            select = typeCheck("select", select);
            separator = typeCheck("separator", separator);
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return "XTSE0870";
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if ((((separator == null) && (select != null)) && xPath10ModeIsEnabled())) {
                select = config.getTypeChecker(true).processValueOf(select, config);
            } else {
                if ((separator == null)) {
                    if ((select == null)) {
                        separator = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                    } else {
                        separator = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE);
                    }
                }
            }
            Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(select, disable, false);
            inst.setRetainedStaticContext(makeRetainedStaticContext());
            compileContent(exec, decl, inst, separator);
            return inst;
        }
    
    }
}
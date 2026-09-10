using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLSequence : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool mayContainFallback() {
            return true;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return select;
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression select) {
            this.select = select;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    select = makeExpression(value, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
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
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                select = compileSequenceConstructor(exec, decl, false);
            }
            if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) {
                select = new Saxon.Hej.expr.instruct.SequenceInstr(select);
            }
            return select;
        }
    
    }
}
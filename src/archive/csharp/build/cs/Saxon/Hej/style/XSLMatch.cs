using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMatch : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.pattern.Pattern pattern;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
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
                    if (f.Equals("pattern")) {
                        pattern = makePattern(value, "pattern");
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((select == null)) {
                select = new Saxon.Hej.expr.ContextItemExpression();
            }
            if ((pattern == null)) {
                reportAbsence("pattern");
                pattern = new Saxon.Hej.pattern.ItemTypePattern(Saxon.Hej.type.AnyItemType.getInstance());
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if (!(((child is Saxon.Hej.style.XSLFallback)))) {
                    compileError("Only xsl:fallback is allowed within xsl:match");
                    break ;
                }
            }
            select = typeCheck("select", select);
            pattern = typeCheck("pattern", pattern);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Hej.expr.PatternMatchExpression(select, pattern);
        }
    
    }
}
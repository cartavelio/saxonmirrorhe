using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLWherePopulated : Saxon.Hej.style.StyleElement {
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
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                select = compileSequenceConstructor(exec, decl, false);
            }
            return new Saxon.Hej.expr.instruct.WherePopulated(select);
        }
    
    }
}
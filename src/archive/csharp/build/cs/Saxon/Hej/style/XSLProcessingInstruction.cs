using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLProcessingInstruction : Saxon.Hej.style.XSLLeafNodeConstructor {
        internal Saxon.Hej.expr.Expression name;
        protected internal override void prepareAttributes() {
            name = prepareAttributesNameAndSelect();
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            name = typeCheck("name", name);
            select = typeCheck("select", select);
            base.validate(decl);
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return "XTSE0880";
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.ProcessingInstruction inst = new Saxon.Hej.expr.instruct.ProcessingInstruction(name);
            compileContent(exec, decl, inst, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE));
            return inst;
        }
    
    }
}
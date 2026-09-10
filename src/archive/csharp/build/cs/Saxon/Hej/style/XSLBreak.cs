using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLBreak : Saxon.Hej.style.XSLBreakOrContinue {
        private Saxon.Hej.expr.Expression select;
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
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            validatePosition();
            if ((xslIterate == null)) {
                compileError((getDisplayName()+" must be a descendant of an xsl:iterate instruction"), "XTSE3120");
            }
            if (((select != null) && hasChildNodes())) {
                compileError("An xsl:break element with a select attribute must be empty", "XTSE3125");
            }
            select = typeCheck("select", select);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression val = select;
            if ((val == null)) {
                val = compileSequenceConstructor(exec, decl, false);
            }
            Saxon.Hej.expr.Expression brake = new Saxon.Hej.expr.instruct.BreakInstr();
            brake.setRetainedStaticContext(makeRetainedStaticContext());
            return Saxon.Hej.expr.instruct.Block.makeBlock(val, brake);
        }
    
    }
}
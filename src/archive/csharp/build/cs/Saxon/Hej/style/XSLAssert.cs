using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLAssert : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression test = null;
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.expr.Expression errorCode = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string testAtt = null;
            string selectAtt = null;
            string errorCodeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "test":
                    testAtt = value;
                    test = makeExpression(testAtt, att);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "error-code":
                    errorCodeAtt = value;
                    errorCode = makeAttributeValueTemplate(errorCodeAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((testAtt == null)) {
                reportAbsence("test");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            test = typeCheck("test", test);
            if ((errorCode == null)) {
                errorCode = new Saxon.Hej.expr.StringLiteral("Q{http://www.w3.org/2005/xqt-errors}XTMM9001");
            } else {
                errorCode = typeCheck("error-code", errorCode);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (exec.getCompilerInfo().isAssertionsEnabled()) {
                Saxon.Hej.expr.Expression b = compileSequenceConstructor(exec, decl, true);
                if ((b != null)) {
                    if ((select == null)) {
                        select = b;
                    } else {
                        select = Saxon.Hej.expr.instruct.Block.makeBlock(select, b);
                        select.setLocation(allocateLocation());
                    }
                }
                if ((select == null)) {
                    select = new Saxon.Hej.expr.StringLiteral("xsl:message (no content)");
                }
                if ((errorCode is Saxon.Hej.expr.StringLiteral)) {
                    string code = ((Saxon.Hej.expr.StringLiteral)(errorCode)).stringify();
                    if ((code.Contains(":") && !(code.StartsWith("Q{")))) {
                        Saxon.Hej.om.StructuredQName name = makeQName(code, null, "error-code");
                        errorCode = new Saxon.Hej.expr.StringLiteral(name.getEQName());
                    }
                }
                Saxon.Hej.expr.instruct.MessageInstr msg = new Saxon.Hej.expr.instruct.MessageInstr(select, new Saxon.Hej.expr.StringLiteral("yes"), errorCode);
                msg.setIsAssert(true);
                if (!(((errorCode is Saxon.Hej.expr.StringLiteral)))) {
                    msg.setRetainedStaticContext(makeRetainedStaticContext());
                }
                Saxon.Hej.expr.Expression condition = Saxon.Hej.functions.SystemFunction.makeCall("not", test.getRetainedStaticContext(), test);
                return new Saxon.Hej.expr.instruct.Choose(new Saxon.Hej.expr.Expression[]{condition}, new Saxon.Hej.expr.Expression[]{msg});
            } else {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLMessage : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression terminate = null;
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.expr.Expression errorCode = null;
        private Saxon.Hej.expr.Expression timer = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string terminateAtt = null;
            string selectAtt = null;
            string errorCodeAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                string value = att.getValue();
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                switch (f) {
                  case "terminate":
                    terminateAtt = Saxon.Hej.value.Whitespace.trim(value);
                    terminate = makeAttributeValueTemplate(terminateAtt, att);
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
                    if ((attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON) && attName.getLocalPart().Equals("time"))) {
                        isExtensionAttributeAllowed(attName.getDisplayName());
                        bool timed = processBooleanAttribute("saxon:time", value);
                        if (timed) {
                            timer = makeExpression("format-dateTime(Q{http://saxon.sf.net/}timestamp(),'[Y0001]-[M01]-[D01]T[H01]:[m01]:[s01].[f,3-3] - ')", att);
                        }
                    } else {
                        checkUnknownAttribute(attName);
                    }
                    break ;
                }
            }
            if ((terminateAtt == null)) {
                terminateAtt = "no";
                terminate = makeAttributeValueTemplate(terminateAtt, null);
            }
            checkAttributeValue("terminate", terminateAtt, true, Saxon.Hej.style.StyleElement.YES_NO);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            terminate = typeCheck("terminate", terminate);
            if ((errorCode == null)) {
                errorCode = new Saxon.Hej.expr.StringLiteral("Q{http://www.w3.org/2005/xqt-errors}XTMM9000");
            } else {
                errorCode = typeCheck("error-code", errorCode);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression b = compileSequenceConstructor(exec, decl, true);
            if ((b != null)) {
                if ((select == null)) {
                    select = b;
                } else {
                    select = Saxon.Hej.expr.instruct.Block.makeBlock(select, b);
                    select.setLocation(allocateLocation());
                }
            }
            if ((timer != null)) {
                select = Saxon.Hej.expr.instruct.Block.makeBlock(timer, select);
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
            Saxon.Hej.expr.instruct.MessageInstr m = new Saxon.Hej.expr.instruct.MessageInstr(select, terminate, errorCode);
            m.setRetainedStaticContext(makeRetainedStaticContext());
            return m;
        }
    
    }
}
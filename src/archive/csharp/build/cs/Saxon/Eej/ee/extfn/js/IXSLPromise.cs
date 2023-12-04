using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLPromise : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.Expression onCompletion;
        private Saxon.Hej.expr.Expression onFailure;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    string selectAtt = att.getValue();
                    select = makeExpression(selectAtt, att);
                } else {
                    if (f.Equals("on-completion")) {
                        string completionAtt = att.getValue();
                        onCompletion = makeExpression(completionAtt, att);
                    } else {
                        if (f.Equals("on-failure")) {
                            string failureAtt = att.getValue();
                            onFailure = makeExpression(failureAtt, att);
                        } else {
                            checkUnknownAttribute(attName);
                        }
                    }
                }
            }
            if ((select == null)) {
                reportAbsence("select");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            onCompletion = typeCheck("on-completion", onCompletion);
            onFailure = typeCheck("on-failure", onFailure);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Eej.ee.extfn.js.IXSLPromise.PromiseInstr(select, onCompletion, onFailure);
        }
        private class PromiseInstr : Saxon.Hej.expr.instruct.Instruction {
            private readonly Saxon.Hej.expr.Operand selectOp;
            private Saxon.Hej.expr.Operand onCompletionOp;
            private Saxon.Hej.expr.Operand onFailureOp;
            public PromiseInstr(Saxon.Hej.expr.Expression select, Saxon.Hej.expr.Expression onCompletion, Saxon.Hej.expr.Expression onFailure)  {
                selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                if ((onCompletion != null)) {
                    onCompletionOp = new Saxon.Hej.expr.Operand(this, onCompletion, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                }
                if ((onFailure != null)) {
                    onFailureOp = new Saxon.Hej.expr.Operand(this, onFailure, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                }
            }
            public Saxon.Hej.expr.Expression getSelect() {
                return selectOp.getChildExpression();
            }
            public Saxon.Hej.expr.Expression getOnCompletion() {
                return ((onCompletionOp == null) ? null : onCompletionOp.getChildExpression());
            }
            public Saxon.Hej.expr.Expression getOnFailure() {
                return ((onFailureOp == null) ? null : onFailureOp.getChildExpression());
            }
            public override int getImplementationMethod() {
                return Saxon.Hej.expr.Expression.PROCESS_METHOD;
            }
            public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
                return operandSparseList(selectOp, onCompletionOp, onFailureOp);
            }
            public override string getExpressionName() {
                return "schedule";
            }
            public override string ToString() {
                return "promise";
            }
            public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
                @out.startElement("schedule", this);
                @out.setChildRole("select");
                selectOp.getChildExpression().export(@out);
                if ((onCompletionOp != null)) {
                    @out.setChildRole("on-completion");
                    onCompletionOp.getChildExpression().export(@out);
                }
                if ((onFailureOp != null)) {
                    @out.setChildRole("on-failure");
                    onFailureOp.getChildExpression().export(@out);
                }
                @out.endElement();
            }
            public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rm) {
                Saxon.Eej.ee.extfn.js.IXSLPromise.PromiseInstr p2 = new Saxon.Eej.ee.extfn.js.IXSLPromise.PromiseInstr(copy(getSelect(), rm), copy(getOnCompletion(), rm), copy(getOnFailure(), rm));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, p2);
                return p2;
            }
            private Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RebindingMap rebindings) {
                return ((exp == null) ? null : exp.copy(rebindings));
            }
        
        }
    
    }
}
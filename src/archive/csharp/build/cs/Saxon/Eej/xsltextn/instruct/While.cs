using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.instruct {
    public class While : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand conditionOp;
        private readonly Saxon.Hej.expr.Operand actionOp;
        public While(Saxon.Hej.expr.Expression test, Saxon.Hej.expr.Expression action)  {
            conditionOp = new Saxon.Hej.expr.Operand(this, test, Saxon.Hej.expr.OperandRole.INSPECT);
            actionOp = new Saxon.Hej.expr.Operand(this, action, Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
        }
        public Saxon.Hej.expr.Expression getTest() {
            return conditionOp.getChildExpression();
        }
        public void setTest(Saxon.Hej.expr.Expression test) {
            conditionOp.setChildExpression(test);
        }
        public Saxon.Hej.expr.Expression getAction() {
            return actionOp.getChildExpression();
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(conditionOp, actionOp);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_WHILE;
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return getAction();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.xsltextn.instruct.While exp = new Saxon.Eej.xsltextn.instruct.While(getTest().copy(rebindings), getAction().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override bool mayCreateNewNodes() {
            return !(getAction().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            while (getTest().effectiveBooleanValue(context)) {
                getAction().process(output, context);
            }
            return null;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("while", this);
            getTest().export(@out);
            getAction().export(@out);
            @out.endElement();
        }
    
    }
}
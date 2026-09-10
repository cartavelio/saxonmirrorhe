using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class WithParam {
        public static Saxon.Hej.expr.instruct.WithParam[] EMPTY_ARRAY = new Saxon.Hej.expr.instruct.WithParam[0];
        private Saxon.Hej.expr.Operand selectOp;
        private bool typeChecked = false;
        private int slotNumber = -1;
        private Saxon.Hej.value.SequenceType requiredType;
        private Saxon.Hej.om.StructuredQName variableQName;
        private Saxon.Hej.expr.elab.SequenceEvaluator evaluator = null;
        public WithParam()  {
        }
        public void setSelectExpression(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression select) {
            selectOp = new Saxon.Hej.expr.Operand(parent, select, Saxon.Hej.expr.OperandRole.NAVIGATE);
        }
        public Saxon.Hej.expr.Operand getSelectOperand() {
            return selectOp;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return selectOp.getChildExpression();
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType required) {
            requiredType = required;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        public void setSlotNumber(int s) {
            slotNumber = s;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName s) {
            variableQName = s;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableQName;
        }
        public void setTypeChecked(bool @checked) {
            typeChecked = @checked;
        }
        public int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_WITH_PARAM;
        }
        public static void simplify(Saxon.Hej.expr.instruct.WithParam[] @params) {
            if ((@params != null)) {
                foreach (Saxon.Hej.expr.instruct.WithParam param in @params) {
                    param.selectOp.setChildExpression(param.selectOp.getChildExpression().simplify());
                }
            }
        }
        public static void typeCheck(Saxon.Hej.expr.instruct.WithParam[] @params, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((@params != null)) {
                foreach (Saxon.Hej.expr.instruct.WithParam param in @params) {
                    param.selectOp.typeCheck(visitor, contextItemType);
                }
            }
        }
        public static void optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.instruct.WithParam[] @params, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((@params != null)) {
                foreach (Saxon.Hej.expr.instruct.WithParam param in @params) {
                    param.selectOp.optimize(visitor, contextItemType);
                }
            }
        }
        public Saxon.Hej.expr.elab.SequenceEvaluator getEvaluator() {
            if ((evaluator == null)) {
                makeEvaluator();
            }
            return evaluator;
        }
        private void makeEvaluator() {
            Saxon.Hej.expr.Expression select = selectOp.getChildExpression();
            evaluator = new Saxon.Hej.expr.elab.LearningEvaluator(select, select.makeElaborator().lazily(true));
        }
        public static Saxon.Hej.expr.instruct.WithParam[] copy(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.instruct.WithParam[] @params, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            if ((@params == null)) {
                return null;
            }
            Saxon.Hej.expr.instruct.WithParam[] result = new Saxon.Hej.expr.instruct.WithParam[@params.Length];
            for (int i = 0; (i<@params.Length); i++) {
                result[i] = new Saxon.Hej.expr.instruct.WithParam();
                result[i].slotNumber = @params[i].slotNumber;
                result[i].typeChecked = @params[i].typeChecked;
                result[i].selectOp = new Saxon.Hej.expr.Operand(parent, @params[i].selectOp.getChildExpression().copy(rebindings), Saxon.Hej.expr.OperandRole.NAVIGATE);
                result[i].requiredType = @params[i].requiredType;
                result[i].variableQName = @params[i].variableQName;
            }
            return result;
        }
        public static void gatherOperands(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.instruct.WithParam[] @params, System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list) {
            if ((@params != null)) {
                foreach (Saxon.Hej.expr.instruct.WithParam param in @params) {
                    list.Add(param.selectOp);
                }
            }
        }
        public static void exportParameters(Saxon.Hej.expr.instruct.WithParam[] @params, Saxon.Hej.trace.ExpressionPresenter @out, bool tunnel) {
            if ((@params != null)) {
                foreach (Saxon.Hej.expr.instruct.WithParam param in @params) {
                    @out.startElement("withParam");
                    @out.emitAttribute("name", param.variableQName);
                    string flags = "";
                    if (tunnel) {
                        flags += "t";
                    }
                    if (param.isTypeChecked()) {
                        flags += "c";
                    }
                    if (!(flags.Length==0)) {
                        @out.emitAttribute("flags", flags);
                    }
                    if ((param.getRequiredType() != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                        @out.emitAttribute("as", param.getRequiredType().toAlphaCode());
                    }
                    if ((param.getSlotNumber() != -1)) {
                        @out.emitAttribute("slot", (param.getSlotNumber()+""));
                    }
                    param.selectOp.getChildExpression().export(@out);
                    @out.endElement();
                }
            }
        }
        public Saxon.Hej.om.Sequence getSelectValue(Saxon.Hej.expr.XPathContext context) {
            if ((evaluator == null)) {
                makeEvaluator();
            }
            int savedOutputState = context.getTemporaryOutputState();
            context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_WITH_PARAM);
            Saxon.Hej.om.Sequence result = evaluator.evaluate(context);
            context.setTemporaryOutputState(savedOutputState);
            return result;
        }
        public bool isTypeChecked() {
            return typeChecked;
        }
    
    }
}
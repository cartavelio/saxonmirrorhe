using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class CopyModifyExpression : Saxon.Hej.expr.Expression {
        private readonly int validationMode;
        private readonly Saxon.Hej.expr.Operand updateOp;
        private readonly Saxon.Hej.expr.Operand returnOp;
        private readonly Saxon.Hej.expr.Operand copyBindingsOp;
        public CopyModifyExpression(Saxon.Hej.expr.Expression updateExp, Saxon.Hej.expr.Expression returnExp, int validationMode)  {
            updateOp = new Saxon.Hej.expr.Operand(this, updateExp, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            returnOp = new Saxon.Hej.expr.Operand(this, returnExp, Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            copyBindingsOp = new Saxon.Hej.expr.Operand(this, returnExp, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            this.validationMode = validationMode;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(updateOp, returnOp, copyBindingsOp);
        }
        public Saxon.Hej.expr.Expression getUpdateExp() {
            return updateOp.getChildExpression();
        }
        public void setUpdateExp(Saxon.Hej.expr.Expression updateExp) {
            updateOp.setChildExpression(updateExp);
        }
        public Saxon.Hej.expr.Expression getReturnExp() {
            return returnOp.getChildExpression();
        }
        public void setReturnExp(Saxon.Hej.expr.Expression returnExp) {
            returnOp.setChildExpression(returnExp);
        }
        public Saxon.Hej.expr.Expression getCopyBindings() {
            return copyBindingsOp.getChildExpression();
        }
        public void setCopyBindings(Saxon.Hej.expr.Expression copyBindings) {
            copyBindingsOp.setChildExpression(copyBindings);
        }
        protected internal override int computeCardinality() {
            return getReturnExp().getCardinality();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getReturnExp().getItemType();
        }
        public override bool isUpdatingExpression() {
            return false;
        }
        public override string getExpressionName() {
            return "copy-modify";
        }
        public override void checkForUpdatingSubexpressions() {
            if (Saxon.Hej.expr.parser.ExpressionTool.isNotAllowedInUpdatingContext(getUpdateExp())) {
                throw new Saxon.Hej.trans.XPathException("Expression in modify clause must be an updating expression (or () or error())", "XUST0002");
            }
            if (getReturnExp().isUpdatingExpression()) {
                throw new Saxon.Hej.trans.XPathException("Expression in return clause must not be an updating expression", "XUST0001");
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.update.CopyModifyExpression exp = new Saxon.Eej.ee.update.CopyModifyExpression(getUpdateExp().copy(rebindings), getReturnExp().copy(rebindings), validationMode);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            exp.setCopyBindings(getCopyBindings().copy(rebindings));
            return exp;
        }
        public override int getImplementationMethod() {
            return ((EVALUATE_METHOD|ITERATE_METHOD)|PROCESS_METHOD);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("copyModify", this);
            @out.emitAttribute("val", Saxon.Hej.lib.Validation.describe(validationMode));
            @out.setChildRole("bindings");
            getCopyBindings().export(@out);
            @out.setChildRole("modify");
            getUpdateExp().export(@out);
            @out.setChildRole("return");
            getReturnExp().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.iterate()*/makeElaborator().elaborateForPull()(context);
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForItem()(context);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            dispatchTailCall(/*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context));
        }
        private void doUpdates(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.PendingUpdateList pul = context.getConfiguration().newPendingUpdateList();
            /*Delegate.registerUpdates()*/getUpdateExp().makeElaborator().elaborateForUpdate()(context, pul);
            pul.apply(context, validationMode);
            System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> copiedNodes = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
            Saxon.Hej.om.SequenceTool.supply(getCopyBindings().iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((node => copiedNodes.Add((Saxon.Hej.om.NodeInfo)(node)))));
            if (!((pul.getAffectedTrees()).All(copiedNodes.Contains))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("A node that was not created during the copy phase has been updated during the modify phase");
                err.setErrorCode("XUDY0014");
                err.setLocation(getLocation());
                err.setXPathContext(context);
                throw err;
            }
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.update.CopyModifyExpression.CopyModifyExpressionElaborator();
        }
        private class CopyModifyExpressionElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Eej.ee.update.CopyModifyExpression expr = (Saxon.Eej.ee.update.CopyModifyExpression)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForPush();
                return (output, context) => {
                    expr.doUpdates(context);
                    return /*Delegate.processLeavingTail()*/returnEval(output, context);
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Eej.ee.update.CopyModifyExpression expr = (Saxon.Eej.ee.update.CopyModifyExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForItem();
                return context => {
                    expr.doUpdates(context);
                    return /*Delegate.eval()*/returnEval(context);
                } ;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Eej.ee.update.CopyModifyExpression expr = (Saxon.Eej.ee.update.CopyModifyExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator returnEval = expr.getReturnExp().makeElaborator().elaborateForPull();
                return context => {
                    expr.doUpdates(context);
                    return /*Delegate.iterate()*/returnEval(context);
                } ;
            }
        
        }
    
    }
}
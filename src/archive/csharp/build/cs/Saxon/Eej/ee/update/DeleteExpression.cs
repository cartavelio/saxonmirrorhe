using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class DeleteExpression : Saxon.Eej.ee.update.BasicUpdatingExpression {
        private readonly Saxon.Hej.expr.Operand operand;
        public DeleteExpression(Saxon.Hej.expr.Expression target)  {
            operand = new Saxon.Hej.expr.Operand(this, target, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(operand);
        }
        public Saxon.Hej.expr.Expression getTargetExp() {
            return operand.getChildExpression();
        }
        public void setTargetExp(Saxon.Hej.expr.Expression targetExp) {
            operand.setChildExpression(targetExp);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier = () => {
                Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "delete", 0);
                role0.setErrorCode("XUTY0007");
                return role0;
            } ;
            setTargetExp(visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(getTargetExp(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, roleSupplier, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.update.DeleteExpression d2 = new Saxon.Eej.ee.update.DeleteExpression(getTargetExp().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, d2);
            return d2;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("delete", this);
            getTargetExp().export(@out);
            @out.endElement();
        }
        public override string getExpressionName() {
            return "delete";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.update.DeleteExpression.DeleteExpressionElaborator();
        }
        private class DeleteExpressionElaborator : Saxon.Hej.expr.elab.UpdateElaborator {
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Eej.ee.update.DeleteExpression expr = (Saxon.Eej.ee.update.DeleteExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator targetPull = expr.getTargetExp().makeElaborator().elaborateForPull();
                return (context, pul) => {
                    try {
                        Saxon.Hej.om.SequenceIterator iter = /*Delegate.iterate()*/targetPull(context);
                        Saxon.Hej.om.NodeInfo node;
                        while (((node = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                            if (!((node.getTreeInfo().isMutable()))) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Node to be deleted is not an updatable node", Saxon.Hej.trans.SaxonErrorCode.SXUP0081);
                                err.setLocation(expr.getLocation());
                                throw err;
                            }
                            if ((node.getParent() == null)) {
                            } else {
                                try {
                                    Saxon.Eej.ee.update.DeleteAction action = new Saxon.Eej.ee.update.DeleteAction(node);
                                    action.setOriginator(expr);
                                    ((Saxon.Eej.ee.update.PendingUpdateListImpl)(pul)).add(action);
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    e.maybeSetLocation(expr.getLocation());
                                    e.maybeSetContext(context);
                                    throw;
                                }
                            }
                        }
                    } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                        throw e.getXPathException();
                    }
                } ;
            }
        
        }
    
    }
}
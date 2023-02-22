using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.instruct {
    public class DoInstr : Saxon.Hej.expr.instruct.Instruction {
        private readonly Saxon.Hej.expr.Operand actionOp;
        public DoInstr(Saxon.Hej.expr.Expression select)  {
            actionOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.NAVIGATE);
        }
        public Saxon.Hej.expr.Expression getActionExpr() {
            return actionOp.getChildExpression();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return actionOp;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.xsltextn.instruct.DoInstr a2 = new Saxon.Eej.xsltextn.instruct.DoInstr(getActionExpr().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            return a2;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_DO;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("do", this);
            getActionExpr().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.xsltextn.instruct.DoInstr.DoInstructionElaborator();
        }
        public class DoInstructionElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Eej.xsltextn.instruct.DoInstr expr = (Saxon.Eej.xsltextn.instruct.DoInstr)(getExpression());
                Saxon.Hej.expr.elab.SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
                return (output, context) => {
                    puller.evaluate(context);
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Eej.xsltextn.instruct.DoInstr expr = (Saxon.Eej.xsltextn.instruct.DoInstr)(getExpression());
                Saxon.Hej.expr.elab.SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
                return context => {
                    puller.evaluate(context);
                    return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Eej.xsltextn.instruct.DoInstr expr = (Saxon.Eej.xsltextn.instruct.DoInstr)(getExpression());
                Saxon.Hej.expr.elab.SequenceEvaluator puller = expr.getActionExpr().makeElaborator().eagerly();
                return context => {
                    puller.evaluate(context);
                    return null;
                } ;
            }
        
        }
    
    }
}
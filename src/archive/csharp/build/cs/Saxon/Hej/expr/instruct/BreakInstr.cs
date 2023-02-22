using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class BreakInstr : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.TailCallLoop.TailCallInfo {
        public BreakInstr()  {
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.BreakInstr b2 = new Saxon.Hej.expr.instruct.BreakInstr();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, b2);
            return b2;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_BREAK;
        }
        public void markContext(Saxon.Hej.expr.XPathContext context) {
            context.getMajorContext().requestTailCall(this, null);
        }
        public override string getExpressionName() {
            return "xsl:break";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("break", this);
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.BreakInstr.BreakElaborator();
        }
        public class BreakElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.BreakInstr expr = (Saxon.Hej.expr.instruct.BreakInstr)(getExpression());
                return (output, context) => {
                    expr.markContext(context);
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.instruct.BreakInstr expr = (Saxon.Hej.expr.instruct.BreakInstr)(getExpression());
                return (context) => {
                    expr.markContext(context);
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.instruct.BreakInstr expr = (Saxon.Hej.expr.instruct.BreakInstr)(getExpression());
                return (context) => {
                    expr.markContext(context);
                    return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                } ;
            }
        
        }
    
    }
}
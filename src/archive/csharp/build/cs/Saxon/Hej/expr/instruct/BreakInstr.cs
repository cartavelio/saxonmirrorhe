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
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            markContext(context);
            return null;
        }
        public void markContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext c = context;
            while (!(((c is Saxon.Hej.expr.XPathContextMajor)))) {
                c = c.getCaller();
            }
            ((Saxon.Hej.expr.XPathContextMajor)(c)).requestTailCall(this, null);
        }
        public override string getExpressionName() {
            return "xsl:break";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("break", this);
            @out.endElement();
        }
    
    }
}
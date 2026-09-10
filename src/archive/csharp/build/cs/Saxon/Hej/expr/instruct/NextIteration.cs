using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class NextIteration : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.TailCallLoop.TailCallInfo {
        private Saxon.Hej.expr.instruct.WithParam[] actualParams = null;
        public NextIteration()  {
        }
        public void setParameters(Saxon.Hej.expr.instruct.WithParam[] actualParams) {
            this.actualParams = actualParams;
        }
        public Saxon.Hej.expr.instruct.WithParam[] getParameters() {
            return actualParams;
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_NEXT_ITERATION;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            Saxon.Hej.expr.instruct.WithParam.simplify(actualParams);
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.instruct.WithParam.typeCheck(actualParams, visitor, contextInfo);
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.NextIteration c2 = new Saxon.Hej.expr.instruct.NextIteration();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, c2);
            c2.actualParams = Saxon.Hej.expr.instruct.WithParam.copy(c2, actualParams, rebindings);
            return c2;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(this, actualParams, list);
            return list;
        }
        public override string getStreamerName() {
            return "NextIteration";
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.XPathContext c = context;
            while (!(((c is Saxon.Hej.expr.XPathContextMajor)))) {
                c = c.getCaller();
            }
            Saxon.Hej.expr.XPathContextMajor cm = (Saxon.Hej.expr.XPathContextMajor)(c);
            if ((actualParams.Length == 1)) {
                cm.setLocalVariable(actualParams[0].getSlotNumber(), actualParams[0].getSelectValue(context));
            } else {
                Saxon.Hej.om.Sequence[] oldVars = cm.getAllVariableValues();
                Saxon.Hej.om.Sequence[] newVars = Saxon.Ejava.util.Arrays.copyOf(oldVars, oldVars.Length);
                foreach (Saxon.Hej.expr.instruct.WithParam wp in actualParams) {
                    newVars[wp.getSlotNumber()] = wp.getSelectValue(context);
                }
                cm.resetAllVariableValues(newVars);
            }
            cm.requestTailCall(this, null);
            return null;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("nextIteration", this);
            if (((actualParams != null) && (actualParams.Length>0))) {
                Saxon.Hej.expr.instruct.WithParam.exportParameters(actualParams, @out, false);
            }
            @out.endElement();
        }
    
    }
}
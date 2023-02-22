using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SortExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.sort.SortExpression exp = (Saxon.Hej.expr.sort.SortExpression)(getExpression());
            Saxon.Hej.expr.Expression sortInput = exp.getBaseExpression();
            Saxon.Eej.ee.stream.PostureAndSweep basePS = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(sortInput, contextInfo, new System.Collections.Generic.List<string>());
            if ((basePS.getPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                reasons.Add("A sort expression is not streamable unless the input sequence is grounded");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> expandedOperands = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            expandedOperands.Add(exp.getBaseOperand());
            Saxon.Hej.expr.sort.SortKeyDefinitionList skdl = exp.getSortKeyDefinitionList();
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in skdl) {
                foreach (Saxon.Hej.expr.Operand o in skd.operands()) {
                    expandedOperands.Add(o);
                }
            }
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(getExpression(), expandedOperands, contextInfo, reasons);
            if (((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.MOTIONLESS) || (ps.getSweep() == Saxon.Eej.ee.stream.Sweep.CONSUMING))) {
                /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(skdl, Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS);
                foreach (Saxon.Hej.expr.sort.SortKeyDefinition skd in skdl) {
                    /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(skd, Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS);
                }
            }
            return ps;
        }
    
    }
}
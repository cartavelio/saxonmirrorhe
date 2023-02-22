using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AscentFunctionCallAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if ((reasons == null)) {
                reasons = new System.Collections.Generic.List<string>();
            }
            Saxon.Hej.expr.UserFunctionCall exp = (Saxon.Hej.expr.UserFunctionCall)(getExpression());
            Saxon.Hej.expr.instruct.UserFunction target = exp.getFunction();
            Saxon.Hej.expr.oper.OperandArray operanda = exp.getOperanda();
            operanda.getOperand(0).setUsage(Saxon.Hej.expr.OperandUsage.TRANSMISSION);
            for (int i = 1; (i<operanda.getNumberOfOperands()); i++) {
                operanda.getOperand(i).setUsage(Saxon.Hej.expr.Operand.typeDeterminedUsage(target.getArgumentType(i).getPrimaryType()));
            }
            Saxon.Eej.ee.stream.PostureAndSweep ps0 = /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(exp, operanda, contextInfo, reasons);
            if ((ps0.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                reasons.Add("The sweep of the function is not motionless");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if ((ps0.getPosture() == Saxon.Eej.ee.stream.Posture.ROAMING)) {
                reasons.Add("The posture of the function is roaming");
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            if ((ps0.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            if ((th.relationship(target.getResultType().getPrimaryType(), Saxon.Hej.pattern.AnyNodeTest.getInstance()) != Saxon.Hej.type.Affinity.DISJOINT)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
            } else {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            }
        }
    
    }
}
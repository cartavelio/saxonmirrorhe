using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CurrentGroupStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.functions.CurrentGroupCall exp = (Saxon.Hej.functions.CurrentGroupCall)(getExpression());
            Saxon.Hej.expr.instruct.ForEachGroup feg = exp.getControllingInstruction();
            if ((feg == null)) {
                if ((reasons != null)) {
                    reasons.Add("The current-group() function has no containing xsl:for-each-group instruction");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(feg.getSelectExpression(), contextInfo, reasons);
            if ((ps.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            } else {
                if (exp.isInHigherOrderOperand()) {
                    if ((reasons != null)) {
                        reasons.Add("The current-group() function is evaluated repeatedly");
                    }
                    return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
                } else {
                    return ps;
                }
            }
        }
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return /*134*/Saxon.Eej.ee.stream.CurrentGroupPattern.getInstance();
        }
    
    }
}
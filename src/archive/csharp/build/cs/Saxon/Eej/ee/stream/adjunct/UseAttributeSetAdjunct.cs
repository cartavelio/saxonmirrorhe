using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class UseAttributeSetAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.UseAttributeSet exp = (Saxon.Hej.expr.instruct.UseAttributeSet)(getExpression());
            if (!(exp.isDeclaredStreamable())) {
                if ((reasons != null)) {
                    reasons.Add((("Attribute set "+exp.getTargetAttributeSetName().getDisplayName())+" is not declared streamable"));
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            return base.computeStreamability(contextInfo, reasons);
        }
    
    }
}
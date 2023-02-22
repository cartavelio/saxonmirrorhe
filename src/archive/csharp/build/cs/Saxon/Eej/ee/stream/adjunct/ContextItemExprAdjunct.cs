using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ContextItemExprAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return Saxon.Hej.pattern.AnchorPattern.getInstance();
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            if (!(contextInfo.getItemType().getUType().overlaps(Saxon.Hej.type.UType.ANY_NODE))) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            }
            return new Saxon.Eej.ee.stream.PostureAndSweep(contextInfo.getContextItemPosture(), Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
        }
    
    }
}
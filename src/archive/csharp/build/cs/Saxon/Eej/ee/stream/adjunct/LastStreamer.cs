using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class LastStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return null;
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Eej.ee.stream.Posture cp = contextInfo.getContextItemPosture();
            if ((((cp == Saxon.Eej.ee.stream.Posture.STRIDING) || (cp == Saxon.Eej.ee.stream.Posture.CRAWLING)) || (cp == Saxon.Eej.ee.stream.Posture.ROAMING))) {
                if ((reasons != null)) {
                    reasons.Add("Call to last() is not streamable when processing streamed nodes");
                }
                return Saxon.Eej.ee.stream.PostureAndSweep.ROAMING_AND_FREE_RANGING;
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
        }
    
    }
}
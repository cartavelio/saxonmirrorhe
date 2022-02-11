using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class RootStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return null;
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.SystemFunctionCall exp = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(exp.getArg(0), contextInfo, reasons);
            switch (/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(exp.getArg(0))) {
              case Saxon.Eej.ee.stream.Posture.GROUNDED:
              case Saxon.Eej.ee.stream.Posture.ROAMING:
                return ps;
              default:
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, ps.getSweep());
            }
        }
    
    }
}
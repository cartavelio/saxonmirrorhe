using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class RootExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            return new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.DOCUMENT);
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Eej.ee.stream.Posture contextItemPosture = ((Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(contextInfo)).getContextItemPosture();
            if ((contextItemPosture == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS;
            }
            if (contextInfo.getItemType().getUType().Equals(Saxon.Hej.type.UType.DOCUMENT)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(contextItemPosture, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
            }
            if ((reasons != null)) {
                reasons.Add((("Expression "+getExpression().toShortString())+" is free-ranging"));
            }
            return Saxon.Eej.ee.stream.PostureAndSweep.CLIMBING_AND_MOTIONLESS;
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Streaming is not supported for expression "+getExpression().toShortString()));
            err.setErrorCode(Saxon.Hej.trans.SaxonErrorCode.SXST0060);
            err.setLocation(getExpression().getLocation());
            throw err;
        }
    
    }
}
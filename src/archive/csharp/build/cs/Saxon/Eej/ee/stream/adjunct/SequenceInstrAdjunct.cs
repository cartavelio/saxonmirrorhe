using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SequenceInstrAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Hej.pattern.Pattern toStreamingPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.SequenceInstr)(getExpression())).getBaseExpression();
            return /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(config, select).toStreamingPattern(config);
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.SequenceInstr)(getExpression())).getBaseExpression();
            return /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(select, contextInfo, reasons);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.SequenceInstr)(getExpression())).getBaseExpression();
            Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
            return sa.getWatchMaker(forGrouping);
        }
    
    }
}
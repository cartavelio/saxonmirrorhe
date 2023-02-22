using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ItemCheckerAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.ItemChecker expr = (Saxon.Hej.expr.ItemChecker)(getExpression());
            Saxon.Eej.ee.stream.PostureAndSweep @base = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(expr.getBaseExpression(), contextInfo, reasons);
            if ((((@base.getPosture() != Saxon.Eej.ee.stream.Posture.ROAMING) && (@base.getSweep() != Saxon.Eej.ee.stream.Sweep.FREE_RANGING)) && (expr.getRequiredType() is Saxon.Hej.pattern.DocumentNodeTest))) {
                reasons.Add((("The node-test "+expr.getRequiredType())+" requires look-ahead"));
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.ROAMING, Saxon.Eej.ee.stream.Sweep.FREE_RANGING);
            }
            return @base;
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.ItemCheckingFeed(getExpression(), @out, context);
        }
    
    }
}
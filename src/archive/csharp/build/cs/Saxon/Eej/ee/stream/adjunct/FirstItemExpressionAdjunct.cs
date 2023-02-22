using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class FirstItemExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Eej.ee.stream.PostureAndSweep ps = base.computeStreamability(contextInfo, reasons);
            if ((ps.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, ps.getSweep());
            } else {
                return ps;
            }
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            return new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => ((position == 1) ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE));
        }
    
    }
}
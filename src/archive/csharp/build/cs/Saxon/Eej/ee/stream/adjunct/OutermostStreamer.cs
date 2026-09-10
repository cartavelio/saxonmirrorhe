using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class OutermostStreamer : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Eej.ee.stream.PostureAndSweep ps = base.computeStreamability(contextInfo, reasons);
            if ((ps.getPosture() == Saxon.Eej.ee.stream.Posture.CRAWLING)) {
                return new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.STRIDING, ps.getSweep());
            } else {
                return ps;
            }
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            return new Saxon.Eej.ee.stream.adjunct.OutermostStreamer.OutermostFilter();
        }
        private class OutermostFilter : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            private Saxon.Hej.om.NodeInfo previous = null;
            public OutermostFilter()  {
            }
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                if (((previous == null) || !(Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(previous, (Saxon.Hej.om.NodeInfo)(item))))) {
                    previous = (Saxon.Hej.om.NodeInfo)(item);
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
                } else {
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP;
                }
            }
        
        }
    
    }
}
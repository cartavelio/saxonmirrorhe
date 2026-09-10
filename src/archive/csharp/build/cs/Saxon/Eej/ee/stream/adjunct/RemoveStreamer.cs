using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class RemoveStreamer : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.value.IntegerValue startD = (Saxon.Hej.value.IntegerValue)(fnc.getArg(1).evaluateItem(context));
            return new Saxon.Eej.ee.stream.adjunct.RemoveStreamer.RemoveFilter((int)(startD.longValue()));
        }
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        private class RemoveFilter : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            private readonly int removeIndex;
            public RemoveFilter(int removeIndex)  {
                this.removeIndex = removeIndex;
            }
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                return ((position != removeIndex) ? /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP);
            }
        
        }
    
    }
}
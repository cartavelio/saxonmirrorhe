using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SubscriptExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.expr.SubscriptExpression fnc = (Saxon.Hej.expr.SubscriptExpression)(getExpression());
            Saxon.Hej.value.NumericValue startD = (Saxon.Hej.value.NumericValue)(fnc.getSubscriptExpression().evaluateItem(context));
            int subscript = (startD.isWholeNumber() ? (int)(startD.round(0).longValue()) : -1);
            return new Saxon.Eej.ee.stream.adjunct.SubsequenceStreamer.SubsequenceFilter(subscript, subscript);
        }
    
    }
}
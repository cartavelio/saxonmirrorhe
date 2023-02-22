using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ElementCreatorAdjunct : Saxon.Eej.ee.stream.adjunct.ParentConstructorAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            Saxon.Hej.expr.instruct.ElementCreator exp = (Saxon.Hej.expr.instruct.ElementCreator)(getExpression());
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getConsumingOperand(exp).getChildExpression() == exp.getContentExpression())) {
                return this;
            } else {
                return getDefaultFeedMaker(arg);
            }
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.ElementCreatorFeed(getExpression(), @out, context);
        }
    
    }
}
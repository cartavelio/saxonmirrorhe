using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CopyOfAdjunct : Saxon.Eej.ee.stream.adjunct.ComposingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeComposingAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.feed.CopyOfFeed feed = new Saxon.Eej.ee.stream.feed.CopyOfFeed(watchManager, @out, context);
            Saxon.Hej.expr.instruct.CopyOf exp = (Saxon.Hej.expr.instruct.CopyOf)(getExpression());
            feed.setCopyNamespaces(exp.isCopyNamespaces());
            feed.setCopyAccumulators(exp.isCopyAccumulators());
            feed.setValidationOptions(exp.getValidationAction(), exp.getSchemaType());
            return feed;
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return makeComposingAction(watchManager, @out, context);
        }
    
    }
}
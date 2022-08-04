using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class NotFnStreamer : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.BooleanFnFeed.Negative(watchManager, getExpression(), @out, context);
        }
    
    }
}
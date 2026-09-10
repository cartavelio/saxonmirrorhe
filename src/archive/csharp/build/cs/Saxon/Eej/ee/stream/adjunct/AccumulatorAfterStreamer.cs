using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AccumulatorAfterStreamer : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            return getInspectionWatchMaker(getExpression(), new Saxon.Hej.expr.ContextItemExpression(), forGrouping);
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Inner_ItemFeed_d202e110(this, getExpression(), @out, context);
        }
    
        private class Inner_ItemFeed_d202e110 : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly AccumulatorAfterStreamer outerInstance;
            public Inner_ItemFeed_d202e110(AccumulatorAfterStreamer outerInstance, /*AB*/Saxon.Hej.expr.Expression arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
            }
            public override void append(Saxon.Hej.om.Item item) {
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                /*superMethod*/processItems(/*superMethod*/getExpression().iterate(/*superMethod*/getContext()), /*superMethod*/getResultFeed());
            }
        }

    }
}
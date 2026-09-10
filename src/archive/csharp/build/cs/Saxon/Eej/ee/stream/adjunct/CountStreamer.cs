using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CountStreamer : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Inner_ExistenceFeed_d225e69(this, getExpression(), @out, context);
        }
    
        private class Inner_ExistenceFeed_d225e69 : Saxon.Eej.ee.stream.feed.ExistenceFeed {
            private readonly CountStreamer outerInstance;
            public Inner_ExistenceFeed_d225e69(CountStreamer outerInstance, /*AB*/Saxon.Hej.expr.Expression arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
                count = 0;
            }
            internal long count;
            public override void action() {
                /*field*/count++;
            }
            public override void close() {
                /*superMethod*/getNextOutputter().append(Saxon.Hej.value.Int64Value.makeIntegerValue(/*field*/count));
                /*field*/count = 0;
                /*superMethod*/getNextOutputter().close();
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class EmptyStreamer : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Inner_ExistenceFeed_d224e69(this, getExpression(), @out, context);
        }
    
        private class Inner_ExistenceFeed_d224e69 : Saxon.Eej.ee.stream.feed.ExistenceFeed {
            private readonly EmptyStreamer outerInstance;
            public Inner_ExistenceFeed_d224e69(EmptyStreamer outerInstance, /*AB*/Saxon.Hej.expr.Expression arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
                found = false;
            }
            internal bool found;
            public override void action() {
                if (!(/*field*/found)) {
                    /*superMethod*/getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
                    /*superMethod*/getNextOutputter().close();
                    /*field*/found = true;
                    /*superMethod*/getTerminator().terminate();
                }
            }
            public override void close() {
                if (!(/*field*/found)) {
                    /*superMethod*/getNextOutputter().append(Saxon.Hej.value.BooleanValue.TRUE);
                    /*superMethod*/getNextOutputter().close();
                }
            }
        }

    }
}
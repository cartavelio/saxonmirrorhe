using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class EmptyStreamer : Saxon.Eej.ee.stream.adjunct.InspectionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.EmptyStreamer.EmptyExistenceFeed(getExpression(), @out, context, watchManager);
        }
        private class EmptyExistenceFeed : Saxon.Eej.ee.stream.feed.ExistenceFeed {
            internal bool found = false;
            internal bool closed = false;
            internal Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            protected internal EmptyExistenceFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.watch.WatchManager watchManager) : base (exp, result, context) {
                this.watchManager = watchManager;
            }
            public override void action() {
                if (!(found)) {
                    getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
                    found = true;
                    if (watchManager.allowsEarlyExit()) {
                        closed = true;
                        getNextOutputter().close();
                        getTerminator().terminate();
                    }
                }
            }
            public override void close() {
                if (!(found)) {
                    getNextOutputter().append(Saxon.Hej.value.BooleanValue.TRUE);
                }
                if (!(closed)) {
                    getNextOutputter().close();
                }
            }
        
        }
    
    }
}
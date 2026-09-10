using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class OnNonEmptyAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer feedToEventBuffer = new Inner_FeedToEventBuffer_d285e62(this, @out, @out, context);
            feedToEventBuffer.setBuffer(((Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed)(@out)).getTentativeContent());
            return feedToEventBuffer;
        }
    
        private class Inner_FeedToEventBuffer_d285e62 : Saxon.Eej.ee.stream.adjunct.FeedToEventBuffer {
            private readonly OnNonEmptyAdjunct outerInstance;
            public Inner_FeedToEventBuffer_d285e62(OnNonEmptyAdjunct outerInstance, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(/*localparam*/terminator);
            }
            public override void close() {
                ((Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed)(/*superMethod*/getNextOutputter())).flushIfNonEmpty();
                /*superMethod*/getNextOutputter().close();
            }
        }

    }
}
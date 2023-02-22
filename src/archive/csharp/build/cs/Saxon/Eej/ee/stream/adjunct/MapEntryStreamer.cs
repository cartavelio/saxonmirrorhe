using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class MapEntryStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 1) ? this : getDefaultFeedMaker(arg));
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.MapEntryStreamer.MapEntryFeed(getExpression(), @out, context);
        }
        private class MapEntryFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal System.Collections.Generic.IList<Saxon.Hej.om.Item> value;
            public MapEntryFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                value = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            }
            public override void append(Saxon.Hej.om.Item item) {
                value.Add(item);
            }
            public override void close() {
                Saxon.Hej.value.AtomicValue key = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.SystemFunctionCall)(getExpression())).getArg(0).evaluateItem(getContext()));
                Saxon.Hej.om.GroundedValue seq = Saxon.Hej.value.SequenceExtent.makeSequenceExtent(value);
                Saxon.Impl.DataModel.ImmMap map = Saxon.Impl.DataModel.ImmMap.singleton(key, seq);
                getNextOutputter().append(map);
                getNextOutputter().close();
            }
        
        }
    
    }
}
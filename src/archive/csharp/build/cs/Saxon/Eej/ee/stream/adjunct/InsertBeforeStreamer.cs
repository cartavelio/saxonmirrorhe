using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class InsertBeforeStreamer : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private int consumingArg = -1;
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return base.getWatchMaker(forGrouping);
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            switch (consumingArg) {
              case 0:
                return new Saxon.Eej.ee.stream.adjunct.InsertBeforeStreamer.InsertBeforeFeed1((Saxon.Hej.expr.SystemFunctionCall)(getExpression()), @out, context);
              case 2:
                return new Saxon.Eej.ee.stream.adjunct.InsertBeforeStreamer.InsertBeforeFeed3((Saxon.Hej.expr.SystemFunctionCall)(getExpression()), @out, context);
              default:
                return getDefaultFeedMaker(consumingArg).makeItemFeed(watchManager, @out, context);
            }
        }
        private class InsertBeforeFeed1 : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
            internal int position;
            internal int insertPosition;
            public InsertBeforeFeed1(Saxon.Hej.expr.SystemFunctionCall exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                this.insertPosition = (int)(((Saxon.Hej.value.NumericValue)(call.getArg(1).evaluateItem(getContext()))).longValue());
                if ((insertPosition<=0)) {
                    doInsertion();
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((++position == insertPosition)) {
                    doInsertion();
                }
                getNextOutputter().append(item);
            }
            private void doInsertion() {
                Saxon.Hej.expr.SystemFunctionCall function = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.om.SequenceIterator iter = function.getArg(2).iterate(getContext());
                processItems(iter, getNextOutputter());
            }
            public override void close() {
                if ((++position<=insertPosition)) {
                    doInsertion();
                }
                getNextOutputter().close();
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if ((++position == insertPosition)) {
                    doInsertion();
                }
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        
        }
        private class InsertBeforeFeed3 : Saxon.Eej.ee.stream.feed.GroundedItemFeed {
            private Saxon.Hej.om.SequenceIterator arg0iter;
            public InsertBeforeFeed3(Saxon.Hej.expr.SystemFunctionCall exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                int insertPosition = (int)(((Saxon.Hej.value.NumericValue)(call.getArg(1).evaluateItem(getContext()))).longValue());
                arg0iter = call.getArg(0).iterate(getContext());
                Saxon.Hej.@event.Outputter @out = getNextOutputter();
                for (int i = 1; (i<insertPosition); i++) {
                    Saxon.Hej.om.Item it = arg0iter.next();
                    if ((it == null)) {
                        arg0iter = null;
                        break ;
                    }
                    @out.append(it);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                getNextOutputter().append(item);
            }
            public override void close() {
                processItems(arg0iter, getNextOutputter());
                base.close();
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        
        }
    
    }
}
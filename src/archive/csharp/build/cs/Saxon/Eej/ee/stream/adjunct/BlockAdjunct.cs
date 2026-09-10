using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class BlockAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private int consumingArg = -1;
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return base.getWatchMaker(forGrouping);
        }
        private class BlockFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal int consumingArg;
            public BlockFeed(Saxon.Hej.expr.instruct.Block exp, int arg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
                this.consumingArg = arg;
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.instruct.Block block = (Saxon.Hej.expr.instruct.Block)(getExpression());
                Saxon.Hej.@event.Outputter result = getNextOutputter();
                for (int i = 0; (i<consumingArg); i++) {
                    Saxon.Hej.expr.Expression child = block.getOperanda()[i].getChildExpression();
                    Saxon.Hej.om.SequenceIterator iter = child.iterate(getContext());
                    processItems(iter, result);
                }
            }
            public override void close() {
                Saxon.Hej.expr.instruct.Block block = (Saxon.Hej.expr.instruct.Block)(getExpression());
                Saxon.Hej.@event.Outputter result = getNextOutputter();
                for (int i = (consumingArg+1); (i<block.getOperanda().Length); i++) {
                    Saxon.Hej.expr.Expression child = block.getOperanda()[i].getChildExpression();
                    Saxon.Hej.om.SequenceIterator iter = child.iterate(getContext());
                    processItems(iter, result);
                }
                result.close();
            }
            public override void append(Saxon.Hej.om.Item item) {
                getNextOutputter().append(item);
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
        
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return new Saxon.Eej.ee.stream.adjunct.BlockAdjunct.BlockFeed((Saxon.Hej.expr.instruct.Block)(getExpression()), consumingArg, @out, context);
        }
    
    }
}
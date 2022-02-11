using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class LetExpressionAdjunctB : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression action = ((Saxon.Hej.expr.LetExpression)(getExpression())).getAction();
            Saxon.Eej.ee.stream.Inversion ie = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(action, forGrouping);
            Saxon.Eej.ee.stream.Inversion route = ie.copy();
            Saxon.Eej.ee.stream.Inversion.PushAction letAction = new Saxon.Eej.ee.stream.Inversion.PushAction();
            letAction.feedMaker = new Saxon.Eej.ee.stream.feed.FeedMakerDelegate((watchManager, @out, context) => this.makeItemFeed(watchManager, @out, context));
            letAction.pushMethod = Saxon.Hej.expr.Expression.ITEM_FEED_METHOD;
            letAction.streamingChildSequence = 0;
            route.prepend(letAction);
            return (watchManager, @out, context) => route.getWatch(watchManager, @out, context);
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Inner_ItemFeed_13(this, getExpression(), @out, context);
        }
    
        private class Inner_ItemFeed_13 : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly LetExpressionAdjunctB outerInstance;
            public Inner_ItemFeed_13(LetExpressionAdjunctB outerInstance, /*AB*/Saxon.Hej.expr.Expression arg1, /*AA*/Saxon.Eej.ee.stream.feed.ItemFeed arg2, /*AA*/Saxon.Hej.expr.XPathContext arg3) : base(arg1, arg2, arg3) {
                this.outerInstance = outerInstance;
                context = /*superMethod*/getContext();
            }
            private readonly Saxon.Hej.expr.XPathContext context;
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(/*localparam*/terminator);
                try {
                    Saxon.Hej.om.Sequence value = ((Saxon.Hej.expr.LetExpression)(/*superMethod*/getExpression())).eval(/*field*/context);
                    /*field*/context.setLocalVariable(((Saxon.Hej.expr.LetExpression)(/*superMethod*/getExpression())).getLocalSlotNumber(), /*localvar*/value);
                } catch (Saxon.Hej.trans.XPathException e) {
                    /*superMethod*/dynamicError(/*exception*/e);
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if (!(/*superMethod*/hasFailed())) {
                    return /*superMethod*/getResultFeed().startSelectedParentNode(/*localparam*/node, /*localparam*/locationId);
                }
                return null;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(/*superMethod*/hasFailed())) {
                    /*superMethod*/getNextOutputter().append(/*localparam*/item);
                }
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                if (!(/*superMethod*/hasFailed())) {
                    /*superMethod*/getResultFeed().endSelectedParentNode(/*localparam*/locationId);
                }
            }
        }

    }
}
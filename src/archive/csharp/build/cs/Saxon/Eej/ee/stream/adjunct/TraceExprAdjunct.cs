using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class TraceExprAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : base.getFeedMaker(arg));
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.TraceExprAdjunct.TraceFeed(getExpression(), @out, context);
        }
        private class TraceFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal Saxon.Hej.trace.Traceable info;
            internal Saxon.Hej.lib.TraceListener listener;
            internal TraceFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.Expression expr = getExpression();
                if ((expr is Saxon.Hej.expr.instruct.TraceExpression)) {
                    info = ((Saxon.Hej.expr.instruct.TraceExpression)(expr)).getChild();
                } else {
                    if ((expr is Saxon.Hej.expr.instruct.ComponentTracer)) {
                        info = ((Saxon.Hej.expr.instruct.ComponentTracer)(expr)).getChild();
                    } else {
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                }
                if (getContext().getController().isTracing()) {
                    listener = getContext().getController().getTraceListener();
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((listener != null)) {
                    listener.enter(info, new System.Collections.Generic.Dictionary<string, System.Object>(), getContext());
                }
                getNextOutputter().append(item);
                if ((listener != null)) {
                    listener.leave(info);
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if ((listener != null)) {
                    listener.enter(info, new System.Collections.Generic.Dictionary<string, System.Object>(), getContext());
                }
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                if ((listener != null)) {
                    listener.leave(info);
                }
                getResultFeed().endSelectedParentNode(locationId);
            }
        
        }
    
    }
}
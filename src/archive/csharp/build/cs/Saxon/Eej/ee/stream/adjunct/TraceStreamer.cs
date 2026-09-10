using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class TraceStreamer : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : base.getFeedMaker(arg));
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.TraceStreamer.TraceFeed((Saxon.Hej.expr.SystemFunctionCall)(getExpression()), @out, context);
        }
        public static int sequenceNr = 0;
        private class TraceFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal string label;
            internal string seq = (("trace "+sequenceNr++)+": ");
            internal System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingParentNode> stack = new System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingParentNode>();
            public TraceFeed(Saxon.Hej.expr.SystemFunctionCall exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.Trace function = (Saxon.Hej.functions.Trace)(fnc.getTargetFunction());
                this.label = fnc.getArg(1).evaluateItem(getContext()).getStringValue();
                emit("open", null);
            }
            public override void append(Saxon.Hej.om.Item item) {
                emit("item", item);
                getNextOutputter().append(item);
            }
            public override void close() {
                emit("close", null);
                getNextOutputter().close();
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                    emit("startDocument", node);
                } else {
                    emit("startElement", node);
                }
                stack.Push(node);
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                Saxon.Eej.ee.stream.om.FleetingParentNode node = stack.Pop();
                if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                    emit("endDocument", node);
                } else {
                    emit("endElement", node);
                }
                getResultFeed().endSelectedParentNode(locationId);
            }
            private void emit(string phase, Saxon.Hej.om.Item val) {
                string message = (((seq+label)+": ")+phase);
                Saxon.Hej.Controller controller = getContext().getController();
                if (controller.isTracing()) {
                    Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                    Saxon.Hej.functions.Trace function = (Saxon.Hej.functions.Trace)(fnc.getTargetFunction());
                    function.notifyListener(message, val, getContext());
                } else {
                    Saxon.Hej.lib.Logger @out = controller.getTraceFunctionDestination();
                    if ((@out != null)) {
                        Saxon.Hej.functions.Trace.traceItem(val, message, @out);
                    }
                }
            }
        
        }
    
    }
}
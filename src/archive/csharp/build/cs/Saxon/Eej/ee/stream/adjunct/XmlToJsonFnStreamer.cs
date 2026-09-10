using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class XmlToJsonFnStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.SystemFunctionCall instruction = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.expr.Expression select = instruction.getArg(0);
            if ((select is Saxon.Hej.expr.CardinalityChecker)) {
                select = ((Saxon.Hej.expr.CardinalityChecker)(select)).getBaseExpression();
            }
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.adjunct.XmlToJsonFnStreamer.ToJsonAction action = new Saxon.Eej.ee.stream.adjunct.XmlToJsonFnStreamer.ToJsonAction(getExpression(), @out, context);
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                } ;
            }
            return null;
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.XmlToJsonFnStreamer.ToJsonAction(getExpression(), @out, context);
        }
        private class ToJsonAction : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.ma.json.JsonReceiver receiver;
            private bool indenting;
            private bool foundOneNode;
            public ToJsonAction(Saxon.Hej.expr.Expression functionCall, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (functionCall, @out, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.SystemFunctionCall exp = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.ma.json.XMLToJsonFn target = (Saxon.Hej.ma.json.XMLToJsonFn)(exp.getTargetFunction(getContext()));
                if ((exp.getArity()>1)) {
                    Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(exp.getArg(1).evaluateItem(getContext()));
                    System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = target.getDetails().optionDetails.processSuppliedOptions(suppliedOptions, getContext());
                    indenting = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "indent").head())).getBooleanValue();
                }
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if (foundOneNode) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Supplied argument to xml-to-json contains more than one node", "XPTY0004");
                    dynamicError(err);
                    return null;
                } else {
                    foundOneNode = true;
                    receiver = new Saxon.Hej.ma.json.JsonReceiver(getPipelineConfiguration(), getNextOutputter().getStringReceiver(false));
                    receiver.setIndenting(indenting);
                    receiver.open();
                    return receiver;
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                throw new Saxon.Ejava.lang.AssertionError();
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            }
            public override void close() {
                receiver.close();
                base.close();
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class LetExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.LetExpression)(getExpression())).getSequence();
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(select) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                Saxon.Eej.ee.stream.Inversion ie = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(select, forGrouping);
                Saxon.Eej.ee.stream.Inversion route = ie.copy();
                Saxon.Eej.ee.stream.Inversion.PushAction letAction = new Saxon.Eej.ee.stream.Inversion.PushAction();
                letAction.feedMaker = new Saxon.Eej.ee.stream.adjunct.LetExpressionAdjunct.FeedMakerForConsumingSelect(((Saxon.Hej.expr.LetExpression)(getExpression())));
                letAction.pushMethod = Saxon.Hej.expr.Expression.ITEM_FEED_METHOD;
                letAction.streamingChildSequence = 0;
                route.prepend(letAction);
                return route.getWatch /* instance method reference */;
            } else {
                Saxon.Eej.ee.stream.adjunct.StreamingAdjunct sa = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), getExpression());
                return sa.getWatchMaker(forGrouping);
            }
        }
        private class FeedMakerForConsumingSelect : Saxon.Eej.ee.stream.feed.FeedMaker {
            public readonly Saxon.Hej.expr.LetExpression letExpr;
            public FeedMakerForConsumingSelect(Saxon.Hej.expr.LetExpression letExpr)  {
                this.letExpr = letExpr;
            }
            public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
                return new Saxon.Eej.ee.stream.adjunct.LetExpressionAdjunct.ItemFeedForConsumingSelect(letExpr, @out, context);
            }
        
        }
        private class ItemFeedForConsumingSelect : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.@event.SequenceCollector collector;
            private Saxon.Hej.@event.ComplexContentOutputter cco;
            public ItemFeedForConsumingSelect(Saxon.Hej.expr.Expression letExpr, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (letExpr, @out, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                collector = new Saxon.Hej.@event.SequenceCollector(getResultFeed().getPipelineConfiguration());
                cco = new Saxon.Hej.@event.ComplexContentOutputter(collector);
            }
            public override void startDocument(int properties) {
                cco.startDocument(properties);
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
                cco.startElement(elemName, typeCode, location, properties);
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                cco.startElement(elemName, type, attributes, namespaces, location, properties);
            }
            public override void endDocument() {
                cco.endDocument();
            }
            public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
                cco.@namespace(prefix, namespaceUri, properties);
            }
            public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
                cco.attribute(attName, typeCode, value, location, properties);
            }
            public override void endElement() {
                cco.endElement();
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
                cco.characters(chars, location, properties);
            }
            public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
                cco.processingInstruction(name, data, location, properties);
            }
            public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
                cco.comment(content, location, properties);
            }
            public override void append(Saxon.Hej.om.Item item) {
                cco.append(item);
            }
            public override void close() {
                Saxon.Hej.expr.LetExpression letExpr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.om.GroundedValue value = collector.getSequence();
                Saxon.Hej.expr.XPathContext context = getContext();
                context.setLocalVariable(letExpr.getLocalSlotNumber(), value);
                Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/letExpr.getAction().makeElaborator().elaborateForPush()(getResultFeed(), context);
                Saxon.Hej.expr.Expression.dispatchTailCall(tc);
                base.close();
            }
        
        }
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
            if (((expr.getSequence() is Saxon.Hej.expr.ContextItemExpression) && (contextInfo.getContextItemPosture() != Saxon.Eej.ee.stream.Posture.GROUNDED))) {
                if (!(hasNonInspectionUsage(expr, expr.getAction()))) {
                    expr.getSequenceOp().setUsage(Saxon.Hej.expr.OperandUsage.INSPECTION);
                }
            }
            return base.computeStreamability(contextInfo, reasons);
        }
        private bool hasNonInspectionUsage(Saxon.Hej.expr.LetExpression binding, Saxon.Hej.expr.Expression child) {
            foreach (Saxon.Hej.expr.Operand o in child.operands()) {
                if ((o.getChildExpression() is Saxon.Hej.expr.VariableReference)) {
                    if (((((Saxon.Hej.expr.VariableReference)(o.getChildExpression())).getBinding() == binding) && (o.getUsage() != Saxon.Hej.expr.OperandUsage.INSPECTION))) {
                        return true;
                    } else {
                        if (hasNonInspectionUsage(binding, o.getChildExpression())) {
                            return true;
                        }
                    }
                } else {
                    if (hasNonInspectionUsage(binding, o.getChildExpression())) {
                        return true;
                    }
                }
            }
            return false;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ConditionalBlockAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private int consumingArg = -1;
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return base.getWatchMaker(forGrouping);
        }
        public class ConditionalBlockFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private readonly int consumingArg;
            private bool empty = true;
            private int level = 0;
            private bool watchForDocumentChildren;
            private readonly bool consumingArgIsOnEmpty;
            private readonly System.Collections.Generic.IList<Saxon.Hej.expr.instruct.OnNonEmptyExpr> nonEmptyPendingList = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.OnNonEmptyExpr>();
            private readonly System.Collections.Generic.IList<Saxon.Hej.@event.OutputterEvent> tentativeContent = new System.Collections.Generic.List<Saxon.Hej.@event.OutputterEvent>();
            internal ConditionalBlockFeed(Saxon.Hej.expr.instruct.ConditionalBlock exp, int arg, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
                this.consumingArg = arg;
                consumingArgIsOnEmpty = (exp.getChildExpression(arg) is Saxon.Hej.expr.instruct.OnEmptyExpr);
            }
            public System.Collections.Generic.IList<Saxon.Hej.@event.OutputterEvent> getTentativeContent() {
                return tentativeContent;
            }
            public void flushIfNonEmpty() {
                if ((!(empty) && !(tentativeContent.Count==0))) {
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    foreach (Saxon.Hej.@event.OutputterEvent @event in tentativeContent) {
                        @event.replay(@out);
                    }
                    tentativeContent.Clear();
                }
            }
            private void action() {
                if (((level == 0) || (((level == 1) && watchForDocumentChildren)))) {
                    empty = false;
                }
                flushIfNonEmpty();
            }
            public bool isEmpty() {
                return empty;
            }
            private bool isSignificant(Saxon.Hej.om.Item item) {
                return Saxon.Hej.@event.SignificantItemDetector.isSignificant(item);
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.instruct.ConditionalBlock block = (Saxon.Hej.expr.instruct.ConditionalBlock)(getExpression());
                Saxon.Hej.@event.Outputter result = getNextOutputter();
                for (int i = 0; (i<consumingArg); i++) {
                    Saxon.Hej.expr.Expression child = block.getChildExpression(i);
                    if ((child is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                    } else {
                        if ((child is Saxon.Hej.expr.instruct.OnNonEmptyExpr)) {
                            if (empty) {
                                Saxon.Hej.@event.OutputterEventBuffer temp = new Saxon.Hej.@event.OutputterEventBuffer();
                                temp.setBuffer(tentativeContent);
                                Saxon.Hej.om.SequenceIterator iter = child.iterate(getContext());
                                processItems(iter, temp);
                            } else {
                                Saxon.Hej.om.SequenceIterator iter = child.iterate(getContext());
                                processItems(iter, result);
                            }
                        } else {
                            Saxon.Hej.om.SequenceTool.supply(child.iterate(getContext()), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                                if ((empty && isSignificant(item))) {
                                    empty = false;
                                    flushIfNonEmpty();
                                }
                                result.append(item);
                            } )));
                        }
                    }
                }
            }
            public override void close() {
                if (consumingArgIsOnEmpty) {
                    empty = true;
                }
                Saxon.Hej.expr.instruct.ConditionalBlock block = (Saxon.Hej.expr.instruct.ConditionalBlock)(getExpression());
                Saxon.Hej.@event.Outputter result = getNextOutputter();
                for (int i = (consumingArg+1); (i<block.size()); i++) {
                    Saxon.Hej.expr.Expression child = block.getChildExpression(i);
                    if ((child is Saxon.Hej.expr.instruct.OnEmptyExpr)) {
                        if (empty) {
                            Saxon.Hej.om.SequenceIterator si = child.iterate(getContext());
                            processItems(si, result);
                        }
                    } else {
                        if ((child is Saxon.Hej.expr.instruct.OnNonEmptyExpr)) {
                            if (empty) {
                                if ((i != (block.size()-1))) {
                                    Saxon.Hej.@event.OutputterEventBuffer temp = new Saxon.Hej.@event.OutputterEventBuffer();
                                    temp.setBuffer(tentativeContent);
                                    Saxon.Hej.om.SequenceIterator iter = child.iterate(getContext());
                                    processItems(iter, temp);
                                }
                            } else {
                                processItems(child.iterate(getContext()), result);
                            }
                        } else {
                            Saxon.Hej.om.SequenceTool.supply(child.iterate(getContext()), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => {
                                if (isSignificant(item)) {
                                    empty = false;
                                    flushIfNonEmpty();
                                }
                                result.append(item);
                            } )));
                        }
                    }
                }
                result.close();
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if (empty) {
                    empty = false;
                    foreach (Saxon.Hej.expr.instruct.OnNonEmptyExpr e in nonEmptyPendingList) {
                        Saxon.Hej.om.SequenceIterator si = e.iterate(getContext());
                        processItems(si, getNextOutputter());
                    }
                }
                return getResultFeed().startSelectedParentNode(node, locationId);
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                getResultFeed().endSelectedParentNode(locationId);
            }
            public override void startDocument(int properties) {
                if ((level == 0)) {
                    watchForDocumentChildren = true;
                }
                level++;
                getNextOutputter().startDocument(properties);
            }
            public override void endDocument() {
                level--;
                watchForDocumentChildren = false;
                getNextOutputter().endDocument();
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
                action();
                level++;
                getNextOutputter().startElement(elemName, typeCode, location, properties);
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                action();
                level++;
                getNextOutputter().startElement(elemName, type, attributes, namespaces, location, properties);
            }
            public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
                action();
                getNextOutputter().@namespace(prefix, namespaceUri, properties);
            }
            public override void endElement() {
                level--;
                getNextOutputter().endElement();
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
                if (((Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.SEPARATOR) && isEmpty()) && chars.ToString().Equals(" "))) {
                    tentativeContent.Add(new Saxon.Hej.@event.OutputterEvent.Text(Saxon.Hej.str.StringConstants.SINGLE_SPACE, location, properties));
                } else {
                    action();
                    getNextOutputter().characters(chars, location, properties);
                }
            }
            public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
                action();
                getNextOutputter().comment(content, location, properties);
            }
            public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
                action();
                getNextOutputter().attribute(attName, typeCode, value, location, properties);
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (((item is Saxon.Hej.value.StringValue) && ((Saxon.Hej.value.StringValue)(item)).isEmpty())) {
                    if (isEmpty()) {
                        tentativeContent.Add(new Saxon.Hej.@event.OutputterEvent.Append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
                    } else {
                        getNextOutputter().append(item);
                    }
                } else {
                    action();
                    getNextOutputter().append(item);
                }
            }
        
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return new Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed((Saxon.Hej.expr.instruct.ConditionalBlock)(getExpression()), consumingArg, @out, context);
        }
    
    }
}
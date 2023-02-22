using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class OnEmptyAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.OnEmptyAdjunct.OnEmptyFeed(getExpression(), (Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed)(@out), context);
        }
        private class OnEmptyFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal bool isEmpty;
            internal OnEmptyFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed next, Saxon.Hej.expr.XPathContext context) : base (expr, next, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                isEmpty = ((Saxon.Eej.ee.stream.adjunct.ConditionalBlockAdjunct.ConditionalBlockFeed)(getNextOutputter())).isEmpty();
            }
            public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
                if (isEmpty) {
                    return base.startSelectedParentNode(node, locationId);
                } else {
                    return null;
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (isEmpty) {
                    getNextOutputter().append(item);
                }
            }
            public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
                if (isEmpty) {
                    base.endSelectedParentNode(locationId);
                }
            }
            public override void close() {
                base.close();
            }
            public override void startDocument(int properties) {
                if (isEmpty) {
                    base.startDocument(properties);
                }
            }
            public override void endDocument() {
                if (isEmpty) {
                    base.endDocument();
                }
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.startElement(elemName, typeCode, location, properties);
                }
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.startElement(elemName, type, attributes, namespaces, location, properties);
                }
            }
            public override void @namespace(string prefix, string namespaceUri, int properties) {
                if (isEmpty) {
                    base.@namespace(prefix, namespaceUri, properties);
                }
            }
            public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.attribute(attName, typeCode, value, location, properties);
                }
            }
            public override void startContent() {
                if (isEmpty) {
                    base.startContent();
                }
            }
            public override void endElement() {
                if (isEmpty) {
                    base.endElement();
                }
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.characters(chars, location, properties);
                }
            }
            public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.processingInstruction(name, data, location, properties);
                }
            }
            public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
                if (isEmpty) {
                    base.comment(content, location, properties);
                }
            }
        
        }
    
    }
}
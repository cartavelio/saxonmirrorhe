using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class TypedValueFeed : Saxon.Eej.ee.stream.feed.AbsorptionFeed {
        public TypedValueFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
        }
        protected internal override Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
            return new Saxon.Eej.ee.stream.feed.TypedValueFeed.PendingTypedValue(sequenceNr, node);
        }
        protected internal override Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.value.AtomicValue)) {
                return item;
            } else {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    return item.atomize();
                } else {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        return item.atomize();
                    } else {
                        dynamicError(new Saxon.Hej.trans.XPathException(("Cannot atomize item of type "+Saxon.Hej.type.Type.displayTypeName(item)), "FOTY0013"));
                        return null;
                    }
                }
            }
        }
        private int depth = 0;
        private Saxon.Ejava.lang.StringBuilder buffer;
        public override void startDocument(int properties) {
            if ((depth == 0)) {
                buffer = new Saxon.Ejava.lang.StringBuilder(256);
            }
            depth++;
        }
        public override void endDocument() {
            depth--;
            if ((depth == 0)) {
                append(new Saxon.Hej.value.StringValue(buffer.ToString()));
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth == 0)) {
                buffer = new Saxon.Ejava.lang.StringBuilder(256);
            }
            depth++;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            startElement(elemName, type, location, properties);
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            if ((depth == 0)) {
                characters(namespaceUri.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, properties);
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth == 0)) {
                characters(Saxon.Hej.str.StringView.of(value), location, properties);
            }
        }
        public override void startContent() {
        }
        public override void endElement() {
            depth--;
            if ((depth == 0)) {
                append(new Saxon.Hej.value.StringValue(buffer.ToString()));
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth == 0)) {
                append(new Saxon.Hej.value.StringValue(chars.tidy()));
            } else {
                buffer.append(chars);
            }
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth == 0)) {
                characters(data, location, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            if ((depth == 0)) {
                characters(content, Saxon.Hej.expr.parser.Loc.NONE, properties);
            }
        }
        private class PendingTypedValue : Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem {
            public Saxon.Hej.str.UnicodeBuilder buffer;
            public Saxon.Eej.ee.stream.om.FleetingParentNode node;
            public PendingTypedValue(int sequence, Saxon.Eej.ee.stream.om.FleetingParentNode node) : base (sequence) {
                buffer = new Saxon.Hej.str.UnicodeBuilder();
                this.node = node;
            }
            public override Saxon.Hej.@event.Receiver getGatherer() {
                return new Saxon.Eej.ee.stream.feed.StringValueGatherer(buffer);
            }
            public override Saxon.Hej.om.Sequence deliver() {
                node.setStringValue(buffer.toUnicodeString());
                return node.atomize();
            }
            public override void rollback() {
                buffer.clear();
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class ExistenceFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private int level = 0;
        protected internal ExistenceFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
        }
        public abstract void action();
        public override void startDocument(int properties) {
            if ((level == 0)) {
                action();
            }
            level++;
        }
        public override void endDocument() {
            level--;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                action();
            }
            level++;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                action();
            }
            level++;
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            if ((level == 0)) {
                action();
            }
        }
        public override void endElement() {
            level--;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                action();
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                action();
            }
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            if ((level == 0)) {
                action();
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            if ((level == 0)) {
                action();
            }
        }
        public abstract override void close();
    
    }
}
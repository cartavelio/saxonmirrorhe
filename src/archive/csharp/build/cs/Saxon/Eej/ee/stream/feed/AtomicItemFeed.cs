using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class AtomicItemFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        public AtomicItemFeed(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
        }
        public AtomicItemFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (exp, result, context) {
        }
        private void notAllowed() {
            throw new Saxon.Ejava.lang.IllegalStateException(("Nodes cannot be passed to an instance of "+this.GetType().Name));
        }
        public override void startDocument(int properties) {
            notAllowed();
        }
        public override void endDocument() {
            notAllowed();
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            notAllowed();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            notAllowed();
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
        public override void startContent() {
            notAllowed();
        }
        public override void endElement() {
            notAllowed();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            notAllowed();
        }
    
    }
}
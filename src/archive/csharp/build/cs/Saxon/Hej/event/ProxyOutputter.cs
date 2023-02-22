using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class ProxyOutputter : Saxon.Hej.@event.Outputter {
        private readonly Saxon.Hej.@event.Outputter next;
        public ProxyOutputter(Saxon.Hej.@event.Outputter next)  {
            this.next = next;
            setPipelineConfiguration(next.getPipelineConfiguration());
            setSystemId(next.getSystemId());
        }
        public Saxon.Hej.@event.Outputter getNextOutputter() {
            return next;
        }
        public override void open() {
            next.open();
        }
        public override void startDocument(int properties) {
            next.startDocument(properties);
        }
        public override void endDocument() {
            next.endDocument();
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            next.setUnparsedEntity(name, systemID, publicID);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            next.startElement(elemName, typeCode, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            next.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void @namespace(string prefix, Saxon.Hej.om.NamespaceUri namespaceUri, int properties) {
            next.@namespace(prefix, namespaceUri, properties);
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            next.attribute(attName, typeCode, value, location, properties);
        }
        public override void startContent() {
            next.startContent();
        }
        public override void endElement() {
            next.endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            next.characters(chars, location, properties);
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            next.processingInstruction(name, data, location, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            next.comment(content, location, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            next.append(item, locationId, properties);
        }
        public override void append(Saxon.Hej.om.Item item) {
            next.append(item);
        }
        public override void close() {
            next.close();
        }
        public override bool usesTypeAnnotations() {
            return next.usesTypeAnnotations();
        }
    
    }
}
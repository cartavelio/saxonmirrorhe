using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class TeeOutputter : Saxon.Hej.@event.SequenceReceiver {
        private Saxon.Hej.@event.Receiver seq1;
        private Saxon.Hej.@event.Receiver seq2;
        public TeeOutputter(Saxon.Hej.@event.Receiver seq1, Saxon.Hej.@event.Receiver seq2) : base (seq1.getPipelineConfiguration()) {
            this.seq1 = seq1;
            this.seq2 = seq2;
        }
        protected internal void setFirstDestination(Saxon.Hej.@event.Receiver seq1) {
            this.seq1 = seq1;
        }
        protected internal void setSecondDestination(Saxon.Hej.@event.Receiver seq2) {
            this.seq2 = seq2;
        }
        protected internal Saxon.Hej.@event.Receiver getFirstDestination() {
            return seq1;
        }
        protected internal Saxon.Hej.@event.Receiver getSecondDestination() {
            return seq2;
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            seq1.setUnparsedEntity(name, systemID, publicID);
            seq2.setUnparsedEntity(name, systemID, publicID);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            seq1.append(item, locationId, properties);
            seq2.append(item, locationId, properties);
        }
        public override void open() {
            base.open();
            seq1.open();
            seq2.open();
        }
        public override void startDocument(int properties) {
            seq1.startDocument(properties);
            seq2.startDocument(properties);
        }
        public override void endDocument() {
            seq1.endDocument();
            seq2.endDocument();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            seq1.startElement(elemName, type, attributes, namespaces, location, properties);
            seq2.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            seq1.endElement();
            seq2.endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            seq1.characters(chars, locationId, properties);
            seq2.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            seq1.processingInstruction(name, data, locationId, properties);
            seq2.processingInstruction(name, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location locationId, int properties) {
            seq1.comment(content, locationId, properties);
            seq2.comment(content, locationId, properties);
        }
        public override void close() {
            seq1.close();
            seq2.close();
        }
        public override bool usesTypeAnnotations() {
            return (seq1.usesTypeAnnotations() || seq2.usesTypeAnnotations());
        }
    
    }
}
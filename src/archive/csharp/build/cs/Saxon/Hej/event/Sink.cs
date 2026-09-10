using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class Sink : Saxon.Hej.@event.SequenceReceiver {
        public Sink(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
        }
        public override void open() {
        }
        public override void close() {
        }
        public override void startDocument(int properties) {
        }
        public override void endDocument() {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
        }
        public override void endElement() {
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
    
    }
}
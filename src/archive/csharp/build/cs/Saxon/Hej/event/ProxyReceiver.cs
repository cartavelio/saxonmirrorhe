using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class ProxyReceiver : Saxon.Hej.@event.SequenceReceiver {
        protected internal Saxon.Hej.@event.Receiver nextReceiver;
        public ProxyReceiver(Saxon.Hej.@event.Receiver nextReceiver) : base (nextReceiver.getPipelineConfiguration()) {
            setUnderlyingReceiver(nextReceiver);
            setPipelineConfiguration(nextReceiver.getPipelineConfiguration());
        }
        public override void setSystemId(string systemId) {
            if ((systemId != this.systemId)) {
                this.systemId = systemId;
                nextReceiver.setSystemId(systemId);
            }
        }
        public void setUnderlyingReceiver(Saxon.Hej.@event.Receiver receiver) {
            nextReceiver = receiver;
        }
        public Saxon.Hej.@event.Receiver getNextReceiver() {
            return nextReceiver;
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            if ((pipelineConfiguration != pipe)) {
                pipelineConfiguration = pipe;
                if ((nextReceiver.getPipelineConfiguration() != pipe)) {
                    nextReceiver.setPipelineConfiguration(pipe);
                }
            }
        }
        public override Saxon.Hej.om.NamePool getNamePool() {
            return pipelineConfiguration.getConfiguration().getNamePool();
        }
        public override void open() {
            nextReceiver.open();
        }
        public override void close() {
            nextReceiver.close();
        }
        public override void startDocument(int properties) {
            nextReceiver.startDocument(properties);
        }
        public override void endDocument() {
            nextReceiver.endDocument();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            nextReceiver.endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.comment(chars, locationId, properties);
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            nextReceiver.setUnparsedEntity(name, uri, publicId);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.append(item, locationId, properties);
        }
        public override bool usesTypeAnnotations() {
            return nextReceiver.usesTypeAnnotations();
        }
    
    }
}
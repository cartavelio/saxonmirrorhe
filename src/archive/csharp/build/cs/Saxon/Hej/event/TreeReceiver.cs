using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class TreeReceiver : Saxon.Hej.@event.SequenceReceiver {
        private readonly Saxon.Hej.@event.Receiver nextReceiver;
        private int level = 0;
        private bool[] isDocumentLevel = new bool[20];
        public TreeReceiver(Saxon.Hej.@event.Receiver nextInChain) : base (nextInChain.getPipelineConfiguration()) {
            nextReceiver = nextInChain;
            previousAtomic = false;
            setPipelineConfiguration(nextInChain.getPipelineConfiguration());
        }
        public override void setSystemId(string systemId) {
            if (((systemId != null) && !(systemId.Equals(this.systemId)))) {
                this.systemId = systemId;
                if ((nextReceiver != null)) {
                    nextReceiver.setSystemId(systemId);
                }
            }
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            if ((pipelineConfiguration != pipe)) {
                pipelineConfiguration = pipe;
                if ((nextReceiver != null)) {
                    nextReceiver.setPipelineConfiguration(pipe);
                }
            }
        }
        public Saxon.Hej.@event.Receiver getNextReceiver() {
            return nextReceiver;
        }
        public override void open() {
            if ((nextReceiver == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("TreeReceiver.open(): no underlying receiver provided");
            }
            nextReceiver.open();
            previousAtomic = false;
        }
        public override void close() {
            if ((nextReceiver != null)) {
                nextReceiver.close();
            }
            previousAtomic = false;
        }
        public override void startDocument(int properties) {
            if ((level == 0)) {
                nextReceiver.startDocument(properties);
            }
            if (((isDocumentLevel.Length-1)<level)) {
                isDocumentLevel = Saxon.Ejava.util.Arrays.copyOf(isDocumentLevel, (level*2));
            }
            isDocumentLevel[level++] = true;
        }
        public override void endDocument() {
            level--;
            if ((level == 0)) {
                nextReceiver.endDocument();
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            previousAtomic = false;
            if (((isDocumentLevel.Length-1)<level)) {
                isDocumentLevel = Saxon.Ejava.util.Arrays.copyOf(isDocumentLevel, (level*2));
            }
            isDocumentLevel[level++] = false;
        }
        public override void endElement() {
            nextReceiver.endElement();
            previousAtomic = false;
            level--;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(chars.isEmpty())) {
                nextReceiver.characters(chars, locationId, properties);
            }
            previousAtomic = false;
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.processingInstruction(target, data, locationId, properties);
            previousAtomic = false;
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            nextReceiver.comment(chars, locationId, properties);
            previousAtomic = false;
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            nextReceiver.setUnparsedEntity(name, uri, publicId);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            decompose(item, locationId, copyNamespaces);
        }
        public override bool usesTypeAnnotations() {
            return nextReceiver.usesTypeAnnotations();
        }
    
    }
}
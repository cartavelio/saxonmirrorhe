using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class IDFilter : Saxon.Hej.@event.ProxyReceiver {
        private readonly string requiredId;
        private int activeDepth = 0;
        private bool matched = false;
        private System.Collections.Generic.HashSet<Saxon.Hej.type.SimpleType> nonIDs;
        public IDFilter(Saxon.Hej.@event.Receiver next, string id) : base (next) {
            this.requiredId = id;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            matched = false;
            if ((activeDepth == 0)) {
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    if (((att.getNodeName().Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME)) || Saxon.Hej.@event.ReceiverOption.contains(att.getProperties(), Saxon.Hej.@event.ReceiverOption.IS_ID))) {
                        if (att.getValue().Equals(requiredId)) {
                            matched = true;
                        }
                    }
                }
                if (matched) {
                    activeDepth = 1;
                    base.startElement(elemName, type, attributes, namespaces, location, properties);
                }
            } else {
                activeDepth++;
                base.startElement(elemName, type, attributes, namespaces, location, properties);
            }
        }
        public override void endElement() {
            if ((activeDepth>0)) {
                nextReceiver.endElement();
                activeDepth--;
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((activeDepth>0)) {
                base.characters(chars, locationId, properties);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((activeDepth>0)) {
                base.processingInstruction(target, data, locationId, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((activeDepth>0)) {
                base.comment(chars, locationId, properties);
            }
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
    
    }
}
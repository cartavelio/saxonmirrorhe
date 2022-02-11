using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class OuterNewlineInserter : Saxon.Hej.@event.ProxyReceiver {
        private int level = 0;
        private bool beforeFirstElement = true;
        private static readonly Saxon.Hej.str.UnicodeString NEWLINE = Saxon.Hej.str.BMPString.of("\n");
        public OuterNewlineInserter(Saxon.Hej.serialize.XMLEmitter next) : base (next) {
        }
        public override void open() {
            nextReceiver.open();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            level++;
            beforeFirstElement = false;
        }
        public override void endElement() {
            level--;
            nextReceiver.endElement();
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((level == 0) && !(beforeFirstElement))) {
                characters(NEWLINE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            nextReceiver.processingInstruction(target, data, locationId, properties);
            if (((level == 0) && beforeFirstElement)) {
                characters(NEWLINE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((level == 0) && !(beforeFirstElement))) {
                characters(NEWLINE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            nextReceiver.comment(chars, locationId, properties);
            if (((level == 0) && beforeFirstElement)) {
                characters(NEWLINE, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
    
    }
}
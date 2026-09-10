using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class ContentDetector : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.@event.Event.StartElement pendingEvent = null;
        public ContentDetector(Saxon.Eej.ee.stream.watch.WatchManager nextReceiver) : base (nextReceiver) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            flush(true);
            pendingEvent = new Saxon.Hej.@event.Event.StartElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            flush(false);
            nextReceiver.endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            flush(true);
            nextReceiver.characters(chars, locationId, properties);
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            flush(true);
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            flush(true);
            nextReceiver.comment(chars, locationId, properties);
        }
        private void flush(bool nonEmpty) {
            if ((pendingEvent != null)) {
                int properties = pendingEvent.getProperties();
                if (nonEmpty) {
                    properties |= Saxon.Hej.@event.ReceiverOption.HAS_CHILDREN;
                }
                pendingEvent.replay(nextReceiver, properties);
                pendingEvent = null;
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class CommentStripper : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.str.UnicodeString currentTextNode = null;
        public CommentStripper(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            flush();
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void endElement() {
            flush();
            nextReceiver.endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((currentTextNode == null)) {
                currentTextNode = chars;
            } else {
                currentTextNode = currentTextNode.concat(chars);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        private void flush() {
            if ((currentTextNode != null)) {
                nextReceiver.characters(currentTextNode, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            }
            currentTextNode = null;
        }
    
    }
}
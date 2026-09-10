using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class CommentStripper : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.str.CompressedWhitespace savedWhitespace = null;
        private readonly Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
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
            if ((chars is Saxon.Hej.str.CompressedWhitespace)) {
                if ((buffer.isEmpty() && (savedWhitespace == null))) {
                    savedWhitespace = (Saxon.Hej.str.CompressedWhitespace)(chars);
                } else {
                    buffer.accept(((Saxon.Hej.str.CompressedWhitespace)(chars)).uncompress());
                }
            } else {
                if ((savedWhitespace != null)) {
                    buffer.accept(savedWhitespace.uncompress());
                    savedWhitespace = null;
                }
                buffer.accept(chars);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        private void flush() {
            if (!(buffer.isEmpty())) {
                nextReceiver.characters(buffer.toUnicodeString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
            } else {
                if ((savedWhitespace != null)) {
                    nextReceiver.characters(savedWhitespace, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
            savedWhitespace = null;
            buffer.clear();
        }
    
    }
}
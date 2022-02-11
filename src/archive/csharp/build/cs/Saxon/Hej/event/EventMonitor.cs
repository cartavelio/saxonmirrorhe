using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class EventMonitor : Saxon.Hej.@event.Outputter {
        private bool written = false;
        private readonly Saxon.Hej.@event.Outputter next;
        public EventMonitor(Saxon.Hej.@event.Outputter next)  {
            this.next = next;
        }
        public override void startDocument(int properties) {
            written = true;
            next.startDocument(properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.startElement(elemName, type, location, properties);
        }
        public override void endElement() {
            written = true;
            next.endElement();
        }
        public override void attribute(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.attribute(name, type, value, location, properties);
        }
        public override void @namespace(string prefix, string uri, int properties) {
            written = true;
            next.@namespace(prefix, uri, properties);
        }
        public override void startContent() {
            written = true;
            next.startContent();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.characters(chars, location, properties);
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.processingInstruction(name, data, location, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.comment(content, location, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location location, int properties) {
            written = true;
            next.append(item, location, properties);
        }
        public override void endDocument() {
            written = true;
            next.endDocument();
        }
        public bool hasBeenWrittenTo() {
            return written;
        }
    
    }
}
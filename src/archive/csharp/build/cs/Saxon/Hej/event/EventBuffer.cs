using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class EventBuffer : Saxon.Hej.@event.SequenceReceiver {
        private readonly System.Collections.Generic.IList<Saxon.Hej.@event.Event> buffer = new System.Collections.Generic.List<Saxon.Hej.@event.Event>();
        public EventBuffer(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
        }
        public override void startDocument(int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.StartDocument(properties));
        }
        public override void endDocument() {
            buffer.Add(new Saxon.Hej.@event.Event.EndDocument());
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.StartElement(elemName, typeCode, attributes, namespaces, location, properties));
        }
        public override void endElement() {
            buffer.Add(new Saxon.Hej.@event.Event.EndElement());
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.Text(chars, location, properties));
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.ProcessingInstruction(name, data, location, properties));
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.Comment(content, location, properties));
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.Event.Append(item, location, properties));
        }
        public override void close() {
        }
        public void replay(Saxon.Hej.@event.Receiver @out) {
            foreach (Saxon.Hej.@event.Event @event in buffer) {
                @event.replay(@out);
            }
        }
    
    }
}
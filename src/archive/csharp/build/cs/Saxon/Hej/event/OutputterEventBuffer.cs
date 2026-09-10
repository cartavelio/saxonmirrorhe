using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class OutputterEventBuffer : Saxon.Hej.@event.Outputter {
        private System.Collections.Generic.IList<Saxon.Hej.@event.OutputterEvent> buffer = new System.Collections.Generic.List<Saxon.Hej.@event.OutputterEvent>();
        public OutputterEventBuffer()  {
        }
        public void setBuffer(System.Collections.Generic.IList<Saxon.Hej.@event.OutputterEvent> buffer) {
            this.buffer = buffer;
        }
        public override void startDocument(int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.StartDocument(properties));
        }
        public override void endDocument() {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.EndDocument());
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.StartElement(elemName, typeCode, location, properties));
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.StartElement(elemName, type, location, properties));
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                buffer.Add(new Saxon.Hej.@event.OutputterEvent.Attribute(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties()));
            }
            foreach (Saxon.Hej.om.NamespaceBinding binding in namespaces) {
                buffer.Add(new Saxon.Hej.@event.OutputterEvent.Namespace(binding.getPrefix(), binding.getURI(), properties));
            }
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.StartContent());
        }
        public override void attribute(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.Attribute(name, type, value.ToString(), location, properties));
        }
        public override void @namespace(string prefix, string uri, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.Namespace(prefix, uri, properties));
        }
        public override void startContent() {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.StartContent());
        }
        public override void endElement() {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.EndElement());
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.Text(chars, location, properties));
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.ProcessingInstruction(name, data, location, properties));
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.Comment(content, location, properties));
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location location, int properties) {
            buffer.Add(new Saxon.Hej.@event.OutputterEvent.Append(item, location, properties));
        }
        public override void close() {
        }
        public void replay(Saxon.Hej.@event.Outputter @out) {
            foreach (Saxon.Hej.@event.OutputterEvent @event in buffer) {
                @event.replay(@out);
            }
        }
        public bool isEmpty() {
            return buffer.Count==0;
        }
        public void reset() {
            buffer.Clear();
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class Event {
        public virtual void replay(Saxon.Hej.@event.Receiver @out) {
        }
        public class StartDocument : Saxon.Hej.@event.Event {
            internal int properties;
            public StartDocument(int properties)  {
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.startDocument(properties);
            }
        
        }
        public class EndDocument : Saxon.Hej.@event.Event {
            public EndDocument()  {
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.endDocument();
            }
        
        }
        public class StartElement : Saxon.Hej.@event.Event {
            internal Saxon.Hej.om.NodeName name;
            internal Saxon.Hej.type.SchemaType type;
            internal Saxon.Hej.om.AttributeMap attributes;
            internal Saxon.Hej.om.NamespaceMap namespaces;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public StartElement(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties)  {
                this.name = name;
                this.type = type;
                this.attributes = attributes;
                this.namespaces = namespaces;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.startElement(name, type, attributes, namespaces, location, properties);
            }
            public void replay(Saxon.Hej.@event.Receiver @out, int newProps) {
                @out.startElement(name, type, attributes, namespaces, location, newProps);
            }
            public int getProperties() {
                return properties;
            }
        
        }
        public class EndElement : Saxon.Hej.@event.Event {
            public EndElement()  {
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.endElement();
            }
        
        }
        public class Text : Saxon.Hej.@event.Event {
            internal Saxon.Hej.str.UnicodeString content;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Text(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties)  {
                this.content = content;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.characters(content, location, properties);
            }
        
        }
        public class Comment : Saxon.Hej.@event.Event {
            internal Saxon.Hej.str.UnicodeString content;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties)  {
                this.content = content;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.comment(content, location, properties);
            }
        
        }
        public class ProcessingInstruction : Saxon.Hej.@event.Event {
            internal string target;
            internal Saxon.Hej.str.UnicodeString content;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public ProcessingInstruction(string target, Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties)  {
                this.target = target;
                this.content = content;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.processingInstruction(target, content, location, properties);
            }
        
        }
        public class Append : Saxon.Hej.@event.Event {
            internal Saxon.Hej.om.Item item;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location location, int properties)  {
                this.item = item;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Receiver @out) {
                @out.append(item, location, properties);
            }
        
        }
    
    }
}
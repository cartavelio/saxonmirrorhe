using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class OutputterEvent {
        public virtual void replay(Saxon.Hej.@event.Outputter @out) {
        }
        public class StartDocument : Saxon.Hej.@event.OutputterEvent {
            internal int properties;
            public StartDocument(int properties)  {
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.startDocument(properties);
            }
        
        }
        public class EndDocument : Saxon.Hej.@event.OutputterEvent {
            public EndDocument()  {
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.endDocument();
            }
        
        }
        public class StartElement : Saxon.Hej.@event.OutputterEvent {
            internal Saxon.Hej.om.NodeName name;
            internal Saxon.Hej.type.SchemaType type;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public StartElement(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type, Saxon.Hej.s9api.Location location, int properties)  {
                this.name = name;
                this.type = type;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.startElement(name, type, location, properties);
            }
        
        }
        public class Attribute : Saxon.Hej.@event.OutputterEvent {
            internal Saxon.Hej.om.NodeName name;
            internal Saxon.Hej.type.SimpleType type;
            internal string value;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Attribute(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location location, int properties)  {
                this.name = name;
                this.type = type;
                this.value = value;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.attribute(name, type, value, location, properties);
            }
        
        }
        public class Namespace : Saxon.Hej.@event.OutputterEvent {
            internal string prefix;
            internal string uri;
            internal int properties;
            public Namespace(string prefix, string uri, int properties)  {
                this.prefix = prefix;
                this.uri = uri;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.@namespace(prefix, uri, properties);
            }
        
        }
        public class StartContent : Saxon.Hej.@event.OutputterEvent {
            public StartContent()  {
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.startContent();
            }
        
        }
        public class EndElement : Saxon.Hej.@event.OutputterEvent {
            public EndElement()  {
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.endElement();
            }
        
        }
        public class Text : Saxon.Hej.@event.OutputterEvent {
            internal Saxon.Hej.str.UnicodeString content;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Text(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties)  {
                this.content = content;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.characters(content, location, properties);
            }
        
        }
        public class Comment : Saxon.Hej.@event.OutputterEvent {
            internal Saxon.Hej.str.UnicodeString content;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties)  {
                this.content = content;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.comment(content, location, properties);
            }
        
        }
        public class ProcessingInstruction : Saxon.Hej.@event.OutputterEvent {
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
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.processingInstruction(target, content, location, properties);
            }
        
        }
        public class Append : Saxon.Hej.@event.OutputterEvent {
            internal Saxon.Hej.om.Item item;
            internal Saxon.Hej.s9api.Location location;
            internal int properties;
            public Append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location location, int properties)  {
                this.item = item;
                this.location = location;
                this.properties = properties;
            }
            public override void replay(Saxon.Hej.@event.Outputter @out) {
                @out.append(item, location, properties);
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class Outputter : Saxon.Hej.@event.Receiver {
        protected internal Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration;
        protected internal string systemId = null;
        public virtual void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipelineConfiguration = pipe;
        }
        public virtual Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipelineConfiguration;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return pipelineConfiguration.getConfiguration();
        }
        public virtual void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public virtual void open() {
        }
        abstract public void startDocument(int properties);
        abstract public void endDocument();
        public virtual void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        abstract public void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties);
        public virtual void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            startElement(elemName, type, location, properties);
            foreach (Saxon.Hej.om.NamespaceBinding ns in namespaces) {
                @namespace(ns.getPrefix(), ns.getURI(), properties);
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                attribute(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties());
            }
            startContent();
        }
        abstract public void @namespace(string prefix, string namespaceUri, int properties);
        public virtual void namespaces(Saxon.Hej.om.NamespaceBindingSet bindings, int properties) {
            foreach (Saxon.Hej.om.NamespaceBinding nb in bindings) {
                @namespace(nb.getPrefix(), nb.getURI(), properties);
            }
        }
        abstract public void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties);
        public virtual void startContent() {
        }
        abstract public void endElement();
        abstract public void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties);
        abstract public void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties);
        abstract public void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties);
        public virtual void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public virtual void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }
        public virtual Saxon.Hej.str.UniStringConsumer getStringReceiver(bool asTextNode, Saxon.Hej.s9api.Location loc) {
            if (asTextNode) {
                return new Inner_AbstractUniStringConsumer_17(this, loc);
            } else {
                return new Inner_AbstractUniStringConsumer_13(this, loc);
            }
        }
        public virtual void close() {
        }
        public virtual bool usesTypeAnnotations() {
            return false;
        }
    
        private class Inner_AbstractUniStringConsumer_13 : Saxon.Hej.str.AbstractUniStringConsumer {
            private readonly Outputter outerInstance;
            public Inner_AbstractUniStringConsumer_13(Outputter outerInstance, Saxon.Hej.s9api.Location loc) {
                this.outerInstance = outerInstance;
                this.loc = loc;
                buffer = new Saxon.Hej.str.UnicodeBuilder();
            }
            private Saxon.Hej.s9api.Location loc;
            readonly Saxon.Hej.str.UnicodeBuilder buffer;
            public override  /* REPLACING public */ Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                /*field*/buffer.accept(/*localparam*/chars);
                return this;
            }
            public override  /* REPLACING public virtual */ void close() {
                this.outerInstance.append(new Saxon.Hej.value.StringValue(/*field*/buffer.toUnicodeString()));
            }
        }

        private class Inner_AbstractUniStringConsumer_17 : Saxon.Hej.str.AbstractUniStringConsumer {
            private readonly Outputter outerInstance;
            public Inner_AbstractUniStringConsumer_17(Outputter outerInstance, Saxon.Hej.s9api.Location loc) {
                this.outerInstance = outerInstance;
                this.loc = loc;
                buffer = new Saxon.Hej.str.UnicodeBuilder();
            }
            private Saxon.Hej.s9api.Location loc;
            readonly Saxon.Hej.str.UnicodeBuilder buffer;
            public override  /* REPLACING public */ Saxon.Hej.str.UniStringConsumer accept(Saxon.Hej.str.UnicodeString chars) {
                /*field*/buffer.accept(/*localparam*/chars);
                return this;
            }
            public override  /* REPLACING public virtual */ void close() {
                this.outerInstance.characters(/*field*/buffer.toUnicodeString(), /*extraParam*/loc, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public virtual bool handlesAppend() {
            return false;
        }

    }
}
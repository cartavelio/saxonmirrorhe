using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public abstract class Builder : Saxon.Hej.@event.Receiver {
        public const int UNSPECIFIED_TREE_MODEL = -1;
        public const int LINKED_TREE = 0;
        public const int TINY_TREE = 1;
        public const int TINY_TREE_CONDENSED = 2;
        public const int JDOM_TREE = 3;
        public const int JDOM2_TREE = 4;
        public const int AXIOM_TREE = 5;
        public const int DOMINO_TREE = 6;
        public const int MUTABLE_LINKED_TREE = 7;
        protected internal Saxon.Hej.@event.PipelineConfiguration pipe;
        protected internal Saxon.Hej.Configuration config;
        protected internal Saxon.Hej.om.NamePool namePool;
        protected internal string systemId;
        protected internal string baseURI;
        protected internal bool uniformBaseURI = true;
        protected internal Saxon.Hej.om.NodeInfo currentRoot;
        protected internal bool lineNumbering = false;
        protected internal bool useEventLocation = true;
        protected internal Saxon.Hej.om.Durability durability = Saxon.Hej.om.Durability.LASTING;
        protected internal bool started = false;
        protected internal bool timing = false;
        protected internal bool opened = false;
        private long startTime;
        public Builder()  {
        }
        public Builder(Saxon.Hej.@event.PipelineConfiguration pipe)  {
            this.pipe = pipe;
            config = pipe.getConfiguration();
            lineNumbering = config.isLineNumbering();
            namePool = config.getNamePool();
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            config = pipe.getConfiguration();
            lineNumbering = (lineNumbering || config.isLineNumbering());
            namePool = config.getNamePool();
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public virtual Saxon.Hej.@event.BuilderMonitor getBuilderMonitor() {
            return null;
        }
        public void setUseEventLocation(bool useEventLocation) {
            this.useEventLocation = useEventLocation;
        }
        public bool isUseEventLocation() {
            return useEventLocation;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
        public string getBaseURI() {
            return baseURI;
        }
        public virtual void setDurability(Saxon.Hej.om.Durability durability) {
            this.durability = durability;
        }
        public Saxon.Hej.om.Durability getDurability() {
            return durability;
        }
        public void setLineNumbering(bool lineNumbering) {
            this.lineNumbering = lineNumbering;
        }
        public void setTiming(bool on) {
            timing = on;
        }
        public bool isTiming() {
            return timing;
        }
        public virtual void open() {
            if ((timing && !(opened))) {
                string sysId = getSystemId();
                if ((sysId == null)) {
                    sysId = "(unknown systemId)";
                }
                getConfiguration().getLogger().info(((("Building tree for "+sysId)+" using ")+this.GetType()));
                startTime = Saxon.Ejava.lang.System.nanoTime();
            }
            opened = true;
        }
        public virtual void close() {
            if ((timing && opened)) {
                long endTime = Saxon.Ejava.lang.System.nanoTime();
                getConfiguration().getLogger().info(("Tree built in "+Saxon.Hej.trans.Timer.showExecutionTimeNano((endTime-startTime))));
                if ((currentRoot is Saxon.Hej.tree.tiny.TinyDocumentImpl)) {
                    ((Saxon.Hej.tree.tiny.TinyDocumentImpl)(currentRoot)).showSize();
                }
                startTime = endTime;
            }
            opened = false;
        }
        public bool usesTypeAnnotations() {
            return true;
        }
        public virtual Saxon.Hej.om.NodeInfo getCurrentRoot() {
            return currentRoot;
        }
        public virtual void reset() {
            systemId = null;
            baseURI = null;
            currentRoot = null;
            lineNumbering = false;
            started = false;
            timing = false;
            opened = false;
        }
    
        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public virtual bool handlesAppend() {
            return false;
        }

        /* Creating stub method startDocument to satisfy interface Receiver */
        public virtual void startDocument(int arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method endDocument to satisfy interface Receiver */
        public virtual void endDocument() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method setUnparsedEntity to satisfy interface Receiver */
        public virtual void setUnparsedEntity(string arg1, string arg2, string arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method startElement to satisfy interface Receiver */
        public virtual void startElement(Saxon.Hej.om.NodeName arg1, Saxon.Hej.type.SchemaType arg2, Saxon.Hej.om.AttributeMap arg3, Saxon.Hej.om.NamespaceMap arg4, Saxon.Hej.s9api.Location arg5, int arg6) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method endElement to satisfy interface Receiver */
        public virtual void endElement() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method characters to satisfy interface Receiver */
        public virtual void characters(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.s9api.Location arg2, int arg3) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method processingInstruction to satisfy interface Receiver */
        public virtual void processingInstruction(string arg1, Saxon.Hej.str.UnicodeString arg2, Saxon.Hej.s9api.Location arg3, int arg4) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method comment to satisfy interface Receiver */
        public virtual void comment(Saxon.Hej.str.UnicodeString arg1, Saxon.Hej.s9api.Location arg2, int arg3) {
            throw new System.NotImplementedException();
        }

    }
}
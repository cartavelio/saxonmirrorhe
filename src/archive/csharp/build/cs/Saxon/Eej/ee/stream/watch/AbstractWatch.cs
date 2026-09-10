using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public abstract class AbstractWatch : Saxon.Eej.ee.stream.watch.Watch {
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        public AbstractWatch()  {
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            namespaceResolver = resolver;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public virtual void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
        }
        public virtual void processItem(Saxon.Hej.om.Item item) {
        }
        public virtual void close() {
        }
    
        /* Creating stub method matchesNode to satisfy interface Watch */
        public virtual bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode arg1, Saxon.Hej.expr.XPathContext arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method matchesNodesOfKind to satisfy interface Watch */
        public virtual bool matchesNodesOfKind(Saxon.Hej.type.UType arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method setAnchorNode to satisfy interface Watch */
        public virtual void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getAnchorNode to satisfy interface Watch */
        public virtual Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method startSelectedParentNode to satisfy interface Watch */
        public virtual Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode arg1, Saxon.Hej.s9api.Location arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method endSelectedParentNode to satisfy interface Watch */
        public virtual void endSelectedParentNode(Saxon.Hej.s9api.Location arg1) {
            throw new System.NotImplementedException();
        }

    }
}
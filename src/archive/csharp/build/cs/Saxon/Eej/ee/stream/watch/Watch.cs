using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public interface Watch {
        bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context);
        bool matchesNodesOfKind(Saxon.Hej.type.UType kind);
        void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe);
        Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration();
        void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver);
        Saxon.Hej.om.NamespaceResolver getNamespaceResolver();
        void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode anchor);
        Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode();
        void open(Saxon.Eej.ee.stream.watch.Terminator terminator);
        Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId);
        void endSelectedParentNode(Saxon.Hej.s9api.Location locationId);
        void processItem(Saxon.Hej.om.Item item);
        void close();
    
    }
}
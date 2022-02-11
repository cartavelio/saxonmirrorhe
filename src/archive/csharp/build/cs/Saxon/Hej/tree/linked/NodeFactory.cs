using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public interface NodeFactory {
        Saxon.Hej.tree.linked.ElementImpl makeElementNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SchemaType elementType, bool isNilled, Saxon.Hej.om.AttributeMap attlist, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.s9api.Location locationId, int sequenceNumber);
        Saxon.Hej.tree.linked.TextImpl makeTextNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.str.UnicodeString content);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public interface PullProvider {
        void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe);
        Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration();
        Saxon.Hej.pull.PullEvent next();
        Saxon.Hej.pull.PullEvent current();
        Saxon.Hej.om.AttributeMap getAttributes();
        Saxon.Hej.om.NamespaceBinding[] getNamespaceDeclarations();
        Saxon.Hej.pull.PullEvent skipToMatchingEnd();
        void close();
        Saxon.Hej.om.NodeName getNodeName();
        Saxon.Hej.str.UnicodeString getStringValue();
        Saxon.Hej.type.SchemaType getSchemaType();
        Saxon.Hej.value.AtomicValue getAtomicValue();
        Saxon.Hej.s9api.Location getSourceLocator();
        System.Collections.Generic.IList<Saxon.Hej.pull.UnparsedEntity> getUnparsedEntities();
    
    }
}
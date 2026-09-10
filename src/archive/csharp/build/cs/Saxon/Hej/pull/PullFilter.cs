using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class PullFilter : Saxon.Hej.pull.PullProvider {
        private readonly Saxon.Hej.pull.PullProvider @base;
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        protected internal Saxon.Hej.pull.PullEvent currentEvent;
        public PullFilter(Saxon.Hej.pull.PullProvider @base)  {
            this.@base = @base;
            if ((@base.getPipelineConfiguration() != null)) {
                setPipelineConfiguration(@base.getPipelineConfiguration());
            }
            currentEvent = @base.current();
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
            @base.setPipelineConfiguration(pipe);
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return getPipelineConfiguration().getConfiguration().getNamePool();
        }
        public Saxon.Hej.pull.PullProvider getUnderlyingProvider() {
            return @base;
        }
        public virtual Saxon.Hej.pull.PullEvent next() {
            return @base.next();
        }
        public Saxon.Hej.pull.PullEvent current() {
            return currentEvent;
        }
        public Saxon.Hej.om.AttributeMap getAttributes() {
            return @base.getAttributes();
        }
        public Saxon.Hej.om.NamespaceBinding[] getNamespaceDeclarations() {
            return @base.getNamespaceDeclarations();
        }
        public Saxon.Hej.pull.PullEvent skipToMatchingEnd() {
            return @base.skipToMatchingEnd();
        }
        public void close() {
            @base.close();
        }
        public Saxon.Hej.om.NodeName getNodeName() {
            return @base.getNodeName();
        }
        public Saxon.Hej.str.UnicodeString getStringValue() {
            return @base.getStringValue();
        }
        public Saxon.Hej.value.AtomicValue getAtomicValue() {
            return @base.getAtomicValue();
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return @base.getSchemaType();
        }
        public Saxon.Hej.s9api.Location getSourceLocator() {
            return @base.getSourceLocator();
        }
        public System.Collections.Generic.IList<Saxon.Hej.pull.UnparsedEntity> getUnparsedEntities() {
            return @base.getUnparsedEntities();
        }
    
    }
}
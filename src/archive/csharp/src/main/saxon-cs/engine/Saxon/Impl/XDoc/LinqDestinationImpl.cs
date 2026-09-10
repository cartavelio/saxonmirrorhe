using Saxon.Impl.Dom;

namespace Saxon.Impl.XDoc {
    public class LinqDestinationImpl : Saxon.Hej.s9api.AbstractDestination {
        private readonly XDocBuilder builder;
        public LinqDestinationImpl(XDocBuilder builder)  {
            this.builder = builder;
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            builder.setPipelineConfiguration(pipe);
            return @params.makeSequenceNormalizer(builder);
        }
        public override void close() {
        }
    }
}
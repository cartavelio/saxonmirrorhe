
namespace Saxon.Impl.Dom {
    public class DomDestinationImpl : net.sf.saxon.s9api.AbstractDestination {
        private readonly DomBuilder builder;
        public DomDestinationImpl(DomBuilder builder)  {
            this.builder = builder;
        }
        public override net.sf.saxon.@event.Receiver getReceiver(net.sf.saxon.@event.PipelineConfiguration pipe, net.sf.saxon.serialize.SerializationProperties @params) {
            builder.setPipelineConfiguration(pipe);
            return @params.makeSequenceNormalizer(builder);
        }
        public override void close() {
        }
    }
}
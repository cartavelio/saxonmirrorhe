
using Saxon.Ejava.util;
using Saxon.Hej.@event;

namespace Saxon.Impl.Dom {
    public class DomDestinationImpl : Saxon.Hej.s9api.AbstractDestination {
        private readonly DomBuilder builder;
        public DomDestinationImpl(DomBuilder builder)  {
            this.builder = builder;
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            builder.setPipelineConfiguration(pipe);
            return @params.makeSequenceNormalizer(new NamespaceDifferencer(builder, new Properties()));
        }
        public override void close() {
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
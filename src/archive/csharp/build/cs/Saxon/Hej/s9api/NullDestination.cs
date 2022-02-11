using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class NullDestination : Saxon.Hej.s9api.AbstractDestination {
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            return new Saxon.Hej.@event.Sink(pipe);
        }
        public override void close() {
        }
    
    }
}
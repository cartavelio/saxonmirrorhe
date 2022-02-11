using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class TeeDestination : Saxon.Hej.s9api.AbstractDestination {
        private readonly Saxon.Hej.s9api.Destination dest0;
        private readonly Saxon.Hej.s9api.Destination dest1;
        public TeeDestination(Saxon.Hej.s9api.Destination destination0, Saxon.Hej.s9api.Destination destination1)  {
            dest0 = destination0;
            dest1 = destination1;
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            return new Saxon.Hej.@event.TeeOutputter(dest0.getReceiver(pipe, @params), dest1.getReceiver(pipe, @params));
        }
        public override void close() {
            dest0.close();
            dest1.close();
        }
    
    }
}
using System;
using System.Collections.Generic;
namespace net.sf.saxon.dotnet {
    public class DotNetDomDestination : net.sf.saxon.s9api.AbstractDestination {
        private readonly DotNetDomBuilder builder;
        public DotNetDomDestination(DotNetDomBuilder builder)  {
            this.builder = builder;
        }
        public net.sf.saxon.@event.Receiver getReceiver(net.sf.saxon.@event.PipelineConfiguration pipe, net.sf.saxon.serialize.SerializationProperties @params) {
            builder.setPipelineConfiguration(pipe);
            return builder;
        }
        public void close() {
        }
    }
}
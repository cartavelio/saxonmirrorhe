using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamingSequenceWriter : Saxon.Hej.@event.SequenceWriter {
        private readonly Saxon.Hej.@event.Outputter @out;
        public StreamingSequenceWriter(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.@event.Outputter @out) : base (pipe) {
            this.@out = @out;
        }
        public override void write(Saxon.Hej.om.Item item) {
            @out.append(item);
        }
    
    }
}
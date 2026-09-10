using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class RawDestination : Saxon.Hej.s9api.AbstractDestination {
        private Saxon.Hej.@event.SequenceCollector sequenceOutputter;
        private bool closed = false;
        public RawDestination()  {
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            sequenceOutputter = new Saxon.Hej.@event.SequenceCollector(pipe);
            closed = false;
            helper.onClose(() => closed = true);
            return new Saxon.Hej.@event.CloseNotifier(sequenceOutputter, helper.getListeners());
        }
        public override void close() {
            try {
                sequenceOutputter.close();
                closed = true;
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmValue getXdmValue() {
            if (!(closed)) {
                throw new Saxon.Ejava.lang.IllegalStateException("The result sequence has not yet been closed");
            }
            return Saxon.Api.XdmValue.Wrap(sequenceOutputter.getSequence());
        }
    
    }
}
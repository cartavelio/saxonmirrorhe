using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    abstract public class AbstractDestination : Saxon.Hej.s9api.Destination {
        internal Saxon.Hej.s9api.DestinationHelper helper;
        private System.Uri baseURI;
        public AbstractDestination()  {
            helper = new Saxon.Hej.s9api.DestinationHelper(this);
        }
        public void setDestinationBaseURI(System.Uri baseURI) {
            this.baseURI = baseURI;
        }
        public System.Uri getDestinationBaseURI() {
            return baseURI;
        }
        public void onClose(Saxon.Hej.s9api.Action listener) {
            helper.onClose(listener);
        }
        public void closeAndNotify() {
            helper.closeAndNotify();
        }
    
        /* Creating stub method getReceiver to satisfy interface Destination */
        public virtual Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration arg1, Saxon.Hej.serialize.SerializationProperties arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method close to satisfy interface Destination */
        public virtual void close() {
            throw new System.NotImplementedException();
        }

    }
}
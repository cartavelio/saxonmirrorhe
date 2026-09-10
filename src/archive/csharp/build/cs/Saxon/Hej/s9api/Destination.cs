using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface Destination {
        void setDestinationBaseURI(System.Uri baseURI);
        System.Uri getDestinationBaseURI();
        Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params);
        void onClose(Saxon.Hej.s9api.Action listener);
        void closeAndNotify();
        void close();
    
    }
}
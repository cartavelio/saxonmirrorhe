using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface SpaceStrippingRule {
        int isSpacePreserving(Saxon.Hej.om.NodeName nodeName, Saxon.Hej.type.SchemaType schemaType);
        Saxon.Hej.@event.ProxyReceiver makeStripper(Saxon.Hej.@event.Receiver next);
        void export(Saxon.Hej.trace.ExpressionPresenter presenter);
    
    }
}
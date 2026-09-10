using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public interface ReceiverWithOutputProperties : Saxon.Hej.@event.Receiver {
        Saxon.Ejava.util.Properties getOutputProperties();
    
    }
}
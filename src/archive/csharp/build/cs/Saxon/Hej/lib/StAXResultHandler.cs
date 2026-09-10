using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface StAXResultHandler {
        Saxon.Hej.@event.Receiver getReceiver(Saxon.Ejavax.xml.transform.Result result, Saxon.Ejava.util.Properties properties);
    
    }
}
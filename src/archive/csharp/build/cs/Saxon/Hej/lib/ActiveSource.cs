using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ActiveSource : Saxon.Ejavax.xml.transform.Source {
        void deliver(Saxon.Hej.@event.Receiver receiver, Saxon.Hej.lib.ParseOptions options);
    
    }
}
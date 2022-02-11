using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface InvalidityHandler {
        void startReporting(string systemId);
        void reportInvalidity(Saxon.Hej.lib.Invalidity failure);
        Saxon.Hej.om.Sequence endReporting();
    
    }
}
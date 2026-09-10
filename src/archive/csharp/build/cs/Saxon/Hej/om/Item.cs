using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface Item : Saxon.Hej.om.GroundedValue {
        Saxon.Hej.om.Genre getGenre();
        /* implicit =====
        Saxon.Hej.str.UnicodeString getUnicodeStringValue();
        ===== implicit */
        Saxon.Hej.om.AtomicSequence atomize();
        bool isStreamed();
    
    }
}
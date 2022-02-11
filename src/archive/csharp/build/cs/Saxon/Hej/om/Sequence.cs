using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface Sequence {
        Saxon.Hej.om.Item head();
        Saxon.Hej.om.SequenceIterator iterate();
        Saxon.Hej.om.GroundedValue materialize();
        Saxon.Hej.om.Sequence makeRepeatable();
    
    }
}
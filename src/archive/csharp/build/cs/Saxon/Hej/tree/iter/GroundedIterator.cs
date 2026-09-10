using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public interface GroundedIterator : Saxon.Hej.om.SequenceIterator {
        bool isActuallyGrounded();
        Saxon.Hej.om.GroundedValue getResidue();
        Saxon.Hej.om.GroundedValue materialize();
    
    }
}
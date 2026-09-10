using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface AtomizedValueIterator : Saxon.Hej.om.SequenceIterator {
        Saxon.Hej.om.AtomicSequence nextAtomizedValue();
    
    }
}
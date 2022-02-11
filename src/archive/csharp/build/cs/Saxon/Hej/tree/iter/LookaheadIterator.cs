using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public interface LookaheadIterator : Saxon.Hej.om.SequenceIterator {
        bool supportsHasNext();
        bool hasNext();
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public interface ReversibleIterator : Saxon.Hej.om.SequenceIterator {
        Saxon.Hej.om.SequenceIterator getReverseIterator();
    
    }
}
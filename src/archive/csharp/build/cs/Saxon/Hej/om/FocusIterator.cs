using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface FocusIterator : Saxon.Hej.om.SequenceIterator {
        Saxon.Hej.om.Item current();
        int position();
        int getLength();
    
    }
}
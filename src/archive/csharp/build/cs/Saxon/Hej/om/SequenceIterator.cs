using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface SequenceIterator : Saxon.Ejava.io.Closeable {
        Saxon.Hej.om.Item next();
        void discharge();
    
    }
}
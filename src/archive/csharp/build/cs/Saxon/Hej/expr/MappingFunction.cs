using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface MappingFunction {
        Saxon.Hej.om.SequenceIterator map(Saxon.Hej.om.Item item);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public interface NodePredicate {
        bool test(Saxon.Hej.om.NodeInfo node);
    
    }
}
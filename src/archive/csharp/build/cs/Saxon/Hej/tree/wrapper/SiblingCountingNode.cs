using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public interface SiblingCountingNode : Saxon.Hej.om.NodeInfo {
        int getSiblingPosition();
    
    }
}
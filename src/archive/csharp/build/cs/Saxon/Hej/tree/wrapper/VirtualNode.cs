using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public interface VirtualNode : Saxon.Hej.om.NodeInfo {
        System.Object getUnderlyingNode();
        System.Object getRealNode();
    
    }
}
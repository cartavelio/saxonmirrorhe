using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public interface WrappingFunction {
        Saxon.Hej.tree.wrapper.VirtualNode makeWrapper(Saxon.Hej.om.NodeInfo node, Saxon.Hej.tree.wrapper.VirtualNode parent);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public interface SteppingNode : Saxon.Hej.om.NodeInfo {
        Saxon.Hej.tree.util.SteppingNode getNextSibling();
        Saxon.Hej.tree.util.SteppingNode getPreviousSibling();
        Saxon.Hej.tree.util.SteppingNode getFirstChild();
        Saxon.Hej.tree.util.SteppingNode getSuccessorElement(Saxon.Hej.tree.util.SteppingNode anchor, string uri, string local);
    
    }
}
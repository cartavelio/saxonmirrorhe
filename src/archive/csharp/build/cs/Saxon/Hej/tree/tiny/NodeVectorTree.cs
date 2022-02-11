using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public interface NodeVectorTree {
        bool isTyped();
        Saxon.Hej.om.NodeInfo getNode(int nodeNr);
        int getNodeKind(int nodeNr);
        int getFingerprint(int nodeNr);
        byte[] getNodeKindArray();
        int[] getNameCodeArray();
    
    }
}
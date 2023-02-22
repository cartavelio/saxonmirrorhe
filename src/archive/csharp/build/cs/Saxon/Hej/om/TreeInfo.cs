using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface TreeInfo {
        Saxon.Hej.om.NodeInfo getRootNode();
        Saxon.Hej.Configuration getConfiguration();
        long getDocumentNumber();
        bool isTyped();
        bool isMutable();
        Saxon.Hej.om.NodeInfo selectID(string id, bool getParent);
        System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames();
        string[] getUnparsedEntity(string name);
        void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule);
        Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule();
        void setUserData(string key, System.Object value);
        System.Object getUserData(string key);
    
    }
}
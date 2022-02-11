using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface PendingUpdateList {
        void apply(Saxon.Hej.expr.XPathContext context, int validationMode);
        System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> getAffectedTrees();
        void addPutAction(Saxon.Hej.om.NodeInfo node, string uri, Saxon.Hej.expr.Expression originator);
    
    }
}
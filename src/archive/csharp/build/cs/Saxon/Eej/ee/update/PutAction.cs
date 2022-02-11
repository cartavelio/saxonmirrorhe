using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class PutAction : Saxon.Eej.ee.update.PendingUpdateAction {
        private readonly Saxon.Hej.om.NodeInfo node;
        private readonly string outputUri;
        public PutAction(Saxon.Hej.om.NodeInfo node, string outputUri)  {
            this.node = node;
            this.outputUri = outputUri;
        }
        public override void apply(Saxon.Hej.expr.XPathContext context, System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> affectedRootSet) {
            Saxon.Ejavax.xml.transform.Result result = new Saxon.Ejavax.xml.transform.stream.StreamResult(outputUri);
            Saxon.Ejava.util.Properties props = context.getController().getExecutable().getPrimarySerializationProperties().getProperties();
            Saxon.Hej.query.QueryResult.serialize(node, result, props);
        }
        public override Saxon.Hej.om.NodeInfo getTargetNode() {
            return node;
        }
        public override int getApplyPhase() {
            return PHASE;
        }
        public const int PHASE = 6;
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.om {
    public class FleetingDocumentNode : Saxon.Eej.ee.stream.om.FleetingParentNode, Saxon.Hej.om.TreeInfo {
        private Saxon.Hej.Configuration config;
        private string systemId;
        private readonly long documentNumber;
        private int lastNodeNumber = 1;
        private System.Collections.Generic.Dictionary<string,System.Object> userData;
        private Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiAccumulatorWatch;
        private System.Collections.Generic.IDictionary<string,string[]> unparsedEntities;
        private System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> initialCommentsAndProcessingInstructions;
        private Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
        public FleetingDocumentNode(long docNr)  {
            setNodeKind(Saxon.Hej.type.Type.DOCUMENT);
            root = this;
            setSystemId("");
            this.documentNumber = docNr;
            setHasChildren(true);
        }
        public Saxon.Hej.om.NodeInfo getRootNode() {
            return this;
        }
        public override Saxon.Hej.om.TreeInfo getTreeInfo() {
            return this;
        }
        protected internal int allocateNodeNumber() {
            return lastNodeNumber++;
        }
        public long getDocumentNumber() {
            return documentNumber;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public void setUnparsedEntity(string name, string systemID, string publicID) {
            if ((unparsedEntities == null)) {
                unparsedEntities = new System.Collections.Generic.Dictionary<string,string[]>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(unparsedEntities, name, new string[]{systemID, publicID});
        }
        public void captureInitialCommentOrProcessingInstruction(Saxon.Hej.om.NodeInfo node) {
            if ((initialCommentsAndProcessingInstructions == null)) {
                initialCommentsAndProcessingInstructions = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(4);
            }
            initialCommentsAndProcessingInstructions.Add(node);
        }
        public void copyInitialCommentsAndProcessingInstructions(Saxon.Hej.@event.Receiver @out) {
            if ((initialCommentsAndProcessingInstructions != null)) {
                foreach (Saxon.Hej.om.NodeInfo node in initialCommentsAndProcessingInstructions) {
                    switch (node.getNodeKind()) {
                      case Saxon.Hej.type.Type.COMMENT:
                        @out.comment(node.getUnicodeStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                        @out.processingInstruction(node.getLocalPart(), node.getUnicodeStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalStateException();
                    }
                }
            }
            initialCommentsAndProcessingInstructions = null;
        }
        public Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            return null;
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.UntypedAtomicValue(getUnicodeStringValue());
        }
        public System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            if ((unparsedEntities == null)) {
                System.Collections.Generic.ICollection<string> e = new System.Collections.Generic.List<string>();
                return e.GetEnumerator();
            } else {
                return Saxon.Impl.Helpers.MapUtils.KeySet(unparsedEntities).GetEnumerator();
            }
        }
        public string[] getUnparsedEntity(string name) {
            if ((unparsedEntities == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(unparsedEntities, name);
            }
        }
        public override void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public override Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public override Saxon.Hej.om.NamePool getNamePool() {
            return config.getNamePool();
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("copy() is not supported in streaming mode for document nodes");
        }
        public override string getSystemId() {
            return systemId;
        }
        public override string getBaseURI() {
            return systemId;
        }
        public void setMultiAccumulatorWatch(Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiWatch) {
            this.multiAccumulatorWatch = multiWatch;
        }
        public Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch getMultiAccumulatorWatch() {
            return this.multiAccumulatorWatch;
        }
        public void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            this.spaceStrippingRule = rule;
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return spaceStrippingRule;
        }
        public void setUserData(string key, System.Object value) {
            if ((userData == null)) {
                userData = new System.Collections.Generic.Dictionary<string,System.Object>(4);
            }
            if ((value == null)) {
                userData.Remove(key);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(userData, key, value);
            }
        }
        public System.Object getUserData(string key) {
            if ((userData == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(userData, key);
            }
        }
        public override void notify(Saxon.Hej.@event.Receiver @out) {
            @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
        }
    
        /* Expanding default method isTyped from interface TreeInfo */
        public virtual bool isTyped() {
            return false;
        }

        /* Expanding default method isMutable from interface TreeInfo */
        public virtual bool isMutable() {
            return false;
        }

    }
}
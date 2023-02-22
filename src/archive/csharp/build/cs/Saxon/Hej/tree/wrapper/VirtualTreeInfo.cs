using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class VirtualTreeInfo : Saxon.Hej.om.GenericTreeInfo {
        private bool copyAccumulators;
        public VirtualTreeInfo(Saxon.Hej.Configuration config) : base (config) {
        }
        public VirtualTreeInfo(Saxon.Hej.Configuration config, Saxon.Hej.tree.wrapper.VirtualCopy vc) : base (config, vc) {
        }
        public void setCopyAccumulators(bool copy) {
            this.copyAccumulators = copy;
        }
        public bool isCopyAccumulators() {
            return copyAccumulators;
        }
        public override System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            return ((Saxon.Hej.tree.wrapper.VirtualCopy)(getRootNode())).getOriginalNode().getTreeInfo().getUnparsedEntityNames();
        }
        public override string[] getUnparsedEntity(string name) {
            return ((Saxon.Hej.tree.wrapper.VirtualCopy)(getRootNode())).getOriginalNode().getTreeInfo().getUnparsedEntity(name);
        }
    
    }
}
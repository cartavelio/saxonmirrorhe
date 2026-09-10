using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class GenericTreeInfo : Saxon.Hej.om.TreeInfo {
        private Saxon.Hej.Configuration config;
        protected internal Saxon.Hej.om.NodeInfo root;
        private string systemId;
        private System.Collections.Generic.IDictionary<string,System.Object> userData;
        private long documentNumber = -1;
        private Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
        public GenericTreeInfo(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public GenericTreeInfo(Saxon.Hej.Configuration config, Saxon.Hej.om.NodeInfo root)  {
            this.config = config;
            setRootNode(root);
        }
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setRootNode(Saxon.Hej.om.NodeInfo root) {
            if ((root.getParent() != null)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("The root node of a tree must be parentless");
            }
            this.root = root;
        }
        public virtual Saxon.Hej.om.NodeInfo getRootNode() {
            return root;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public string getPublicId() {
            return null;
        }
        public long getDocumentNumber() {
            if ((documentNumber == -1)) {
                Saxon.Hej.tree.util.DocumentNumberAllocator dna = config.getDocumentNumberAllocator();
                lock (this) {
                    if ((documentNumber == -1)) {
                        documentNumber = dna.allocateDocumentNumber();
                    }
                }
            }
            return documentNumber;
        }
        public void setDocumentNumber(long documentNumber) { lock(this) {
            this.documentNumber = documentNumber;
        }}
        public virtual Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            return null;
        }
        public virtual System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            return new System.Collections.Generic.List<string>().GetEnumerator();;
        }
        public virtual string[] getUnparsedEntity(string name) {
            return null;
        }
        public void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            this.spaceStrippingRule = rule;
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return spaceStrippingRule;
        }
        public void setUserData(string key, System.Object value) {
            if ((userData == null)) {
                userData = new System.Collections.Generic.Dictionary<string,System.Object>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(userData, key, value);
        }
        public System.Object getUserData(string key) {
            if ((userData == null)) {
                return userData;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(userData, key);
            }
        }
        public bool isStreamed() {
            return false;
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
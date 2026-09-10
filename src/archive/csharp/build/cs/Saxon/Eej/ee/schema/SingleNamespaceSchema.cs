using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SingleNamespaceSchema : Saxon.Eej.ee.schema.PreparedSchema {
        private Saxon.Hej.om.NamespaceUri targetNamespace;
        public SingleNamespaceSchema(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.om.NamespaceUri targetNamespace) : base (config) {
            this.targetNamespace = targetNamespace;
        }
        public void setTargetNamespace(Saxon.Hej.om.NamespaceUri targetNamespace) {
            this.targetNamespace = targetNamespace;
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return targetNamespace;
        }
    
    }
}
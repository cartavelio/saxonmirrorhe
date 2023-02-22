using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SingleNamespaceSchema : Saxon.Eej.ee.schema.PreparedSchema {
        private string targetNamespace;
        public SingleNamespaceSchema(Saxon.Eej.config.EnterpriseConfiguration config, string targetNamespace) : base (config) {
            this.targetNamespace = targetNamespace;
        }
        public void setTargetNamespace(string targetNamespace) {
            this.targetNamespace = Saxon.Hej.value.Whitespace.trim(targetNamespace);
        }
        public string getTargetNamespace() {
            return targetNamespace;
        }
    
    }
}
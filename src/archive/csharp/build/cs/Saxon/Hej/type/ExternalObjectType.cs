using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class ExternalObjectType : Saxon.Hej.type.AnyExternalObjectType {
        public abstract string getName();
        public abstract string getTargetNamespace();
        public abstract Saxon.Hej.om.StructuredQName getTypeName();
        public override bool isPlainType() {
            return false;
        }
        public override string ToString() {
            return getTypeName().ToString();
        }
    
    }
}
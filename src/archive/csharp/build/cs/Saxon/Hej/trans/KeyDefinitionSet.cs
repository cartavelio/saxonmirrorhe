using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class KeyDefinitionSet {
        private readonly Saxon.Hej.om.StructuredQName keyName;
        private readonly int keySetNumber;
        private readonly System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> keyDefinitions;
        private string collationName;
        private bool composite;
        private bool backwardsCompatible;
        private bool rangeKey;
        private bool reusable = true;
        public KeyDefinitionSet(Saxon.Hej.om.StructuredQName keyName, int keySetNumber)  {
            this.keyName = keyName;
            this.keySetNumber = keySetNumber;
            keyDefinitions = new System.Collections.Generic.List<Saxon.Hej.trans.KeyDefinition>(3);
        }
        public void addKeyDefinition(Saxon.Hej.trans.KeyDefinition keyDef) {
            if (keyDefinitions.Count==0) {
                collationName = keyDef.getCollationName();
                composite = keyDef.isComposite();
            } else {
                if (((((collationName == null) && (keyDef.getCollationName() != null))) || (((collationName != null) && !(collationName.Equals(keyDef.getCollationName())))))) {
                    throw new Saxon.Hej.trans.XPathException("All keys with the same name must use the same collation", "XTSE1220");
                }
                if ((keyDef.isComposite() != composite)) {
                    throw new Saxon.Hej.trans.XPathException("All keys with the same name must have the same value for @composite", "XTSE1222");
                }
                System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> v = getKeyDefinitions();
                foreach (Saxon.Hej.trans.KeyDefinition other in v) {
                    if ((keyDef.getMatch().isEqual(other.getMatch()) && keyDef.getBody().isEqual(other.getBody()))) {
                        return ;
                    }
                }
            }
            if (keyDef.isBackwardsCompatible()) {
                backwardsCompatible = true;
            }
            if (keyDef.isRangeKey()) {
                rangeKey = true;
            }
            keyDefinitions.Add(keyDef);
        }
        public Saxon.Hej.om.StructuredQName getKeyName() {
            return keyName;
        }
        public string getCollationName() {
            return collationName;
        }
        public bool isComposite() {
            return composite;
        }
        public int getKeySetNumber() {
            return keySetNumber;
        }
        public System.Collections.Generic.IList<Saxon.Hej.trans.KeyDefinition> getKeyDefinitions() {
            return keyDefinitions;
        }
        public bool isBackwardsCompatible() {
            return backwardsCompatible;
        }
        public bool isRangeKey() {
            return rangeKey;
        }
        public void setReusable(bool reusable) {
            this.reusable = reusable;
        }
        public bool isReusable() {
            return reusable;
        }
    
    }
}
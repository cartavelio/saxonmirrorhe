using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class AncestorCharacteristic : Saxon.Eej.ee.trans.Characteristic {
        private readonly Saxon.Hej.pattern.Pattern pattern;
        private readonly bool isParent;
        public AncestorCharacteristic(Saxon.Hej.pattern.Pattern pattern, bool isParent)  {
            this.pattern = pattern;
            this.isParent = isParent;
        }
        public override Saxon.Hej.value.BooleanValue evaluate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
            if (isParent) {
                return Saxon.Hej.value.BooleanValue.get(pattern.matches(((Saxon.Hej.om.NodeInfo)(item)).getParent(), context));
            } else {
                Saxon.Hej.om.NodeInfo a = (Saxon.Hej.om.NodeInfo)(item);
                while ((a != null)) {
                    if (pattern.matches(a, context)) {
                        return Saxon.Hej.value.BooleanValue.TRUE;
                    }
                    a = a.getParent();
                }
                return Saxon.Hej.value.BooleanValue.FALSE;
            }
        }
        public override int GetHashCode() {
            return (pattern.GetHashCode()^((isParent ? 8484 : 7373)));
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Eej.ee.trans.AncestorCharacteristic) && pattern.isEqual(((Saxon.Eej.ee.trans.AncestorCharacteristic)(obj)).pattern)) && (isParent == ((Saxon.Eej.ee.trans.AncestorCharacteristic)(obj)).isParent));
        }
        public override string ToString() {
            return (pattern+((isParent ? "/" : "//")));
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public abstract class Characteristic {
        internal int slotNumber;
        public Characteristic()  {
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        public void setSlotNumber(int slotNumber) {
            this.slotNumber = slotNumber;
        }
        public abstract System.Object evaluate(Saxon.Hej.expr.XPathContext context);
    
    }
}
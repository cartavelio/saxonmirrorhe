using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public interface ScopedBindingElement {
        int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree);
    
    }
}
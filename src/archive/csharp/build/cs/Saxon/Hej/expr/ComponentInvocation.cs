using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ComponentInvocation {
        Saxon.Hej.expr.Component getFixedTarget();
        void setBindingSlot(int slot);
        int getBindingSlot();
        Saxon.Hej.trans.SymbolicName getSymbolicName();
    
    }
}
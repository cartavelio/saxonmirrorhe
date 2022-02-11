using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface LocalBinding : Saxon.Hej.expr.Binding {
        int getLocalSlotNumber();
        void setIndexedVariable();
        bool isIndexedVariable();
    
    }
}
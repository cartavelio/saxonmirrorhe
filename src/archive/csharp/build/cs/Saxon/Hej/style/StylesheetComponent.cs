using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public interface StylesheetComponent {
        Saxon.Hej.expr.instruct.SlotManager getSlotManager();
        void optimize(Saxon.Hej.style.ComponentDeclaration declaration);
        Saxon.Hej.expr.instruct.Actor getActor();
        Saxon.Hej.trans.SymbolicName getSymbolicName();
        void checkCompatibility(Saxon.Hej.expr.Component component);
    
    }
}
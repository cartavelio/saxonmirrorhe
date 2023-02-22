using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public interface GlobalVariableManager {
        Saxon.Hej.expr.instruct.GlobalVariable getEquivalentVariable(Saxon.Hej.expr.Expression select);
        void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable);
    
    }
}
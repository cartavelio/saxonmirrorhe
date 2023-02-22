using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public interface FunctionLibrary {
        void setConfiguration(Saxon.Hej.Configuration config);
        bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel);
        Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons);
        Saxon.Hej.functions.FunctionLibrary copy();
        Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public interface XQueryFunctionBinder : Saxon.Hej.functions.FunctionLibrary {
        Saxon.Hej.query.XQueryFunction getDeclaration(Saxon.Hej.om.StructuredQName functionName, int staticArgs);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public interface FunctionDefinition {
        Saxon.Hej.om.StructuredQName getFunctionName();
        int getNumberOfParameters();
        int getMinimumArity();
        Saxon.Hej.om.StructuredQName getParameterName(int i);
        Saxon.Hej.expr.Expression getDefaultValueExpression(int i);
        int getPositionOfParameter(Saxon.Hej.om.StructuredQName name);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public interface InstructionWithComplexContent {
        Saxon.Hej.expr.Expression getContentExpression();
    
    }
}
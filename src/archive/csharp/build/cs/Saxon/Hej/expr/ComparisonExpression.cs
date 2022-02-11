using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public interface ComparisonExpression {
        Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer();
        int getSingletonOperator();
        Saxon.Hej.expr.Operand getLhs();
        Saxon.Hej.expr.Operand getRhs();
        Saxon.Hej.expr.Expression getLhsExpression();
        Saxon.Hej.expr.Expression getRhsExpression();
        bool convertsUntypedToOther();
    
    }
}
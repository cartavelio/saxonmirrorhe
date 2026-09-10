using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class TreatExpression {
        private TreatExpression()  {
        }
        public static Saxon.Hej.expr.Expression make(Saxon.Hej.expr.Expression sequence, Saxon.Hej.value.SequenceType type) {
            return make(sequence, type, "XPDY0050");
        }
        public static Saxon.Hej.expr.Expression make(Saxon.Hej.expr.Expression sequence, Saxon.Hej.value.SequenceType type, string errorCode) {
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.TYPE_OP, "treat as", 0);
            role.setErrorCode(errorCode);
            Saxon.Hej.expr.Expression e = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(sequence, type.getCardinality(), role);
            return new Saxon.Hej.expr.ItemChecker(e, type.getPrimaryType(), role);
        }
    
    }
}
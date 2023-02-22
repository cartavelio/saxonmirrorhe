using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class LetClausePull : Saxon.Hej.expr.flwor.TuplePull {
        internal Saxon.Hej.expr.flwor.TuplePull @base;
        internal Saxon.Hej.expr.flwor.LetClause letClause;
        public LetClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.LetClause letClause)  {
            this.@base = @base;
            this.letClause = letClause;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if (!(@base.nextTuple(context))) {
                return false;
            }
            letClause.evaluateRangeVariable(context);
            return true;
        }
        public override void close() {
            @base.close();
        }
    
    }
}
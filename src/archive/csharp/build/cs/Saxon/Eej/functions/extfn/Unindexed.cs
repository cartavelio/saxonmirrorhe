using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Unindexed : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression arg = arguments[0];
            while ((arg is Saxon.Hej.expr.UnaryExpression)) {
                if (((((arg is Saxon.Hej.expr.ItemChecker) || (arg is Saxon.Hej.expr.CardinalityChecker)) || (arg is Saxon.Hej.expr.sort.DocumentSorter)) || (arg is Saxon.Hej.expr.HomogeneityChecker))) {
                    arg = ((Saxon.Hej.expr.UnaryExpression)(arg)).getBaseExpression();
                }
            }
            if ((arg is Saxon.Hej.expr.FilterExpression)) {
                ((Saxon.Hej.expr.FilterExpression)(arg)).disableIndexing();
            } else {
                if ((arg is Saxon.Hej.expr.SlashExpression)) {
                    ((Saxon.Hej.expr.SlashExpression)(arg)).disableIndexing();
                }
            }
            return arguments[0];
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return arguments[0];
        }
    
    }
}
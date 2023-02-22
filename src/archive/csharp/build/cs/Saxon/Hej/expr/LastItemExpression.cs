using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public sealed class LastItemExpression : Saxon.Hej.expr.SingleItemFilter {
        public LastItemExpression(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.LastItemExpression exp = new Saxon.Hej.expr.LastItemExpression(getBaseExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator forwards = getBaseExpression().iterate(context);
            if ((forwards is Saxon.Hej.tree.iter.ReversibleIterator)) {
                return ((Saxon.Hej.tree.iter.ReversibleIterator)(forwards)).getReverseIterator().next();
            } else {
                Saxon.Hej.om.Item current = null;
                while (true) {
                    Saxon.Hej.om.Item item = forwards.next();
                    if ((item == null)) {
                        return current;
                    }
                    current = item;
                }
            }
        }
        public override string getExpressionName() {
            return "lastOf";
        }
    
    }
}
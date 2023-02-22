using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class SingletonIntersectExpression : Saxon.Hej.expr.VennExpression {
        public SingletonIntersectExpression(Saxon.Hej.expr.Expression p1, int op, Saxon.Hej.expr.Expression p2) : base (p1, op, p2) {
        }
        public override Saxon.Hej.expr.Expression simplify() {
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.SingletonIntersectExpression exp = new Saxon.Hej.expr.SingletonIntersectExpression(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.om.NodeInfo m = (Saxon.Hej.om.NodeInfo)(getLhsExpression().evaluateItem(c));
            if ((m == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
            }
            Saxon.Hej.om.SequenceIterator iter = getRhsExpression().iterate(c);
            Saxon.Hej.om.NodeInfo n;
            while (((n = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                if (n.Equals(m)) {
                    iter.close();
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(m);
                }
            }
            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.om.NodeInfo m = (Saxon.Hej.om.NodeInfo)(getLhsExpression().evaluateItem(c));
            return ((m != null) && containsNode(getRhsExpression().iterate(c), m));
        }
        public static bool containsNode(Saxon.Hej.om.SequenceIterator iter, Saxon.Hej.om.NodeInfo m) {
            Saxon.Hej.om.NodeInfo n;
            while (((n = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                if (n.Equals(m)) {
                    iter.close();
                    return true;
                }
            }
            return false;
        }
        public override string getExpressionName() {
            return "singleton-intersect";
        }
        protected internal override string displayOperator() {
            return "among";
        }
        protected internal override string tag() {
            return "among";
        }
    
    }
}
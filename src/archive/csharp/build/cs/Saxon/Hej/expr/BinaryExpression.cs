using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class BinaryExpression : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand lhs;
        private readonly Saxon.Hej.expr.Operand rhs;
        protected internal int @operator;
        public BinaryExpression(Saxon.Hej.expr.Expression p0, int op, Saxon.Hej.expr.Expression p1)  {
            @operator = op;
            lhs = new Saxon.Hej.expr.Operand(this, p0, getOperandRole(0));
            rhs = new Saxon.Hej.expr.Operand(this, p1, getOperandRole(1));
            adoptChildExpression(p0);
            adoptChildExpression(p1);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(lhs, rhs);
        }
        protected internal virtual Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public Saxon.Hej.expr.Operand getLhs() {
            return lhs;
        }
        public Saxon.Hej.expr.Expression getLhsExpression() {
            return lhs.getChildExpression();
        }
        public void setLhsExpression(Saxon.Hej.expr.Expression child) {
            lhs.setChildExpression(child);
        }
        public Saxon.Hej.expr.Operand getRhs() {
            return rhs;
        }
        public Saxon.Hej.expr.Expression getRhsExpression() {
            return rhs.getChildExpression();
        }
        public void setRhsExpression(Saxon.Hej.expr.Expression child) {
            rhs.setChildExpression(child);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            resetLocalStaticProperties();
            lhs.typeCheck(visitor, contextInfo);
            rhs.typeCheck(visitor, contextInfo);
            try {
                if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                    Saxon.Hej.om.GroundedValue v = evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()).materialize();
                    return Saxon.Hej.expr.Literal.makeLiteral(v, this);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            lhs.optimize(visitor, contextItemType);
            rhs.optimize(visitor, contextItemType);
            try {
                Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
                if (((opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING) && ((getLhsExpression() is Saxon.Hej.expr.Literal))) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                    Saxon.Hej.om.Item item = evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext());
                    if ((item != null)) {
                        Saxon.Hej.om.GroundedValue v = item.materialize();
                        return Saxon.Hej.expr.Literal.makeLiteral(v, this);
                    }
                }
            } catch (Saxon.Hej.trans.XPathException err) {
            }
            return this;
        }
        public override void setFlattened(bool flattened) {
            getLhsExpression().setFlattened(flattened);
            getRhsExpression().setFlattened(flattened);
        }
        public int getOperator() {
            return @operator;
        }
        protected internal override int computeCardinality() {
            Saxon.Hej.expr.Expression lhs = getLhsExpression();
            Saxon.Hej.expr.Expression rhs = getRhsExpression();
            if ((((!(Saxon.Hej.value.Cardinality.allowsZero(lhs.getCardinality())) && (lhs.getItemType() is Saxon.Hej.type.AtomicType)) && !(Saxon.Hej.value.Cardinality.allowsZero(rhs.getCardinality()))) && (rhs.getItemType() is Saxon.Hej.type.AtomicType))) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return (p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        protected internal static bool isCommutative(int @operator) {
            return ((((((((((@operator == Saxon.Hej.expr.parser.Token.AND) || (@operator == Saxon.Hej.expr.parser.Token.OR)) || (@operator == Saxon.Hej.expr.parser.Token.UNION)) || (@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) || (@operator == Saxon.Hej.expr.parser.Token.PLUS)) || (@operator == Saxon.Hej.expr.parser.Token.MULT)) || (@operator == Saxon.Hej.expr.parser.Token.EQUALS)) || (@operator == Saxon.Hej.expr.parser.Token.FEQ)) || (@operator == Saxon.Hej.expr.parser.Token.NE)) || (@operator == Saxon.Hej.expr.parser.Token.FNE));
        }
        protected internal static bool isAssociative(int @operator) {
            return ((((((@operator == Saxon.Hej.expr.parser.Token.AND) || (@operator == Saxon.Hej.expr.parser.Token.OR)) || (@operator == Saxon.Hej.expr.parser.Token.UNION)) || (@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) || (@operator == Saxon.Hej.expr.parser.Token.PLUS)) || (@operator == Saxon.Hej.expr.parser.Token.MULT));
        }
        protected internal static bool isInverse(int op1, int op2) {
            return ((op1 != op2) && (op1 == Saxon.Hej.expr.parser.Token.inverse(op2)));
        }
        public override int getImplementationMethod() {
            return (EVALUATE_METHOD|ITERATE_METHOD);
        }
        public override bool Equals(System.Object other) {
            if (((other is Saxon.Hej.expr.BinaryExpression) && hasCompatibleStaticContext((Saxon.Hej.expr.Expression)(other)))) {
                Saxon.Hej.expr.BinaryExpression b = (Saxon.Hej.expr.BinaryExpression)(other);
                Saxon.Hej.expr.Expression lhs1 = getLhsExpression();
                Saxon.Hej.expr.Expression rhs1 = getRhsExpression();
                Saxon.Hej.expr.Expression lhs2 = b.getLhsExpression();
                Saxon.Hej.expr.Expression rhs2 = b.getRhsExpression();
                if ((@operator == b.@operator)) {
                    if ((lhs1.isEqual(lhs2) && rhs1.isEqual(rhs2))) {
                        return true;
                    }
                    if (((isCommutative(@operator) && lhs1.isEqual(rhs2)) && rhs1.isEqual(lhs2))) {
                        return true;
                    }
                    if ((isAssociative(@operator) && pairwiseEqual(flattenExpression(), b.flattenExpression()))) {
                        return true;
                    }
                }
                return ((isInverse(@operator, b.@operator) && lhs1.isEqual(rhs2)) && rhs1.isEqual(lhs2));
            }
            return false;
        }
        private System.Collections.Generic.IList<Saxon.Hej.expr.Expression> flattenExpression() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            return flattenExpression(list);
        }
        private System.Collections.Generic.IList<Saxon.Hej.expr.Expression> flattenExpression(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if (((getLhsExpression() is Saxon.Hej.expr.BinaryExpression) && (((Saxon.Hej.expr.BinaryExpression)(getLhsExpression())).@operator == @operator))) {
                ((Saxon.Hej.expr.BinaryExpression)(getLhsExpression())).flattenExpression(list);
            } else {
                int h = getLhsExpression().GetHashCode();
                list.Add(getLhsExpression());
                int i = (list.Count-1);
                while (((i>0) && (h>list[(i-1)].GetHashCode()))) {
                    list[i] = list[(i-1)];
                    list[(i-1)] = getLhsExpression();
                    i--;
                }
            }
            if (((getRhsExpression() is Saxon.Hej.expr.BinaryExpression) && (((Saxon.Hej.expr.BinaryExpression)(getRhsExpression())).@operator == @operator))) {
                ((Saxon.Hej.expr.BinaryExpression)(getRhsExpression())).flattenExpression(list);
            } else {
                int h = getRhsExpression().GetHashCode();
                list.Add(getRhsExpression());
                int i = (list.Count-1);
                while (((i>0) && (h>list[(i-1)].GetHashCode()))) {
                    list[i] = list[(i-1)];
                    list[(i-1)] = getRhsExpression();
                    i--;
                }
            }
            return list;
        }
        private bool pairwiseEqual<T> (System.Collections.Generic.IList<T> a, System.Collections.Generic.IList<T> b) {
            if ((a.Count != b.Count)) {
                return false;
            }
            for (int i = 0; (i<a.Count); i++) {
                if (!(a[i].Equals(b[i]))) {
                    return false;
                }
            }
            return true;
        }
        protected internal override int computeHashCode() {
            int op = System.Math.Min(@operator, Saxon.Hej.expr.parser.Token.inverse(@operator));
            return (((("BinaryExpression "+op)).GetHashCode()^getLhsExpression().GetHashCode())^getRhsExpression().GetHashCode());
        }
        public override string ToString() {
            return ((((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getLhsExpression())+" ")+displayOperator())+" ")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getRhsExpression()));
        }
        public override string toShortString() {
            return ((((parenthesize(getLhsExpression())+" ")+displayOperator())+" ")+parenthesize(getRhsExpression()));
        }
        private string parenthesize(Saxon.Hej.expr.Expression operand) {
            string operandStr = operand.toShortString();
            if (((operand is Saxon.Hej.expr.BinaryExpression) && (Saxon.Hej.expr.parser.XPathParser.operatorPrecedence(((Saxon.Hej.expr.BinaryExpression)(operand)).@operator)<Saxon.Hej.expr.parser.XPathParser.operatorPrecedence(@operator)))) {
                operandStr = (("("+operandStr)+")");
            }
            return operandStr;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement(tag(), this);
            @out.emitAttribute("op", displayOperator());
            explainExtraAttributes(@out);
            getLhsExpression().export(@out);
            getRhsExpression().export(@out);
            @out.endElement();
        }
        protected internal virtual string tag() {
            return "operator";
        }
        protected internal virtual void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        protected internal virtual string displayOperator() {
            return Saxon.Hej.expr.parser.Token.tokens[@operator];
        }
    
    }
}
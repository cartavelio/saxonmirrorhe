using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class CompareToConstant : Saxon.Hej.expr.UnaryExpression, Saxon.Hej.expr.ComparisonExpression {
        protected internal int @operator;
        public CompareToConstant(Saxon.Hej.expr.Expression p0) : base (p0) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC;
        }
        public Saxon.Hej.expr.Expression getLhsExpression() {
            return getBaseExpression();
        }
        public Saxon.Hej.expr.Operand getLhs() {
            return getOperand();
        }
        public abstract Saxon.Hej.expr.Expression getRhsExpression();
        public Saxon.Hej.expr.Operand getRhs() {
            return new Saxon.Hej.expr.Operand(this, getRhsExpression(), Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public int getComparisonOperator() {
            return @operator;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        protected internal override int computeSpecialProperties() {
            return Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getOperand().optimize(visitor, contextInfo);
            if ((getLhsExpression() is Saxon.Hej.expr.Literal)) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(null)), this);
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public int getSingletonOperator() {
            return @operator;
        }
        public bool convertsUntypedToOther() {
            return true;
        }
        internal bool interpretComparisonResult(int c) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.FEQ:
                return (c == 0);
              case Saxon.Hej.expr.parser.Token.FNE:
                return (c != 0);
              case Saxon.Hej.expr.parser.Token.FGT:
                return (c>0);
              case Saxon.Hej.expr.parser.Token.FLT:
                return (c<0);
              case Saxon.Hej.expr.parser.Token.FGE:
                return (c>=0);
              case Saxon.Hej.expr.parser.Token.FLE:
                return (c<=0);
              default:
                throw new Saxon.Ejava.lang.UnsupportedOperationException(("Unknown operator "+@operator));
            }
        }
    
        /* Creating stub method getAtomicComparer to satisfy interface ComparisonExpression */
        public virtual Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            throw new System.NotImplementedException();
        }

    }
}
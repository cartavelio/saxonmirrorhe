using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class EquivalenceComparison : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.ComparisonExpression {
        private Saxon.Hej.expr.sort.AtomicComparer comparer;
        private bool knownToBeComparable = false;
        public EquivalenceComparison(Saxon.Hej.expr.Expression p1, int @operator, Saxon.Hej.expr.Expression p2) : base (p1, @operator, p2) {
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            string defaultCollationName = env.getDefaultCollationName();
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.lib.StringCollator collation = config.getCollation(defaultCollationName);
            if ((collation == null)) {
                collation = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
            }
            comparer = new Saxon.Hej.expr.EquivalenceComparer(collation, config.getConversionContext());
            Saxon.Hej.expr.Expression oldOp0 = getLhsExpression();
            Saxon.Hej.expr.Expression oldOp1 = getRhsExpression();
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            setLhsExpression(getLhsExpression().unordered(false, false));
            setRhsExpression(getRhsExpression().unordered(false, false));
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC;
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "eq", 0);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), atomicType, role0, visitor));
            Saxon.Hej.expr.parser.RoleDiagnostic role1 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "eq", 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), atomicType, role1, visitor));
            if ((getLhsExpression() != oldOp0)) {
                adoptChildExpression(getLhsExpression());
            }
            if ((getRhsExpression() != oldOp1)) {
                adoptChildExpression(getRhsExpression());
            }
            Saxon.Hej.type.ItemType t0 = getLhsExpression().getItemType();
            Saxon.Hej.type.ItemType t1 = getRhsExpression().getItemType();
            if ((t0 is Saxon.Hej.type.ErrorType)) {
                t0 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            if ((t1 is Saxon.Hej.type.ErrorType)) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            if (t0.getUType().union(t1.getUType()).overlaps(Saxon.Hej.type.UType.EXTENSION)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot perform comparisons involving external objects");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                err.setLocation(getLocation());
                throw err;
            }
            Saxon.Hej.type.BuiltInAtomicType pt0 = (Saxon.Hej.type.BuiltInAtomicType)(t0.getPrimitiveItemType());
            Saxon.Hej.type.BuiltInAtomicType pt1 = (Saxon.Hej.type.BuiltInAtomicType)(t1.getPrimitiveItemType());
            if ((((t0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || t0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
            } else {
                if (Saxon.Hej.type.Type.isGuaranteedComparable(pt0, pt1, false)) {
                    knownToBeComparable = true;
                } else {
                    if (!(Saxon.Hej.type.Type.isPossiblyComparable(pt0, pt1, false))) {
                        env.issueWarning(((("Cannot compare "+t0.ToString())+" to ")+t1.ToString()), getLocation());
                    }
                }
            }
            try {
                if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                    Saxon.Hej.om.GroundedValue v = evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()).materialize();
                    return Saxon.Hej.expr.Literal.makeLiteral(v, this);
                }
            } catch (Saxon.Hej.trans.XPathException err) {
            }
            return this;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            return comparer;
        }
        public int getSingletonOperator() {
            return @operator;
        }
        public bool convertsUntypedToOther() {
            return false;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public bool isKnownToBeComparable() {
            return knownToBeComparable;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getComparer() {
            return comparer;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.EquivalenceComparison sc = new Saxon.Hej.expr.EquivalenceComparison(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, sc);
            sc.comparer = comparer;
            sc.knownToBeComparable = knownToBeComparable;
            return sc;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.value.BooleanValue.get(effectiveBooleanValue(context));
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.AtomicValue v0 = (Saxon.Hej.value.AtomicValue)(getLhsExpression().evaluateItem(context));
            Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
            if (((v0 == null) || (v1 == null))) {
                return ((v0 == v1));
            }
            Saxon.Hej.expr.sort.AtomicComparer comp2 = comparer.provideContext(context);
            return (((knownToBeComparable || Saxon.Hej.type.Type.isGuaranteedComparable(v0.getPrimitiveType(), v1.getPrimitiveType(), false))) && comp2.comparesEqual(v0, v1));
        }
        public override string getExpressionName() {
            return "equivalent";
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.emitAttribute("cardinality", "singleton");
        }
    
    }
}
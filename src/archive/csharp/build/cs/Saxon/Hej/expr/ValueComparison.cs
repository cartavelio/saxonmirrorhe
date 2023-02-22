using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class ValueComparison : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.ComparisonExpression, Saxon.Hej.expr.Negatable {
        private Saxon.Hej.value.BooleanValue resultWhenEmpty = null;
        private bool needsRuntimeCheck;
        public ValueComparison(Saxon.Hej.expr.Expression p1, int op, Saxon.Hej.expr.Expression p2) : base (p1, op, p2) {
        }
        public override string getExpressionName() {
            return "ValueComparison";
        }
        public Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            Saxon.Hej.type.ItemType t0 = getLhsExpression().getItemType().getPrimitiveItemType();
            if (!(((t0 is Saxon.Hej.type.BuiltInAtomicType)))) {
                t0 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            Saxon.Hej.type.ItemType t1 = getRhsExpression().getItemType().getPrimitiveItemType();
            if (!(((t1 is Saxon.Hej.type.BuiltInAtomicType)))) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            return Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer((Saxon.Hej.type.BuiltInAtomicType)(t0), (Saxon.Hej.type.BuiltInAtomicType)(t1), getStringCollator(), getConfiguration().getConversionContext());
        }
        public Saxon.Hej.lib.StringCollator getStringCollator() {
            try {
                return getConfiguration().getCollation((getRetainedStaticContext().getDefaultCollationName()));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e);
            }
        }
        public int getSingletonOperator() {
            return @operator;
        }
        public bool convertsUntypedToOther() {
            return false;
        }
        public void setResultWhenEmpty(Saxon.Hej.value.BooleanValue value) {
            resultWhenEmpty = value;
        }
        public Saxon.Hej.value.BooleanValue getResultWhenEmpty() {
            return resultWhenEmpty;
        }
        public bool needsRuntimeComparabilityCheck() {
            return needsRuntimeCheck;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            resetLocalStaticProperties();
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            if (Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression())) {
                return ((resultWhenEmpty == null) ? getLhsExpression() : Saxon.Hej.expr.Literal.makeLiteral(resultWhenEmpty, this));
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression())) {
                return ((resultWhenEmpty == null) ? getRhsExpression() : Saxon.Hej.expr.Literal.makeLiteral(resultWhenEmpty, this));
            }
            if (convertsUntypedToOther()) {
                return this;
            }
            Saxon.Hej.value.SequenceType optionalAtomic = Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC;
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role0 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), optionalAtomic, role0, visitor));
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role1 = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), optionalAtomic, role1, visitor));
            Saxon.Hej.type.PlainType t0 = getLhsExpression().getItemType().getAtomizedItemType();
            Saxon.Hej.type.PlainType t1 = getRhsExpression().getItemType().getAtomizedItemType();
            if (t0.getUType().union(t1.getUType()).overlaps(Saxon.Hej.type.UType.EXTENSION)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot perform comparisons involving external objects");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                err.setLocation(getLocation());
                throw err;
            }
            Saxon.Hej.type.BuiltInAtomicType p0 = (Saxon.Hej.type.BuiltInAtomicType)(((/*1430*/Saxon.Hej.type.AtomicType)t0.getPrimitiveItemType()));
            if (p0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                p0 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            Saxon.Hej.type.BuiltInAtomicType p1 = (Saxon.Hej.type.BuiltInAtomicType)(((/*1430*/Saxon.Hej.type.AtomicType)t1.getPrimitiveItemType()));
            if (p1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                p1 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            needsRuntimeCheck = (p0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || p1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC));
            if ((!(needsRuntimeCheck) && !(Saxon.Hej.type.Type.isPossiblyComparable(p0, p1, Saxon.Hej.expr.parser.Token.isOrderedOperator(@operator))))) {
                bool opt0 = Saxon.Hej.value.Cardinality.allowsZero(getLhsExpression().getCardinality());
                bool opt1 = Saxon.Hej.value.Cardinality.allowsZero(getRhsExpression().getCardinality());
                if ((opt0 || opt1)) {
                    string which = null;
                    if (opt0) {
                        which = "the first operand is";
                    }
                    if (opt1) {
                        which = "the second operand is";
                    }
                    if ((opt0 && opt1)) {
                        which = "one or both operands are";
                    }
                    visitor.getStaticContext().issueWarning((((((((("Comparison of "+t0)+((opt0 ? "?" : "")))+" to ")+t1)+((opt1 ? "?" : "")))+" will fail unless ")+which)+" empty"), Saxon.Hej.trans.SaxonErrorCode.SXWN9026, getLocation());
                    needsRuntimeCheck = true;
                } else {
                    string message = ((((("In {"+toShortString())+"}: cannot compare ")+t0)+" to ")+t1);
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                    err.setIsTypeError(true);
                    err.setErrorCode("XPTY0004");
                    err.setLocation(getLocation());
                    throw err;
                }
            }
            if (!((((@operator == Saxon.Hej.expr.parser.Token.FEQ) || (@operator == Saxon.Hej.expr.parser.Token.FNE))))) {
                mustBeOrdered(t0, p0);
                mustBeOrdered(t1, p1);
            }
            return this;
        }
        private void mustBeOrdered(Saxon.Hej.type.PlainType t1, Saxon.Hej.type.BuiltInAtomicType p1) {
            if (!(p1.isOrdered(true))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Type "+t1.ToString())+" is not an ordered type"));
                err.setErrorCode("XPTY0004");
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().optimize(visitor, contextInfo);
            getRhs().optimize(visitor, contextInfo);
            return visitor.obtainOptimizer().optimizeValueComparison(this, visitor, contextInfo);
        }
        public bool isNegatable(Saxon.Hej.type.TypeHierarchy th) {
            return (isNeverNaN(getLhsExpression(), th) && isNeverNaN(getRhsExpression(), th));
        }
        private bool isNeverNaN(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.TypeHierarchy th) {
            return ((th.relationship(exp.getItemType(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE) == Saxon.Hej.type.Affinity.DISJOINT) && (th.relationship(exp.getItemType(), Saxon.Hej.type.BuiltInAtomicType.FLOAT) == Saxon.Hej.type.Affinity.DISJOINT));
        }
        public Saxon.Hej.expr.Expression negate() {
            Saxon.Hej.expr.ValueComparison vc = new Saxon.Hej.expr.ValueComparison(getLhsExpression(), Saxon.Hej.expr.parser.Token.negate(@operator), getRhsExpression());
            if (((resultWhenEmpty == null) || (resultWhenEmpty == Saxon.Hej.value.BooleanValue.FALSE))) {
                vc.resultWhenEmpty = Saxon.Hej.value.BooleanValue.TRUE;
            } else {
                vc.resultWhenEmpty = Saxon.Hej.value.BooleanValue.FALSE;
            }
            vc.needsRuntimeCheck = needsRuntimeCheck;
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, vc);
            return vc;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.expr.ValueComparison) && base.Equals(other));
        }
        protected internal override int computeHashCode() {
            return base.computeHashCode();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ValueComparison vc = new Saxon.Hej.expr.ValueComparison(getLhsExpression().copy(rebindings), @operator, getRhsExpression().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, vc);
            vc.resultWhenEmpty = resultWhenEmpty;
            vc.needsRuntimeCheck = needsRuntimeCheck;
            return vc;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForBoolean()(context);
        }
        public static bool compare(Saxon.Hej.value.AtomicValue v0, int op, Saxon.Hej.value.AtomicValue v1, Saxon.Hej.expr.sort.AtomicComparer comparer, bool checkTypes) {
            if ((checkTypes && !(Saxon.Hej.type.Type.isGuaranteedComparable(v0.getPrimitiveType(), v1.getPrimitiveType(), Saxon.Hej.expr.parser.Token.isOrderedOperator(op))))) {
                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(((("Cannot compare "+Saxon.Hej.type.Type.displayTypeName(v0))+" to ")+Saxon.Hej.type.Type.displayTypeName(v1)));
                e2.setErrorCode("XPTY0004");
                e2.setIsTypeError(true);
                throw e2;
            }
            if ((v0.isNaN() || v1.isNaN())) {
                return (op == Saxon.Hej.expr.parser.Token.FNE);
            }
            try {
                switch (op) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                    return comparer.comparesEqual(v0, v1);
                  case Saxon.Hej.expr.parser.Token.FNE:
                    return !(comparer.comparesEqual(v0, v1));
                  case Saxon.Hej.expr.parser.Token.FGT:
                    return (comparer.compareAtomicValues(v0, v1)>0);
                  case Saxon.Hej.expr.parser.Token.FLT:
                    return (comparer.compareAtomicValues(v0, v1)<0);
                  case Saxon.Hej.expr.parser.Token.FGE:
                    return (comparer.compareAtomicValues(v0, v1)>=0);
                  case Saxon.Hej.expr.parser.Token.FLE:
                    return (comparer.compareAtomicValues(v0, v1)<=0);
                  default:
                    throw new Saxon.Ejava.lang.UnsupportedOperationException(("Unknown operator "+op));
                }
            } catch (Saxon.Hej.expr.sort.ComparisonException err) {
                throw err.getReason();
            } catch (System.InvalidCastException err) {
                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(((("Cannot compare "+Saxon.Hej.type.Type.displayTypeName(v0))+" to ")+Saxon.Hej.type.Type.displayTypeName(v1)));
                e2.setErrorCode("XPTY0004");
                e2.setIsTypeError(true);
                throw e2;
            }
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return (Saxon.Hej.value.BooleanValue)(/*Delegate.eval()*/makeElaborator().elaborateForItem()(context));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        protected internal override int computeCardinality() {
            if ((resultWhenEmpty != null)) {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            } else {
                return base.computeCardinality();
            }
        }
        protected internal override string tag() {
            return "vc";
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            if ((resultWhenEmpty != null)) {
                @out.emitAttribute("onEmpty", (resultWhenEmpty.getBooleanValue() ? "1" : "0"));
            }
            if (("JS".Equals(@out.getOptions().target) && (@out.getOptions().targetVersion == 2))) {
                Saxon.Hej.expr.sort.AtomicComparer comparer = getAtomicComparer();
                @out.emitAttribute("comp", comparer.save());
            }
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.ValueComparison.ValueComparisonElaborator();
        }
        public class ValueComparisonElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.ValueComparison expr = (Saxon.Hej.expr.ValueComparison)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator p0 = expr.getLhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator p1 = expr.getRhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.value.BooleanValue resultWhenEmpty = expr.getResultWhenEmpty();
                Saxon.Hej.lib.StringCollator defaultCollation;
                try {
                    defaultCollation = expr.getConfiguration().getCollation(expr.getRetainedStaticContext().getDefaultCollationName());
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.IllegalStateException(("Unknown default collation in static context: "+expr.getRetainedStaticContext().getDefaultCollationName()));
                }
                int @operator = expr.getOperator();
                int card0 = expr.getLhsExpression().getCardinality();
                int card1 = expr.getRhsExpression().getCardinality();
                if (((card0 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO) || (card1 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO))) {
                    return context => resultWhenEmpty;
                }
                Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction comparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparisonFunction(operandType(expr.getLhsExpression()), operandType(expr.getRhsExpression()), defaultCollation, @operator, true);
                bool nullable0 = Saxon.Hej.value.Cardinality.allowsZero(card0);
                bool nullable1 = Saxon.Hej.value.Cardinality.allowsZero(card1);
                if ((!(nullable0) && !(nullable1))) {
                    return context => Saxon.Hej.value.BooleanValue.get(/*Delegate.compare()*/comparer((Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p0(context)), (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p1(context)), context));
                } else {
                    return context => {
                        Saxon.Hej.value.AtomicValue v0 = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p0(context));
                        if ((v0 == null)) {
                            return resultWhenEmpty;
                        }
                        Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p1(context));
                        if ((v1 == null)) {
                            return resultWhenEmpty;
                        }
                        return Saxon.Hej.value.BooleanValue.get(/*Delegate.compare()*/comparer(v0, v1, context));
                    } ;
                }
            }
            private Saxon.Hej.type.BuiltInAtomicType operandType(Saxon.Hej.expr.Expression operand) {
                Saxon.Hej.type.ItemType type = operand.getItemType();
                if ((type == Saxon.Hej.type.AnyItemType.getInstance())) {
                    return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                } else {
                    return (Saxon.Hej.type.BuiltInAtomicType)(type.getPrimitiveItemType());
                }
            }
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.expr.ValueComparison expr = (Saxon.Hej.expr.ValueComparison)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator p0 = expr.getLhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator p1 = expr.getRhsExpression().makeElaborator().elaborateForItem();
                Saxon.Hej.lib.StringCollator defaultCollation;
                try {
                    defaultCollation = expr.getConfiguration().getCollation(expr.getRetainedStaticContext().getDefaultCollationName());
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.IllegalStateException(("Unknown default collation in static context: "+expr.getRetainedStaticContext().getDefaultCollationName()));
                }
                int @operator = expr.getOperator();
                bool resultWhenEmpty = ((expr.getResultWhenEmpty() != null) && expr.getResultWhenEmpty().getBooleanValue());
                int card0 = expr.getLhsExpression().getCardinality();
                int card1 = expr.getRhsExpression().getCardinality();
                if (((card0 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO) || (card1 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO))) {
                    return context => resultWhenEmpty;
                }
                Saxon.Hej.type.ItemType t0 = expr.getLhsExpression().getItemType().getPrimitiveItemType();
                if (!(((t0 is Saxon.Hej.type.BuiltInAtomicType)))) {
                    t0 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                }
                Saxon.Hej.type.ItemType t1 = expr.getRhsExpression().getItemType().getPrimitiveItemType();
                if (!(((t1 is Saxon.Hej.type.BuiltInAtomicType)))) {
                    t1 = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                }
                Saxon.Hej.expr.sort.GenericAtomicComparer.AtomicComparisonFunction comparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparisonFunction((Saxon.Hej.type.BuiltInAtomicType)(t0), (Saxon.Hej.type.BuiltInAtomicType)(t1), defaultCollation, @operator, true);
                bool nullable0 = Saxon.Hej.value.Cardinality.allowsZero(card0);
                bool nullable1 = Saxon.Hej.value.Cardinality.allowsZero(card1);
                if ((!(nullable0) && !(nullable1))) {
                    return context => /*Delegate.compare()*/comparer((Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p0(context)), (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p1(context)), context);
                } else {
                    return context => {
                        Saxon.Hej.value.AtomicValue v0 = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p0(context));
                        if ((v0 == null)) {
                            return resultWhenEmpty;
                        }
                        Saxon.Hej.value.AtomicValue v1 = (Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/p1(context));
                        if ((v1 == null)) {
                            return resultWhenEmpty;
                        }
                        return /*Delegate.compare()*/comparer(v0, v1, context);
                    } ;
                }
            }
        
        }
    
    }
}
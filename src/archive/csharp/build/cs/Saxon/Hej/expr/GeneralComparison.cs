using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class GeneralComparison : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.ComparisonExpression, Saxon.Hej.expr.Callable {
        public enum ComparisonCardinality {
            ONE_TO_ONE,
            MANY_TO_ONE,
            MANY_TO_MANY,
        }
        protected internal int singletonOperator;
        protected internal Saxon.Hej.expr.sort.AtomicComparer comparer;
        protected internal bool runtimeCheckNeeded = true;
        protected internal Saxon.Hej.expr.GeneralComparison.ComparisonCardinality comparisonCardinality = Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY;
        protected internal bool doneWarnings = false;
        public GeneralComparison(Saxon.Hej.expr.Expression p0, int op, Saxon.Hej.expr.Expression p1) : base (p0, op, p1) {
            singletonOperator = getCorrespondingSingletonOperator(op);
        }
        public bool needsRuntimeCheck() {
            return runtimeCheckNeeded;
        }
        public void setNeedsRuntimeCheck(bool needsCheck) {
            runtimeCheckNeeded = needsCheck;
        }
        public Saxon.Hej.expr.GeneralComparison.ComparisonCardinality getComparisonCardinality() {
            return comparisonCardinality;
        }
        public void setComparisonCardinality(Saxon.Hej.expr.GeneralComparison.ComparisonCardinality card) {
            comparisonCardinality = card;
        }
        public void setAtomicComparer(Saxon.Hej.expr.sort.AtomicComparer comparer) {
            this.comparer = comparer;
        }
        public override string getExpressionName() {
            return "GeneralComparison";
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return getRetainedStaticContext();
        }
        public Saxon.Hej.expr.sort.AtomicComparer getAtomicComparer() {
            return comparer;
        }
        public int getSingletonOperator() {
            return singletonOperator;
        }
        public bool convertsUntypedToOther() {
            return true;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.Expression oldOp0 = getLhsExpression();
            Saxon.Hej.expr.Expression oldOp1 = getRhsExpression();
            getLhs().typeCheck(visitor, contextInfo);
            getRhs().typeCheck(visitor, contextInfo);
            if ((Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression()))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            }
            setLhsExpression(getLhsExpression().unordered(false, false));
            setRhsExpression(getRhsExpression().unordered(false, false));
            Saxon.Hej.value.SequenceType atomicType = Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE;
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 0);
            setLhsExpression(tc.staticTypeCheck(getLhsExpression(), atomicType, role0, visitor));
            Saxon.Hej.expr.parser.RoleDiagnostic role1 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, Saxon.Hej.expr.parser.Token.tokens[@operator], 1);
            setRhsExpression(tc.staticTypeCheck(getRhsExpression(), atomicType, role1, visitor));
            if ((getLhsExpression() != oldOp0)) {
                adoptChildExpression(getLhsExpression());
            }
            if ((getRhsExpression() != oldOp1)) {
                adoptChildExpression(getRhsExpression());
            }
            Saxon.Hej.type.ItemType t0 = getLhsExpression().getItemType();
            Saxon.Hej.type.ItemType t1 = getRhsExpression().getItemType();
            if (((t0 is Saxon.Hej.type.ErrorType) || (t1 is Saxon.Hej.type.ErrorType))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            }
            if (t0.getUType().union(t1.getUType()).overlaps(Saxon.Hej.type.UType.EXTENSION)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Cannot perform comparisons involving external objects");
                err.setIsTypeError(true);
                err.setErrorCode("XPTY0004");
                err.setLocator(getLocation());
                throw err;
            }
            Saxon.Hej.type.BuiltInAtomicType pt0 = (Saxon.Hej.type.BuiltInAtomicType)(t0.getPrimitiveItemType());
            Saxon.Hej.type.BuiltInAtomicType pt1 = (Saxon.Hej.type.BuiltInAtomicType)(t1.getPrimitiveItemType());
            int c0 = getLhsExpression().getCardinality();
            int c1 = getRhsExpression().getCardinality();
            if (((c0 == Saxon.Hej.expr.StaticProperty.EMPTY) || (c1 == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            }
            if ((((t0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || t0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
            } else {
                if (!(Saxon.Hej.type.Type.isPossiblyComparable(pt0, pt1, Saxon.Hej.expr.parser.Token.isOrderedOperator(singletonOperator)))) {
                    string message = ((((("In {"+toShortString())+"}: cannot compare ")+t0)+" to ")+t1);
                    if ((Saxon.Hej.value.Cardinality.allowsZero(c0) || Saxon.Hej.value.Cardinality.allowsZero(c1))) {
                        if (!(doneWarnings)) {
                            doneWarnings = true;
                            string which = "one";
                            if ((Saxon.Hej.value.Cardinality.allowsZero(c0) && !(Saxon.Hej.value.Cardinality.allowsZero(c1)))) {
                                which = "the first";
                            } else {
                                if ((Saxon.Hej.value.Cardinality.allowsZero(c1) && !(Saxon.Hej.value.Cardinality.allowsZero(c0)))) {
                                    which = "the second";
                                }
                            }
                            visitor.getStaticContext().issueWarning((((message+". The comparison can succeed only if ")+which)+" operand is empty, and in that case will always be false"), getLocation());
                        }
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                        err.setErrorCode("XPTY0004");
                        err.setIsTypeError(true);
                        err.setLocator(getLocation());
                        throw err;
                    }
                }
            }
            runtimeCheckNeeded = !(Saxon.Hej.type.Type.isGuaranteedGenerallyComparable(pt0, pt1, Saxon.Hej.expr.parser.Token.isOrderedOperator(singletonOperator)));
            if ((((!(Saxon.Hej.value.Cardinality.allowsMany(c0)) && !(Saxon.Hej.value.Cardinality.allowsMany(c1))) && !(t0.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))) && !(t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)))) {
                Saxon.Hej.expr.Expression e0 = getLhsExpression();
                Saxon.Hej.expr.Expression e1 = getRhsExpression();
                if (t0.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                        e0 = new Saxon.Hej.expr.CastExpression(getLhsExpression(), Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.Cardinality.allowsZero(c0));
                        adoptChildExpression(e0);
                        e1 = new Saxon.Hej.expr.CastExpression(getRhsExpression(), Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.Cardinality.allowsZero(c1));
                        adoptChildExpression(e1);
                    } else {
                        if (Saxon.Hej.type.NumericType.isNumericType(t1)) {
                            Saxon.Hej.expr.Expression vun = makeCompareUntypedToNumeric(getLhsExpression(), getRhsExpression(), singletonOperator);
                            return vun.typeCheck(visitor, contextInfo);
                        } else {
                            e0 = new Saxon.Hej.expr.CastExpression(getLhsExpression(), pt1, Saxon.Hej.value.Cardinality.allowsZero(c0));
                            adoptChildExpression(e0);
                        }
                    }
                } else {
                    if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                        if (Saxon.Hej.type.NumericType.isNumericType(t0)) {
                            Saxon.Hej.expr.Expression vun = makeCompareUntypedToNumeric(getRhsExpression(), getLhsExpression(), Saxon.Hej.expr.parser.Token.inverse(singletonOperator));
                            return vun.typeCheck(visitor, contextInfo);
                        } else {
                            e1 = new Saxon.Hej.expr.CastExpression(getRhsExpression(), pt0, Saxon.Hej.value.Cardinality.allowsZero(c1));
                            adoptChildExpression(e1);
                        }
                    }
                }
                Saxon.Hej.expr.ValueComparison vc = new Saxon.Hej.expr.ValueComparison(e0, singletonOperator, e1);
                vc.setAtomicComparer(comparer);
                vc.setResultWhenEmpty(Saxon.Hej.value.BooleanValue.FALSE);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, vc);
                Saxon.Hej.expr.parser.Optimizer.trace(config, "Replaced general comparison by value comparison", vc);
                return vc.typeCheck(visitor, contextInfo);
            }
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            if ((comparer == null)) {
                string defaultCollationName = env.getDefaultCollationName();
                Saxon.Hej.lib.StringCollator collation = config.getCollation(defaultCollationName);
                if ((collation == null)) {
                    collation = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
                }
                comparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer(pt0, pt1, collation, config.getConversionContext());
            }
            if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(env.makeEarlyEvaluationContext()), this);
            }
            return this;
        }
        private Saxon.Hej.expr.Expression makeCompareUntypedToNumeric(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs, int @operator) {
            Saxon.Hej.expr.ValueComparison vc = new Saxon.Hej.expr.ValueComparison(lhs, @operator, rhs);
            vc.setAtomicComparer(new Saxon.Hej.expr.sort.UntypedNumericComparer());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, vc);
            Saxon.Hej.expr.parser.Optimizer.trace(getConfiguration(), "Replaced general comparison by untyped-numeric value comparison", vc);
            return vc;
        }
        private static Saxon.Hej.expr.Expression makeMinOrMax(Saxon.Hej.expr.Expression exp, string function) {
            if (Saxon.Hej.value.Cardinality.allowsMany(exp.getCardinality())) {
                Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall(function, exp.getRetainedStaticContext(), exp);
                System.Diagnostics.Debug.Assert((fn != null));
                ((Saxon.Hej.functions.Minimax)(((Saxon.Hej.expr.SystemFunctionCall)(fn)).getTargetFunction())).setIgnoreNaN(true);
                return fn;
            } else {
                return exp;
            }
        }
        public override int getIntrinsicDependencies() {
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            if ((mayInvolveCastToQName(th, getLhsExpression(), getRhsExpression()) || mayInvolveCastToQName(th, getRhsExpression(), getLhsExpression()))) {
                return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT;
            } else {
                return 0;
            }
        }
        private bool mayInvolveCastToQName(Saxon.Hej.type.TypeHierarchy th, Saxon.Hej.expr.Expression e1, Saxon.Hej.expr.Expression e2) {
            Saxon.Hej.type.SimpleType s1 = (Saxon.Hej.type.SimpleType)(e1.getItemType().getAtomizedItemType());
            return (((((s1 == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || s1.isNamespaceSensitive())) && (th.relationship(e2.getItemType().getAtomizedItemType(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) != Saxon.Hej.type.Affinity.DISJOINT)) && (((e2.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC)) == 0));
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.GeneralComparison) && base.Equals(other)) && comparer.Equals(((Saxon.Hej.expr.GeneralComparison)(other)).comparer));
        }
        protected internal override int computeHashCode() {
            return base.computeHashCode();
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
            getLhs().optimize(visitor, contextInfo);
            getRhs().optimize(visitor, contextInfo);
            if ((Saxon.Hej.expr.Literal.isEmptySequence(getLhsExpression()) || Saxon.Hej.expr.Literal.isEmptySequence(getRhsExpression()))) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE, this);
            }
            setLhsExpression(getLhsExpression().unordered(false, false));
            setRhsExpression(getRhsExpression().unordered(false, false));
            if (((getLhsExpression() is Saxon.Hej.expr.Literal) && (getRhsExpression() is Saxon.Hej.expr.Literal))) {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(visitor.getStaticContext().makeEarlyEvaluationContext()).materialize(), this);
            }
            Saxon.Hej.type.ItemType t0 = getLhsExpression().getItemType();
            Saxon.Hej.type.ItemType t1 = getRhsExpression().getItemType();
            int c0 = getLhsExpression().getCardinality();
            int c1 = getRhsExpression().getCardinality();
            bool many0 = Saxon.Hej.value.Cardinality.allowsMany(c0);
            bool many1 = Saxon.Hej.value.Cardinality.allowsMany(c1);
            if (many0) {
                if (many1) {
                    comparisonCardinality = Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY;
                } else {
                    comparisonCardinality = Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE;
                }
            } else {
                if (many1) {
                    Saxon.Hej.expr.GeneralComparison mc = getInverseComparison();
                    mc.comparisonCardinality = Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE;
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, mc);
                    mc.comparer = comparer;
                    mc.runtimeCheckNeeded = runtimeCheckNeeded;
                    return mc.optimize(visitor, contextInfo);
                } else {
                    comparisonCardinality = Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.ONE_TO_ONE;
                }
            }
            if (((((@operator != Saxon.Hej.expr.parser.Token.EQUALS) && (@operator != Saxon.Hej.expr.parser.Token.NE)) && (((comparisonCardinality == Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY) || ((comparisonCardinality == Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE) && ((manyOperandIsLiftable() || manyOperandIsRangeExpression())))))) && ((Saxon.Hej.type.NumericType.isNumericType(t0) || Saxon.Hej.type.NumericType.isNumericType(t1))))) {
                Saxon.Hej.expr.ValueComparison vc;
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.LT:
                  case Saxon.Hej.expr.parser.Token.LE:
                    vc = new Saxon.Hej.expr.ValueComparison(makeMinOrMax(getLhsExpression(), "min"), singletonOperator, makeMinOrMax(getRhsExpression(), "max"));
                    vc.setResultWhenEmpty(Saxon.Hej.value.BooleanValue.FALSE);
                    vc.setAtomicComparer(comparer);
                    break ;
                  case Saxon.Hej.expr.parser.Token.GT:
                  case Saxon.Hej.expr.parser.Token.GE:
                    vc = new Saxon.Hej.expr.ValueComparison(makeMinOrMax(getLhsExpression(), "max"), singletonOperator, makeMinOrMax(getRhsExpression(), "min"));
                    vc.setResultWhenEmpty(Saxon.Hej.value.BooleanValue.FALSE);
                    vc.setAtomicComparer(comparer);
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.UnsupportedOperationException(("Unknown operator "+@operator));
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, vc);
                vc.setRetainedStaticContext(getRetainedStaticContext());
                return vc.typeCheck(visitor, contextInfo);
            }
            if ((((getLhsExpression() is Saxon.Hej.expr.Literal)) && ((getRhsExpression() is Saxon.Hej.expr.Literal)))) {
                return Saxon.Hej.expr.Literal.makeLiteral(evaluateItem(env.makeEarlyEvaluationContext()), this);
            }
            return visitor.obtainOptimizer().optimizeGeneralComparison(visitor, this, false, contextInfo);
        }
        private bool manyOperandIsLiftable() {
            if (((getParentExpression() is Saxon.Hej.expr.ContextSwitchingExpression) && (((Saxon.Hej.expr.ContextSwitchingExpression)(getParentExpression())).getActionExpression() == this))) {
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    if (Saxon.Hej.value.Cardinality.allowsMany(o.getChildExpression().getCardinality())) {
                        if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(o.getChildExpression())) {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }
        private bool manyOperandIsRangeExpression() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression e = o.getChildExpression();
                if (Saxon.Hej.value.Cardinality.allowsMany(e.getCardinality())) {
                    return (((e is Saxon.Hej.expr.RangeExpression) || ((e is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(e)).getGroundedValue() is Saxon.Hej.value.IntegerRange))));
                }
            }
            return false;
        }
        public override Saxon.Hej.value.BooleanValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            switch (comparisonCardinality) {
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.ONE_TO_ONE:
                {
                    Saxon.Hej.value.AtomicValue value0 = (Saxon.Hej.value.AtomicValue)(getLhsExpression().evaluateItem(context));
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                    return Saxon.Hej.value.BooleanValue.get(evaluateOneToOne(value0, value1, context));
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE:
                {
                    Saxon.Hej.om.SequenceIterator iter0 = getLhsExpression().iterate(context);
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                    return Saxon.Hej.value.BooleanValue.get(evaluateManyToOne(iter0, value1, context));
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY:
                {
                    Saxon.Hej.om.SequenceIterator iter1 = getLhsExpression().iterate(context);
                    Saxon.Hej.om.SequenceIterator iter2 = getRhsExpression().iterate(context);
                    return Saxon.Hej.value.BooleanValue.get(evaluateManyToMany(iter1, iter2, context));
                }
            }
            return null;
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            switch (comparisonCardinality) {
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.ONE_TO_ONE:
                {
                    Saxon.Hej.value.AtomicValue value0 = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                    return Saxon.Hej.value.BooleanValue.get(evaluateOneToOne(value0, value1, context));
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE:
                {
                    Saxon.Hej.om.SequenceIterator iter0 = arguments[0].iterate();
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(arguments[1].head());
                    return Saxon.Hej.value.BooleanValue.get(evaluateManyToOne(iter0, value1, context));
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY:
                {
                    Saxon.Hej.om.SequenceIterator iter1 = arguments[0].iterate();
                    Saxon.Hej.om.SequenceIterator iter2 = arguments[1].iterate();
                    return Saxon.Hej.value.BooleanValue.get(evaluateManyToMany(iter1, iter2, context));
                }
            }
            return null;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            switch (comparisonCardinality) {
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.ONE_TO_ONE:
                {
                    Saxon.Hej.value.AtomicValue value0 = (Saxon.Hej.value.AtomicValue)(getLhsExpression().evaluateItem(context));
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                    return evaluateOneToOne(value0, value1, context);
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE:
                {
                    Saxon.Hej.om.SequenceIterator iter0 = getLhsExpression().iterate(context);
                    Saxon.Hej.value.AtomicValue value1 = (Saxon.Hej.value.AtomicValue)(getRhsExpression().evaluateItem(context));
                    return evaluateManyToOne(iter0, value1, context);
                }
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY:
                {
                    Saxon.Hej.om.SequenceIterator iter1 = getLhsExpression().iterate(context);
                    Saxon.Hej.om.SequenceIterator iter2 = getRhsExpression().iterate(context);
                    return evaluateManyToMany(iter1, iter2, context);
                }
            }
            return false;
        }
        private bool evaluateOneToOne(Saxon.Hej.value.AtomicValue value0, Saxon.Hej.value.AtomicValue value1, Saxon.Hej.expr.XPathContext context) {
            try {
                return (!((((value0 == null) || (value1 == null)))) && compare(value0, singletonOperator, value1, comparer.provideContext(context), runtimeCheckNeeded, context, getRetainedStaticContext()));
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        private bool evaluateManyToOne(Saxon.Hej.om.SequenceIterator iter0, Saxon.Hej.value.AtomicValue value1, Saxon.Hej.expr.XPathContext context) {
            try {
                if ((value1 == null)) {
                    return false;
                }
                if ((iter0 is Saxon.Hej.tree.iter.RangeIterator)) {
                    if ((value1 is Saxon.Hej.value.UntypedAtomicValue)) {
                        value1 = Saxon.Hej.type.StringConverter.StringToInteger.INSTANCE.convertString(value1.getUnicodeStringValue()).asAtomic();
                    }
                    Saxon.Hej.tree.iter.RangeIterator ri = (Saxon.Hej.tree.iter.RangeIterator)(iter0);
                    switch (singletonOperator) {
                      case Saxon.Hej.expr.parser.Token.FEQ:
                        return ri.containsEq((Saxon.Hej.value.NumericValue)(value1));
                      case Saxon.Hej.expr.parser.Token.FNE:
                        return ((ri.getFirst().compareTo(ri.getLast()) != 0) || (ri.getFirst().compareTo(((Saxon.Hej.value.NumericValue)(value1))) != 0));
                      case Saxon.Hej.expr.parser.Token.FLE:
                        return (ri.getMin().compareTo(((Saxon.Hej.value.NumericValue)(value1)))<=0);
                      case Saxon.Hej.expr.parser.Token.FLT:
                        return (ri.getMin().compareTo(((Saxon.Hej.value.NumericValue)(value1)))<0);
                      case Saxon.Hej.expr.parser.Token.FGE:
                        return (ri.getMax().compareTo(((Saxon.Hej.value.NumericValue)(value1)))>=0);
                      case Saxon.Hej.expr.parser.Token.FGT:
                        return (ri.getMax().compareTo(((Saxon.Hej.value.NumericValue)(value1)))>0);
                      default:
                        throw new Saxon.Ejava.lang.AssertionError();
                    }
                }
                Saxon.Hej.value.AtomicValue item0;
                Saxon.Hej.expr.sort.AtomicComparer boundComparer = comparer.provideContext(context);
                while (((item0 = (Saxon.Hej.value.AtomicValue)(iter0.next())) != null)) {
                    if (compare(item0, singletonOperator, value1, boundComparer, runtimeCheckNeeded, context, getRetainedStaticContext())) {
                        iter0.close();
                        return true;
                    }
                }
                return false;
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        public bool evaluateManyToMany(Saxon.Hej.om.SequenceIterator iter0, Saxon.Hej.om.SequenceIterator iter1, Saxon.Hej.expr.XPathContext context) {
            try {
                bool exhausted0 = false;
                bool exhausted1 = false;
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> value0 = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> value1 = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                Saxon.Hej.expr.sort.AtomicComparer boundComparer = comparer.provideContext(context);
                while (true) {
                    if (!(exhausted0)) {
                        Saxon.Hej.value.AtomicValue item0 = (Saxon.Hej.value.AtomicValue)(iter0.next());
                        if ((item0 == null)) {
                            if (exhausted1) {
                                return false;
                            }
                            exhausted0 = true;
                        } else {
                            foreach (Saxon.Hej.value.AtomicValue item1 in value1) {
                                if (compare(item0, singletonOperator, item1, boundComparer, runtimeCheckNeeded, context, getRetainedStaticContext())) {
                                    iter0.close();
                                    iter1.close();
                                    return true;
                                }
                            }
                            if (!(exhausted1)) {
                                value0.Add(item0);
                            }
                        }
                    }
                    if (!(exhausted1)) {
                        Saxon.Hej.value.AtomicValue item1 = (Saxon.Hej.value.AtomicValue)(iter1.next());
                        if ((item1 == null)) {
                            if (exhausted0) {
                                return false;
                            }
                            exhausted1 = true;
                        } else {
                            foreach (Saxon.Hej.value.AtomicValue item0 in value0) {
                                if (compare(item0, singletonOperator, item1, boundComparer, runtimeCheckNeeded, context, getRetainedStaticContext())) {
                                    iter0.close();
                                    iter1.close();
                                    return true;
                                }
                            }
                            if (!(exhausted0)) {
                                value1.Add(item1);
                            }
                        }
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        public static bool compare(Saxon.Hej.value.AtomicValue a0, int @operator, Saxon.Hej.value.AtomicValue a1, Saxon.Hej.expr.sort.AtomicComparer comparer, bool checkTypes, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NamespaceResolver nsResolver) {
            bool u0 = a0.isUntypedAtomic();
            bool u1 = a1.isUntypedAtomic();
            if ((u0 != u1)) {
                Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
                if (u0) {
                    if ((a1 is Saxon.Hej.value.NumericValue)) {
                        return Saxon.Hej.expr.sort.UntypedNumericComparer.quickCompare((Saxon.Hej.value.StringValue)(a0), (Saxon.Hej.value.NumericValue)(a1), @operator, rules);
                    } else {
                        if ((a1 is Saxon.Hej.value.StringValue)) {
                        } else {
                            Saxon.Hej.type.AtomicType prim = a1.getPrimitiveType();
                            Saxon.Hej.type.StringConverter sc = prim.getStringConverter(rules);
                            if ((a1 is Saxon.Hej.value.QualifiedNameValue)) {
                                sc = (Saxon.Hej.type.StringConverter)(sc.setNamespaceResolver(nsResolver));
                            }
                            a0 = sc.convertString(a0.getUnicodeStringValue()).asAtomic();
                        }
                    }
                } else {
                    if ((a0 is Saxon.Hej.value.NumericValue)) {
                        return Saxon.Hej.expr.sort.UntypedNumericComparer.quickCompare((Saxon.Hej.value.StringValue)(a1), (Saxon.Hej.value.NumericValue)(a0), Saxon.Hej.expr.parser.Token.inverse(@operator), rules);
                    } else {
                        if ((a0 is Saxon.Hej.value.StringValue)) {
                        } else {
                            Saxon.Hej.type.AtomicType prim = a0.getPrimitiveType();
                            Saxon.Hej.type.StringConverter sc = prim.getStringConverter(rules);
                            if ((a0 is Saxon.Hej.value.QualifiedNameValue)) {
                                sc = (Saxon.Hej.type.StringConverter)(sc.setNamespaceResolver(nsResolver));
                            }
                            a1 = sc.convertString(a1.getUnicodeStringValue()).asAtomic();
                        }
                    }
                }
                checkTypes = false;
            }
            return Saxon.Hej.expr.ValueComparison.compare(a0, @operator, a1, comparer, checkTypes);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.BOOLEAN;
        }
        public static int getCorrespondingSingletonOperator(int op) {
            switch (op) {
              case Saxon.Hej.expr.parser.Token.EQUALS:
                return Saxon.Hej.expr.parser.Token.FEQ;
              case Saxon.Hej.expr.parser.Token.GE:
                return Saxon.Hej.expr.parser.Token.FGE;
              case Saxon.Hej.expr.parser.Token.NE:
                return Saxon.Hej.expr.parser.Token.FNE;
              case Saxon.Hej.expr.parser.Token.LT:
                return Saxon.Hej.expr.parser.Token.FLT;
              case Saxon.Hej.expr.parser.Token.GT:
                return Saxon.Hej.expr.parser.Token.FGT;
              case Saxon.Hej.expr.parser.Token.LE:
                return Saxon.Hej.expr.parser.Token.FLE;
              default:
                return op;
            }
        }
        protected internal virtual Saxon.Hej.expr.GeneralComparison getInverseComparison() {
            Saxon.Hej.expr.GeneralComparison20 gc2 = new Saxon.Hej.expr.GeneralComparison20(getRhsExpression(), Saxon.Hej.expr.parser.Token.inverse(@operator), getLhsExpression());
            gc2.setRetainedStaticContext(getRetainedStaticContext());
            return gc2;
        }
        public override string getStreamerName() {
            return "GeneralComparison";
        }
        protected internal override string tag() {
            return "gc";
        }
        protected internal override void explainExtraAttributes(Saxon.Hej.trace.ExpressionPresenter @out) {
            string cc = "";
            switch (comparisonCardinality) {
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.ONE_TO_ONE:
                cc = "1:1";
                break ;
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_ONE:
                cc = "N:1";
                break ;
              case Saxon.Hej.expr.GeneralComparison.ComparisonCardinality.MANY_TO_MANY:
                cc = "M:N";
                break ;
            }
            @out.emitAttribute("card", cc);
            @out.emitAttribute("comp", comparer.save());
        }
    
    }
}
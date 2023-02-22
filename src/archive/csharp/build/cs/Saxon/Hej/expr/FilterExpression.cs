using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class FilterExpression : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.ContextSwitchingExpression {
        private bool filterIsPositional;
        private bool filterIsSingletonBoolean;
        private bool filterIsIndependent;
        public bool doneReorderingPredicates = false;
        private bool indexingDisabled;
        public const int FILTERED = 10000;
        public readonly static Saxon.Hej.expr.OperandRole FILTER_PREDICATE = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.USES_NEW_FOCUS|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.INSPECTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public FilterExpression(Saxon.Hej.expr.Expression @base, Saxon.Hej.expr.Expression filter) : base (@base, Saxon.Hej.expr.parser.Token.LSQB, filter) {
            @base.setFiltered(true);
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return ((arg == 0) ? Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION : FILTER_PREDICATE);
        }
        public Saxon.Hej.expr.Expression getBase() {
            return getLhsExpression();
        }
        public void setBase(Saxon.Hej.expr.Expression @base) {
            setLhsExpression(@base);
        }
        public void disableIndexing() {
            indexingDisabled = true;
        }
        public Saxon.Hej.expr.Expression getFilter() {
            return getRhsExpression();
        }
        public void setFilter(Saxon.Hej.expr.Expression filter) {
            setRhsExpression(filter);
        }
        public override string getExpressionName() {
            return "filter";
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if (((getFilter() is Saxon.Hej.expr.InstanceOfExpression) && (((Saxon.Hej.expr.InstanceOfExpression)(getFilter())).getBaseExpression() is Saxon.Hej.expr.ContextItemExpression))) {
                return ((Saxon.Hej.expr.InstanceOfExpression)(getFilter())).getRequiredItemType();
            }
            return getBase().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getBase().getStaticUType(contextItemType);
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return getBase();
        }
        public bool isFilterIsPositional() {
            return filterIsPositional;
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return getFilter();
        }
        public bool isPositional(Saxon.Hej.type.TypeHierarchy th) {
            return isPositionalFilter(getFilter(), th);
        }
        public bool isSimpleBooleanFilter() {
            return filterIsSingletonBoolean;
        }
        public bool isIndependentFilter() {
            return filterIsIndependent;
        }
        public override Saxon.Hej.expr.Expression simplify() {
            setBase(getBase().simplify());
            setFilter(getFilter().simplify());
            if (Saxon.Hej.expr.Literal.isEmptySequence(getBase())) {
                return getBase();
            }
            if (((getFilter() is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue() is Saxon.Hej.value.NumericValue))))) {
                try {
                    if (getFilter().effectiveBooleanValue(new Saxon.Hej.expr.EarlyEvaluationContext(getConfiguration()))) {
                        return getBase();
                    } else {
                        return Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(getLocation());
                    throw;
                }
            }
            if (getFilter().isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Last))) {
                setFilter(new Saxon.Hej.expr.IsLastExpression(true));
                adoptChildExpression(getFilter());
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            getLhs().typeCheck(visitor, contextInfo);
            getBase().setFiltered(true);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getBase())) {
                return getBase();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo baseItemType = config.makeContextItemStaticInfo(getSelectExpression().getItemType(), false);
            baseItemType.setContextSettingExpression(getBase());
            getRhs().typeCheck(visitor, baseItemType);
            Saxon.Hej.expr.Expression filter2 = Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(getFilter(), visitor.isOptimizeForStreaming());
            if ((filter2 != getFilter())) {
                setFilter(filter2);
            }
            if (Saxon.Hej.expr.Literal.isConstantOne(getFilter())) {
                Saxon.Hej.expr.Expression fie = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(getBase());
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fie);
                return fie;
            }
            filterIsPositional = isPositionalFilter(getFilter(), th);
            filterIsSingletonBoolean = ((getFilter().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && getFilter().getItemType().Equals(Saxon.Hej.type.BuiltInAtomicType.BOOLEAN));
            filterIsIndependent = (((getFilter().getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) == 0);
            Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(this);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((visitor.isOptimizeForStreaming() && (getSelectExpression() is Saxon.Hej.expr.GlobalVariableReference))) {
                visitor.setOptimizeForStreaming(false);
                Saxon.Hej.expr.Expression eOpt = optimize(visitor, contextItemType);
                visitor.setOptimizeForStreaming(true);
                return eOpt;
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            bool tracing = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            getLhs().optimize(visitor, contextItemType);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getSelectExpression())) {
                return getSelectExpression();
            }
            getBase().setFiltered(true);
            Saxon.Hej.expr.parser.ContextItemStaticInfo baseItemType = config.makeContextItemStaticInfo(getSelectExpression().getItemType(), false);
            baseItemType.setContextSettingExpression(getBase());
            getRhs().optimize(visitor, baseItemType);
            Saxon.Hej.expr.Expression filter2 = Saxon.Hej.expr.parser.ExpressionTool.unsortedIfHomogeneous(getFilter(), visitor.isOptimizeForStreaming());
            if ((filter2 != getFilter())) {
                setFilter(filter2);
            }
            if (((((getFilter() is Saxon.Hej.expr.IsLastExpression) && ((Saxon.Hej.expr.IsLastExpression)(getFilter())).getCondition()) && (getBase() is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(getBase())).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))) {
                Saxon.Hej.pattern.NodeTest test = ((Saxon.Hej.expr.AxisExpression)(getBase())).getNodeTest();
                Saxon.Hej.expr.AxisExpression fs = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING, test);
                setFilter(Saxon.Hej.functions.SystemFunction.makeCall("empty", getRetainedStaticContext(), fs));
                if (tracing) {
                    Saxon.Hej.expr.parser.Optimizer.trace(config, "Replaced [last()] predicate by test for following-sibling", this);
                }
            }
            if (((((((getBase() is Saxon.Hej.expr.AxisExpression) && (((Saxon.Hej.expr.AxisExpression)(getBase())).getNodeTest() == Saxon.Hej.pattern.NodeKindTest.ELEMENT)) && (getFilter() is Saxon.Hej.expr.CompareToStringConstant)) && (((Saxon.Hej.expr.CompareToStringConstant)(getFilter())).getSingletonOperator() == Saxon.Hej.expr.parser.Token.FEQ)) && ((Saxon.Hej.expr.CompareToStringConstant)(getFilter())).getLhsExpression().isCallOn(typeof(Saxon.Hej.functions.LocalName_1))) && (((Saxon.Hej.expr.SystemFunctionCall)(((Saxon.Hej.expr.CompareToStringConstant)(getFilter())).getLhsExpression())).getArg(0) is Saxon.Hej.expr.ContextItemExpression))) {
                Saxon.Hej.expr.AxisExpression ax2 = new Saxon.Hej.expr.AxisExpression(((Saxon.Hej.expr.AxisExpression)(getBase())).getAxis(), new Saxon.Hej.pattern.LocalNameTest(config.getNamePool(), Saxon.Hej.type.Type.ELEMENT, ((Saxon.Hej.expr.CompareToStringConstant)(getFilter())).getComparand().ToString()));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ax2);
                return ax2;
            }
            Saxon.Hej.type.ItemType filterType = getFilter().getItemType();
            if ((!(th.isSubType(filterType, Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) && (th.relationship(filterType, Saxon.Hej.type.NumericType.getInstance()) == Saxon.Hej.type.Affinity.DISJOINT))) {
                Saxon.Hej.expr.Expression f = Saxon.Hej.functions.SystemFunction.makeCall("boolean", getRetainedStaticContext(), getFilter());
                setFilter(f.optimize(visitor, baseItemType));
            }
            if (((getFilter() is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue() is Saxon.Hej.value.BooleanValue))) {
                if (((Saxon.Hej.value.BooleanValue)(((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue())).getBooleanValue()) {
                    if (tracing) {
                        opt.trace("Redundant filter removed", getBase());
                    }
                    return getBase();
                } else {
                    Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeEmptySequence();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
                    if (tracing) {
                        opt.trace("Filter expression eliminated because predicate is always false", result);
                    }
                    return result;
                }
            }
            filterIsPositional = isPositionalFilter(getFilter(), th);
            filterIsSingletonBoolean = ((getFilter().getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && getFilter().getItemType().Equals(Saxon.Hej.type.BuiltInAtomicType.BOOLEAN));
            if (((!(filterIsPositional) && !(visitor.isOptimizeForStreaming())) && !(indexingDisabled))) {
                int isIndexable = opt.isIndexableFilter(getFilter());
                if ((isIndexable != 0)) {
                    bool contextIsDoc = (((contextItemType != null) && (contextItemType.getItemType() != Saxon.Hej.type.ErrorType.getInstance())) && th.isSubType(contextItemType.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT));
                    Saxon.Hej.expr.Expression f = opt.tryIndexedFilter(this, visitor, (isIndexable>0), contextIsDoc);
                    if ((f != this)) {
                        return f.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                    }
                }
            }
            if (((filterIsPositional && (getFilter() is Saxon.Hej.expr.BooleanExpression)) && (((Saxon.Hej.expr.BooleanExpression)(getFilter())).@operator == Saxon.Hej.expr.parser.Token.AND))) {
                Saxon.Hej.expr.BooleanExpression bf = (Saxon.Hej.expr.BooleanExpression)(getFilter());
                if ((isExplicitlyPositional(bf.getLhsExpression()) && !(isExplicitlyPositional(bf.getRhsExpression())))) {
                    Saxon.Hej.expr.Expression p0 = forceToBoolean(bf.getLhsExpression());
                    Saxon.Hej.expr.Expression p1 = forceToBoolean(bf.getRhsExpression());
                    Saxon.Hej.expr.FilterExpression f1 = new Saxon.Hej.expr.FilterExpression(getBase(), p0);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f1);
                    Saxon.Hej.expr.FilterExpression f2 = new Saxon.Hej.expr.FilterExpression(f1, p1);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
                    if (tracing) {
                        opt.trace("Composite filter replaced by nested filter expressions", f2);
                    }
                    return f2.optimize(visitor, contextItemType);
                }
                if ((isExplicitlyPositional(bf.getRhsExpression()) && !(isExplicitlyPositional(bf.getLhsExpression())))) {
                    Saxon.Hej.expr.Expression p0 = forceToBoolean(bf.getLhsExpression());
                    Saxon.Hej.expr.Expression p1 = forceToBoolean(bf.getRhsExpression());
                    Saxon.Hej.expr.FilterExpression f1 = new Saxon.Hej.expr.FilterExpression(getBase(), p1);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f1);
                    Saxon.Hej.expr.FilterExpression f2 = new Saxon.Hej.expr.FilterExpression(f1, p0);
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
                    if (tracing) {
                        opt.trace("Composite filter replaced by nested filter expressions", f2);
                    }
                    return f2.optimize(visitor, contextItemType);
                }
            }
            if (((getFilter() is Saxon.Hej.expr.IsLastExpression) && ((Saxon.Hej.expr.IsLastExpression)(getFilter())).getCondition())) {
                if ((getBase() is Saxon.Hej.expr.Literal)) {
                    setFilter(Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.Int64Value(((Saxon.Hej.expr.Literal)(getBase())).getGroundedValue().getLength()), this));
                } else {
                    return new Saxon.Hej.expr.LastItemExpression(getBase());
                }
            }
            Saxon.Hej.expr.Expression subsequence = tryToRewritePositionalFilter(visitor, tracing);
            if ((subsequence != null)) {
                if (tracing) {
                    subsequence.setRetainedStaticContext(getRetainedStaticContext());
                    opt.trace("Rewrote Filter Expression as:", subsequence);
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, subsequence);
                return subsequence.simplify().typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
            }
            if (((!(filterIsPositional) && !(doneReorderingPredicates)) && !(((getParentExpression() is Saxon.Hej.expr.FilterExpression))))) {
                Saxon.Hej.expr.FilterExpression f2 = opt.reorderPredicates(this, visitor, contextItemType);
                if ((f2 != this)) {
                    f2.doneReorderingPredicates = true;
                    return f2;
                }
            }
            Saxon.Hej.om.Sequence sequence = tryEarlyEvaluation(visitor);
            if ((sequence != null)) {
                Saxon.Hej.om.GroundedValue value = sequence.materialize();
                return Saxon.Hej.expr.Literal.makeLiteral(value, this);
            }
            return this;
        }
        public override double getCost() {
            return System.Math.Max((getLhsExpression().getCost()+(5*getRhsExpression().getCost())), MAX_COST);
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getBase().getIntegerBounds();
        }
        private Saxon.Hej.om.Sequence tryEarlyEvaluation(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            try {
                if ((((getBase() is Saxon.Hej.expr.Literal) && !(Saxon.Hej.expr.parser.ExpressionTool.refersToVariableOrFunction(getFilter()))) && (((getFilter().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS))) == 0))) {
                    Saxon.Hej.expr.XPathContext context = visitor.getStaticContext().makeEarlyEvaluationContext();
                    return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate(context));
                }
            } catch (System.Exception e) {
                return null;
            }
            return null;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getBase().addToPathMap(pathMap, pathMapNodeSet);
            getFilter().addToPathMap(pathMap, target);
            return target;
        }
        private static Saxon.Hej.expr.Expression forceToBoolean(Saxon.Hej.expr.Expression @in) {
            if ((@in.getItemType().getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_BOOLEAN)) {
                return @in;
            }
            return Saxon.Hej.functions.SystemFunction.makeCall("boolean", @in.getRetainedStaticContext(), @in);
        }
        private Saxon.Hej.expr.Expression tryToRewritePositionalFilter(Saxon.Hej.expr.parser.ExpressionVisitor visitor, bool tracing) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if ((getFilter() is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue();
                if ((val is Saxon.Hej.value.NumericValue)) {
                    Saxon.Hej.expr.Expression result;
                    int lvalue = ((Saxon.Hej.value.NumericValue)(val)).asSubscript();
                    if ((lvalue != -1)) {
                        if ((lvalue == 1)) {
                            result = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(getBase());
                        } else {
                            result = new Saxon.Hej.expr.SubscriptExpression(getBase(), getFilter());
                        }
                    } else {
                        result = Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                    if (tracing) {
                        Saxon.Hej.expr.parser.Optimizer.trace(config, "Rewriting numeric filter expression with constant subscript", result);
                    }
                    return result;
                } else {
                    Saxon.Hej.expr.Expression result = (Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(val.iterate()) ? getBase() : Saxon.Hej.expr.Literal.makeEmptySequence());
                    if (tracing) {
                        Saxon.Hej.expr.parser.Optimizer.trace(config, "Rewriting boolean filter expression with constant subscript", result);
                    }
                    return result;
                }
            }
            if (((Saxon.Hej.type.NumericType.isNumericType(getFilter().getItemType()) && !(Saxon.Hej.value.Cardinality.allowsMany(getFilter().getCardinality()))) && (((getFilter().getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) == 0))) {
                Saxon.Hej.expr.Expression result = new Saxon.Hej.expr.SubscriptExpression(getBase(), getFilter());
                if (tracing) {
                    Saxon.Hej.expr.parser.Optimizer.trace(config, "Rewriting numeric filter expression with focus-independent subscript", result);
                }
                return result;
            }
            if ((getFilter() is Saxon.Hej.expr.ComparisonExpression)) {
                Saxon.Hej.expr.Expression lhs = ((Saxon.Hej.expr.ComparisonExpression)(getFilter())).getLhsExpression();
                Saxon.Hej.expr.Expression rhs = ((Saxon.Hej.expr.ComparisonExpression)(getFilter())).getRhsExpression();
                int @operator = ((Saxon.Hej.expr.ComparisonExpression)(getFilter())).getSingletonOperator();
                Saxon.Hej.expr.Expression comparand;
                if ((lhs.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Position)) && Saxon.Hej.type.NumericType.isNumericType(rhs.getItemType()))) {
                    comparand = rhs;
                } else {
                    if ((rhs.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Position)) && Saxon.Hej.type.NumericType.isNumericType(lhs.getItemType()))) {
                        comparand = lhs;
                        @operator = Saxon.Hej.expr.parser.Token.inverse(@operator);
                    } else {
                        return null;
                    }
                }
                if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(comparand)) {
                    return null;
                }
                int card = comparand.getCardinality();
                if (Saxon.Hej.value.Cardinality.allowsMany(card)) {
                    return null;
                }
                if (Saxon.Hej.value.Cardinality.allowsZero(card)) {
                    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                    let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(comparand.getItemType(), card));
                    let.setVariableQName(new Saxon.Hej.om.StructuredQName("pp", Saxon.Hej.om.NamespaceUri.SAXON, ("pp"+let.GetHashCode())));
                    let.setSequence(comparand);
                    comparand = new Saxon.Hej.expr.LocalVariableReference(let);
                    Saxon.Hej.expr.LocalVariableReference existsArg = new Saxon.Hej.expr.LocalVariableReference(let);
                    Saxon.Hej.expr.Expression exists = Saxon.Hej.functions.SystemFunction.makeCall("exists", getRetainedStaticContext(), existsArg);
                    Saxon.Hej.expr.Expression rewrite = tryToRewritePositionalFilterSupport(getBase(), comparand, @operator, th);
                    if ((rewrite == null)) {
                        return this;
                    }
                    Saxon.Hej.expr.Expression choice = Saxon.Hej.expr.instruct.Choose.makeConditional(exists, rewrite);
                    let.setAction(choice);
                    return let;
                } else {
                    return tryToRewritePositionalFilterSupport(getBase(), comparand, @operator, th);
                }
            } else {
                if ((getFilter() is Saxon.Hej.expr.IntegerRangeTest)) {
                    Saxon.Hej.expr.Expression val = ((Saxon.Hej.expr.IntegerRangeTest)(getFilter())).getValue();
                    if (!(val.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast)))) {
                        return null;
                    }
                    Saxon.Hej.expr.Expression min = ((Saxon.Hej.expr.IntegerRangeTest)(getFilter())).getMin();
                    Saxon.Hej.expr.Expression max = ((Saxon.Hej.expr.IntegerRangeTest)(getFilter())).getMax();
                    if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(min)) {
                        return null;
                    }
                    if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(max)) {
                        if (max.isCallOn(typeof(Saxon.Hej.functions.PositionAndLast.Last))) {
                            Saxon.Hej.expr.Expression result = Saxon.Hej.functions.SystemFunction.makeCall("subsequence", getRetainedStaticContext(), getBase(), min);
                            if (tracing) {
                                Saxon.Hej.expr.parser.Optimizer.trace(config, "Rewriting numeric range filter expression using subsequence()", result);
                            }
                            return result;
                        } else {
                            return null;
                        }
                    }
                    Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                    let.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
                    let.setVariableQName(new Saxon.Hej.om.StructuredQName("nn", Saxon.Hej.om.NamespaceUri.SAXON, ("nn"+let.GetHashCode())));
                    let.setSequence(min);
                    min = new Saxon.Hej.expr.LocalVariableReference(let);
                    Saxon.Hej.expr.LocalVariableReference min2 = new Saxon.Hej.expr.LocalVariableReference(let);
                    Saxon.Hej.expr.Expression minMinusOne = new Saxon.Hej.expr.ArithmeticExpression(min2, Saxon.Hej.expr.parser.Token.MINUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), this));
                    Saxon.Hej.expr.Expression length = new Saxon.Hej.expr.ArithmeticExpression(max, Saxon.Hej.expr.parser.Token.MINUS, minMinusOne);
                    Saxon.Hej.expr.Expression subs = Saxon.Hej.functions.SystemFunction.makeCall("subsequence", getRetainedStaticContext(), getBase(), min, length);
                    let.setAction(subs);
                    if (tracing) {
                        Saxon.Hej.expr.parser.Optimizer.trace(config, "Rewriting numeric range filter expression using subsequence()", subs);
                    }
                    return let;
                } else {
                    return null;
                }
            }
        }
        private static Saxon.Hej.expr.Expression tryToRewritePositionalFilterSupport(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression comparand, int @operator, Saxon.Hej.type.TypeHierarchy th) {
            if (th.isSubType(comparand.getItemType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER)) {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                    {
                        if (Saxon.Hej.expr.Literal.isConstantOne(comparand)) {
                            return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(start);
                        } else {
                            if (((comparand is Saxon.Hej.expr.Literal) && (((Saxon.Hej.value.IntegerValue)(((Saxon.Hej.expr.Literal)(comparand)).getGroundedValue())).asBigInteger().CompareTo(System.Numerics.BigInteger.Zero)<=0))) {
                                return Saxon.Hej.expr.Literal.makeEmptySequence();
                            } else {
                                return new Saxon.Hej.expr.SubscriptExpression(start, comparand);
                            }
                        }
                    }
                  case Saxon.Hej.expr.parser.Token.FLT:
                    {
                        Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[3];
                        args[0] = start;
                        args[1] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start);
                        if (Saxon.Hej.expr.Literal.isAtomic(comparand)) {
                            long n = ((Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(comparand)).getGroundedValue())).longValue();
                            args[2] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue((n-1)), start);
                        } else {
                            Saxon.Hej.expr.ArithmeticExpression decrement = new Saxon.Hej.expr.ArithmeticExpression(comparand, Saxon.Hej.expr.parser.Token.MINUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start));
                            decrement.setCalculator(Saxon.Hej.expr.Calculator.getCalculator(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.expr.Calculator.MINUS, true));
                            args[2] = decrement;
                        }
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), args);
                    }
                  case Saxon.Hej.expr.parser.Token.FLE:
                    {
                        Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[3];
                        args[0] = start;
                        args[1] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start);
                        args[2] = comparand;
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), args);
                    }
                  case Saxon.Hej.expr.parser.Token.FNE:
                    {
                        return Saxon.Hej.functions.SystemFunction.makeCall("remove", start.getRetainedStaticContext(), start, comparand);
                    }
                  case Saxon.Hej.expr.parser.Token.FGT:
                    {
                        Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[2];
                        args[0] = start;
                        if (Saxon.Hej.expr.Literal.isAtomic(comparand)) {
                            long n = ((Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(comparand)).getGroundedValue())).longValue();
                            args[1] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue((n+1)), start);
                        } else {
                            args[1] = new Saxon.Hej.expr.ArithmeticExpression(comparand, Saxon.Hej.expr.parser.Token.PLUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start));
                        }
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), args);
                    }
                  case Saxon.Hej.expr.parser.Token.FGE:
                    {
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), start, comparand);
                    }
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException("operator");
                }
            } else {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.FEQ:
                    {
                        return new Saxon.Hej.expr.SubscriptExpression(start, comparand);
                    }
                  case Saxon.Hej.expr.parser.Token.FLT:
                    {
                        Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                        let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(comparand.getItemType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE));
                        let.setVariableQName(new Saxon.Hej.om.StructuredQName("pp", Saxon.Hej.om.NamespaceUri.SAXON, ("pp"+let.GetHashCode())));
                        let.setSequence(comparand);
                        Saxon.Hej.expr.LocalVariableReference isWholeArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.LocalVariableReference arithArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.LocalVariableReference floorArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.Expression isWhole = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("is-whole-number", 1).makeFunctionCall(isWholeArg);
                        Saxon.Hej.expr.Expression minusOne = new Saxon.Hej.expr.ArithmeticExpression(arithArg, Saxon.Hej.expr.parser.Token.MINUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start));
                        Saxon.Hej.expr.Expression floor = Saxon.Hej.functions.SystemFunction.makeCall("floor", start.getRetainedStaticContext(), floorArg);
                        Saxon.Hej.expr.Expression choice = Saxon.Hej.expr.instruct.Choose.makeConditional(isWhole, minusOne, floor);
                        Saxon.Hej.expr.Expression subs = Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), start, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start), choice);
                        let.setAction(subs);
                        return let;
                    }
                  case Saxon.Hej.expr.parser.Token.FLE:
                    {
                        Saxon.Hej.expr.Expression floor = Saxon.Hej.functions.SystemFunction.makeCall("floor", start.getRetainedStaticContext(), comparand);
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), start, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start), floor);
                    }
                  case Saxon.Hej.expr.parser.Token.FNE:
                    {
                        Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(start, let);
                        let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(comparand.getItemType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE));
                        let.setVariableQName(new Saxon.Hej.om.StructuredQName("pp", Saxon.Hej.om.NamespaceUri.SAXON, ("pp"+let.GetHashCode())));
                        let.setSequence(comparand);
                        Saxon.Hej.expr.LocalVariableReference isWholeArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.LocalVariableReference castArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.Expression isWhole = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("is-whole-number", 1).makeFunctionCall(isWholeArg);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(start, isWhole);
                        Saxon.Hej.expr.Expression cast = new Saxon.Hej.expr.CastExpression(castArg, Saxon.Hej.type.BuiltInAtomicType.INTEGER, false);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(start, cast);
                        Saxon.Hej.expr.Expression choice = Saxon.Hej.expr.instruct.Choose.makeConditional(isWhole, cast, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(0), start));
                        Saxon.Hej.expr.Expression rem = Saxon.Hej.functions.SystemFunction.makeCall("remove", start.getRetainedStaticContext(), start, choice);
                        let.setAction(rem);
                        return let;
                    }
                  case Saxon.Hej.expr.parser.Token.FGT:
                    {
                        Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                        let.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(comparand.getItemType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE));
                        let.setVariableQName(new Saxon.Hej.om.StructuredQName("pp", Saxon.Hej.om.NamespaceUri.SAXON, ("pp"+let.GetHashCode())));
                        let.setSequence(comparand);
                        Saxon.Hej.expr.LocalVariableReference isWholeArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.LocalVariableReference arithArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.LocalVariableReference ceilingArg = new Saxon.Hej.expr.LocalVariableReference(let);
                        Saxon.Hej.expr.Expression isWhole = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("is-whole-number", 1).makeFunctionCall(isWholeArg);
                        Saxon.Hej.expr.Expression plusOne = new Saxon.Hej.expr.ArithmeticExpression(arithArg, Saxon.Hej.expr.parser.Token.PLUS, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(1), start));
                        Saxon.Hej.expr.Expression ceiling = Saxon.Hej.functions.SystemFunction.makeCall("ceiling", start.getRetainedStaticContext(), ceilingArg);
                        Saxon.Hej.expr.Expression choice = Saxon.Hej.expr.instruct.Choose.makeConditional(isWhole, plusOne, ceiling);
                        Saxon.Hej.expr.Expression subs = Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), start, choice);
                        let.setAction(subs);
                        return let;
                    }
                  case Saxon.Hej.expr.parser.Token.FGE:
                    {
                        Saxon.Hej.expr.Expression ceiling = Saxon.Hej.functions.SystemFunction.makeCall("ceiling", start.getRetainedStaticContext(), comparand);
                        return Saxon.Hej.functions.SystemFunction.makeCall("subsequence", start.getRetainedStaticContext(), start, ceiling);
                    }
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException("operator");
                }
            }
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            if (!(filterIsPositional)) {
                setBase(getBase().unordered(retainAllNodes, forStreaming));
            }
            return this;
        }
        private Saxon.Hej.expr.FilterExpression promoteIndependentPredicates(Saxon.Hej.expr.Binding[] bindings, Saxon.Hej.expr.parser.Optimizer opt, Saxon.Hej.type.TypeHierarchy th) {
            if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getBase(), bindings))) {
                return this;
            }
            if (isPositional(th)) {
                return this;
            }
            if ((getBase() is Saxon.Hej.expr.FilterExpression)) {
                Saxon.Hej.expr.FilterExpression fe = (Saxon.Hej.expr.FilterExpression)(getBase());
                if (fe.isPositional(th)) {
                    return this;
                }
                if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(fe.getFilter(), bindings))) {
                    return this;
                }
                if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getFilter(), bindings))) {
                    Saxon.Hej.expr.FilterExpression result = new Saxon.Hej.expr.FilterExpression(new Saxon.Hej.expr.FilterExpression(fe.getBase(), getFilter()).promoteIndependentPredicates(bindings, opt, th), fe.getFilter());
                    opt.trace("Reordered filter predicates:", result);
                    return result;
                }
            }
            return this;
        }
        public static bool isPositionalFilter(Saxon.Hej.expr.Expression exp, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.ItemType type = exp.getItemType();
            if (type.Equals(Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) {
                return isExplicitlyPositional(exp);
            }
            return (((((type.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || (type is Saxon.Hej.type.AnyItemType)) || type.Equals(Saxon.Hej.type.BuiltInAtomicType.INTEGER)) || type.Equals(Saxon.Hej.type.NumericType.getInstance())) || Saxon.Hej.type.NumericType.isNumericType(type)) || isExplicitlyPositional(exp));
        }
        private static bool isExplicitlyPositional(Saxon.Hej.expr.Expression exp) {
            return (((exp.getDependencies()&((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)))) != 0);
        }
        protected internal override int computeCardinality() {
            if (((getFilter() is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue() is Saxon.Hej.value.NumericValue))) {
                if (((((Saxon.Hej.value.NumericValue)(((Saxon.Hej.expr.Literal)(getFilter())).getGroundedValue())).compareTo(1) == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(getBase().getCardinality())))) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
                } else {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                }
            }
            if (filterIsIndependent) {
                Saxon.Hej.type.ItemType filterType = getFilter().getItemType().getPrimitiveItemType();
                if (((((filterType == Saxon.Hej.type.BuiltInAtomicType.INTEGER) || (filterType == Saxon.Hej.type.BuiltInAtomicType.DOUBLE)) || (filterType == Saxon.Hej.type.BuiltInAtomicType.DECIMAL)) || (filterType == Saxon.Hej.type.BuiltInAtomicType.FLOAT))) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                }
                if ((getFilter() is Saxon.Hej.expr.ArithmeticExpression)) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                }
            }
            if (((getFilter() is Saxon.Hej.expr.IsLastExpression) && ((Saxon.Hej.expr.IsLastExpression)(getFilter())).getCondition())) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getBase().getCardinality()))) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        protected internal override int computeSpecialProperties() {
            return getBase().getSpecialProperties();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.expr.FilterExpression)) {
                Saxon.Hej.expr.FilterExpression f = (Saxon.Hej.expr.FilterExpression)(other);
                return (getBase().isEqual(f.getBase()) && getFilter().isEqual(f.getFilter()));
            }
            return false;
        }
        protected internal override int computeHashCode() {
            return (("FilterExpression".GetHashCode()+getBase().GetHashCode())+getFilter().GetHashCode());
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.Expression @base = getSelectExpression();
            Saxon.Hej.expr.Expression filter = getFilter();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.pattern.Pattern basePattern = @base.toPattern(config);
            if (!(isPositional(th))) {
                return new Saxon.Hej.pattern.BasePatternWithPredicate(basePattern, filter);
            } else {
                if (((((((basePattern is Saxon.Hej.pattern.NodeTestPattern) && (basePattern.getItemType() is Saxon.Hej.pattern.NodeTest)) && filterIsPositional) && (@base is Saxon.Hej.expr.AxisExpression)) && (((Saxon.Hej.expr.AxisExpression)(@base)).getAxis() == Saxon.Hej.om.AxisInfo.CHILD)) && (((filter.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)) == 0))) {
                    if (((filter is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(filter)).getGroundedValue() is Saxon.Hej.value.IntegerValue))) {
                        return new Saxon.Hej.pattern.SimplePositionalPattern((Saxon.Hej.pattern.NodeTest)(basePattern.getItemType()), (int)(((Saxon.Hej.value.IntegerValue)(((Saxon.Hej.expr.Literal)(filter)).getGroundedValue())).longValue()));
                    } else {
                        return new Saxon.Hej.pattern.GeneralPositionalPattern((Saxon.Hej.pattern.NodeTest)(basePattern.getItemType()), filter);
                    }
                }
            }
            if ((@base.getItemType() is Saxon.Hej.pattern.NodeTest)) {
                return new Saxon.Hej.pattern.GeneralNodePattern(this, (Saxon.Hej.pattern.NodeTest)(@base.getItemType()));
            } else {
                throw new Saxon.Hej.trans.XPathException("The filtered expression in an XSLT 2.0 pattern must be a simple step");
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            if (filterIsIndependent) {
                try {
                    Saxon.Hej.om.SequenceIterator it = getFilter().iterate(context);
                    Saxon.Hej.om.Item first = it.next();
                    if ((first == null)) {
                        return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                    }
                    if ((first is Saxon.Hej.value.NumericValue)) {
                        if ((it.next() != null)) {
                            Saxon.Hej.expr.parser.ExpressionTool.ebvError("sequence of two or more items starting with a numeric value", getFilter());
                        } else {
                            int pos = ((Saxon.Hej.value.NumericValue)(first)).asSubscript();
                            if ((pos != -1)) {
                                if ((getBase() is Saxon.Hej.expr.VariableReference)) {
                                    Saxon.Hej.om.Sequence baseVal = ((Saxon.Hej.expr.VariableReference)(getBase())).evaluateVariable(context);
                                    if ((baseVal is Saxon.Hej.value.MemoClosure)) {
                                        Saxon.Hej.om.Item m = ((Saxon.Hej.value.MemoClosure)(baseVal)).itemAt((pos-1));
                                        return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(m);
                                    } else {
                                        Saxon.Hej.om.Item m = baseVal.materialize().itemAt((pos-1));
                                        return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(m);
                                    }
                                } else {
                                    if ((getBase() is Saxon.Hej.expr.Literal)) {
                                        Saxon.Hej.om.Item i = ((Saxon.Hej.expr.Literal)(getBase())).getGroundedValue().itemAt((pos-1));
                                        return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(i);
                                    } else {
                                        return Saxon.Hej.expr.SubsequenceIterator.make(getBase().iterate(context), pos, pos);
                                    }
                                }
                            }
                            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                        }
                    } else {
                        bool ebv = false;
                        if ((first is Saxon.Hej.om.NodeInfo)) {
                            ebv = true;
                        } else {
                            if ((first is Saxon.Hej.value.BooleanValue)) {
                                ebv = ((Saxon.Hej.value.BooleanValue)(first)).getBooleanValue();
                                if ((it.next() != null)) {
                                    Saxon.Hej.expr.parser.ExpressionTool.ebvError("sequence of two or more items starting with a boolean value", getFilter());
                                }
                            } else {
                                if ((first is Saxon.Hej.value.StringValue)) {
                                    ebv = !(((Saxon.Hej.value.StringValue)(first)).isEmpty());
                                    if ((it.next() != null)) {
                                        Saxon.Hej.expr.parser.ExpressionTool.ebvError("sequence of two or more items starting with a boolean value", getFilter());
                                    }
                                } else {
                                    Saxon.Hej.expr.parser.ExpressionTool.ebvError("sequence starting with an atomic value other than a boolean, number, or string", getFilter());
                                }
                            }
                        }
                        if (ebv) {
                            return getBase().iterate(context);
                        } else {
                            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(getLocation());
                    throw;
                }
            }
            Saxon.Hej.om.SequenceIterator baseIter = getBase().iterate(context);
            if ((baseIter is Saxon.Hej.tree.iter.EmptyIterator)) {
                return baseIter;
            }
            if ((filterIsPositional && !(filterIsSingletonBoolean))) {
                return new Saxon.Hej.expr.FilterIterator(baseIter, getFilter(), context);
            } else {
                return new Saxon.Hej.expr.FilterIterator.NonNumeric(baseIter, getFilter(), context);
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.FilterExpression fe = new Saxon.Hej.expr.FilterExpression(getBase().copy(rebindings), getFilter().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, fe);
            fe.filterIsIndependent = filterIsIndependent;
            fe.filterIsPositional = filterIsPositional;
            fe.filterIsSingletonBoolean = filterIsSingletonBoolean;
            fe.indexingDisabled = indexingDisabled;
            return fe;
        }
        public override string getStreamerName() {
            return "FilterExpression";
        }
        public override string ToString() {
            return (((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getBase())+"[")+getFilter())+"]");
        }
        public override string toShortString() {
            return (((getBase().toShortString()+"[")+getFilter().toShortString())+"]");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("filter", this);
            string flags = "";
            if (filterIsIndependent) {
                flags += "i";
            }
            if (filterIsPositional) {
                flags += "p";
            }
            if (filterIsSingletonBoolean) {
                flags += "b";
            }
            @out.emitAttribute("flags", flags);
            getBase().export(@out);
            getFilter().export(@out);
            @out.endElement();
        }
        public void setFlags(string flags) {
            filterIsIndependent = flags.Contains("i");
            filterIsPositional = flags.Contains("p");
            filterIsSingletonBoolean = flags.Contains("b");
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.FilterExpression.FilterExprElaborator();
        }
        public class FilterExprElaborator : Saxon.Hej.expr.elab.PullElaborator {
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.FilterExpression expr = (Saxon.Hej.expr.FilterExpression)(getExpression());
                Saxon.Hej.expr.elab.PullEvaluator baseEval = expr.getBase().makeElaborator().elaborateForPull();
                if (expr.isSimpleBooleanFilter()) {
                    Saxon.Hej.expr.elab.BooleanEvaluator conditionEval = expr.getFilter().makeElaborator().elaborateForBoolean();
                    return context => {
                        Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/baseEval(context);
                        Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                        c2.setCurrentIterator(Saxon.Hej.om.SequenceTool.focusTracker(@base));
                        return new Saxon.Hej.expr.FilterExpression.FilterExprElaborator.SimpleFilteredIterator(c2, conditionEval);
                    } ;
                } else {
                    Saxon.Hej.expr.elab.PullEvaluator conditionEval = expr.getFilter().makeElaborator().elaborateForPull();
                    return context => {
                        Saxon.Hej.om.SequenceIterator @base = /*Delegate.iterate()*/baseEval(context);
                        Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                        c2.setCurrentIterator(Saxon.Hej.om.SequenceTool.focusTracker(@base));
                        return new Saxon.Hej.expr.FilterExpression.FilterExprElaborator.PositionalFilteredIterator(c2, conditionEval);
                    } ;
                }
            }
            public class PositionalFilteredIterator : Saxon.Hej.om.SequenceIterator {
                private readonly Saxon.Hej.expr.XPathContext outerContext;
                private readonly Saxon.Hej.om.FocusIterator @base;
                private readonly Saxon.Hej.expr.elab.PullEvaluator condition;
                public PositionalFilteredIterator(Saxon.Hej.expr.XPathContext outerContext, Saxon.Hej.expr.elab.PullEvaluator condition)  {
                    this.outerContext = outerContext;
                    this.@base = outerContext.getCurrentIterator();
                    this.condition = condition;
                }
                public Saxon.Hej.om.Item next() {
                    try {
                        while (true) {
                            Saxon.Hej.om.Item next = @base.next();
                            if ((next == null)) {
                                return null;
                            }
                            if (Saxon.Hej.expr.FilterIterator.testPredicateValue(/*Delegate.iterate()*/condition(outerContext), @base.position(), null)) {
                                return next;
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                }
            
                /* Expanding default method close from interface SequenceIterator */
                public virtual void close() {
                }

            }
            public class SimpleFilteredIterator : Saxon.Hej.om.SequenceIterator {
                private readonly Saxon.Hej.expr.XPathContext outerContext;
                private readonly Saxon.Hej.om.FocusIterator @base;
                private readonly Saxon.Hej.expr.elab.BooleanEvaluator condition;
                public SimpleFilteredIterator(Saxon.Hej.expr.XPathContext outerContext, Saxon.Hej.expr.elab.BooleanEvaluator condition)  {
                    this.outerContext = outerContext;
                    this.@base = outerContext.getCurrentIterator();
                    this.condition = condition;
                }
                public Saxon.Hej.om.Item next() {
                    try {
                        while (true) {
                            Saxon.Hej.om.Item next = @base.next();
                            if ((next == null)) {
                                return null;
                            }
                            if (/*Delegate.eval()*/condition(outerContext)) {
                                return next;
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                }
            
                /* Expanding default method close from interface SequenceIterator */
                public virtual void close() {
                }

            }
        
        }
    
    }
}
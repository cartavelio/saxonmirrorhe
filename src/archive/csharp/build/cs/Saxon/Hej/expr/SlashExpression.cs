using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class SlashExpression : Saxon.Hej.expr.BinaryExpression, Saxon.Hej.expr.ContextSwitchingExpression {
        internal bool contextFree;
        internal bool indexingDisabled;
        public SlashExpression(Saxon.Hej.expr.Expression start, Saxon.Hej.expr.Expression step) : base (start, Saxon.Hej.expr.parser.Token.SLASH, step) {
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole(int arg) {
            return ((arg == 0) ? Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLING_SELECT : Saxon.Hej.expr.OperandRole.FOCUS_CONTROLLED_ACTION);
        }
        public Saxon.Hej.expr.Expression getStart() {
            return getLhsExpression();
        }
        public void setStart(Saxon.Hej.expr.Expression start) {
            setLhsExpression(start);
        }
        public Saxon.Hej.expr.Expression getStep() {
            return getRhsExpression();
        }
        public void setStep(Saxon.Hej.expr.Expression step) {
            setRhsExpression(step);
        }
        public override string getExpressionName() {
            return "pathExpression";
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return getStart();
        }
        public Saxon.Hej.expr.Expression getActionExpression() {
            return getStep();
        }
        public void disableIndexing() {
            indexingDisabled = true;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getStep().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return getStep().getStaticUType(getStart().getStaticUType(contextItemType));
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getStep().getIntegerBounds();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getLhs().typeCheck(visitor, contextInfo);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getStart())) {
                return getStart();
            }
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.expr.parser.TypeChecker tc = config.getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role0 = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.BINARY_EXPR, "/", 0);
            role0.setErrorCode("XPTY0019");
            setStart(tc.staticTypeCheck(getStart(), Saxon.Hej.value.SequenceType.NODE_SEQUENCE, role0, visitor));
            Saxon.Hej.type.ItemType startType = getStart().getItemType();
            if ((startType == Saxon.Hej.type.ErrorType.getInstance())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(startType, false);
            cit.setContextSettingExpression(getStart());
            getRhs().typeCheck(visitor, cit);
            if ((getRhsExpression() is Saxon.Hej.ma.arrays.SquareArrayConstructor)) {
                Saxon.Hej.ma.arrays.SquareArrayConstructor sq = (Saxon.Hej.ma.arrays.SquareArrayConstructor)(getRhsExpression());
                if ((sq.getOperanda().getNumberOfOperands() == 1)) {
                    visitor.getStaticContext().issueWarning(("An array constructor appears immediately after '/' or '//'. Perhaps "+"'/*[predicate]' was intended? If not, consider using '!' rather than '/' to remove this warning."), getLocation());
                }
            }
            Saxon.Hej.expr.Expression e2 = simplifyDescendantPath(visitor.getStaticContext());
            if ((e2 != null)) {
                return e2.typeCheck(visitor, contextInfo);
            }
            if (((getStart() is Saxon.Hej.expr.ContextItemExpression) && getStep().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET))) {
                return getStep();
            }
            if (((getStep() is Saxon.Hej.expr.ContextItemExpression) && getStart().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET))) {
                return getStart();
            }
            if ((((getStep() is Saxon.Hej.expr.AxisExpression) && (((Saxon.Hej.expr.AxisExpression)(getStep())).getAxis() == Saxon.Hej.om.AxisInfo.SELF)) && config.getTypeHierarchy().isSubType(startType, getStep().getItemType()))) {
                return getStart();
            }
            return this;
        }
        public Saxon.Hej.expr.SlashExpression simplifyDescendantPath(Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.Expression underlyingStep = getStep();
            while ((underlyingStep is Saxon.Hej.expr.FilterExpression)) {
                if (((Saxon.Hej.expr.FilterExpression)(underlyingStep)).isPositional(env.getConfiguration().getTypeHierarchy())) {
                    return null;
                }
                underlyingStep = ((Saxon.Hej.expr.FilterExpression)(underlyingStep)).getSelectExpression();
            }
            if (!(((underlyingStep is Saxon.Hej.expr.AxisExpression)))) {
                return null;
            }
            Saxon.Hej.expr.Expression st = getStart();
            if ((st is Saxon.Hej.expr.AxisExpression)) {
                Saxon.Hej.expr.AxisExpression stax = (Saxon.Hej.expr.AxisExpression)(st);
                if ((stax.getAxis() != Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF)) {
                    return null;
                }
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cie);
                st = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(cie, stax.copy(new Saxon.Hej.expr.parser.RebindingMap()));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, st);
            }
            if (!(((st is Saxon.Hej.expr.SlashExpression)))) {
                return null;
            }
            Saxon.Hej.expr.SlashExpression startPath = (Saxon.Hej.expr.SlashExpression)(st);
            if (!(((startPath.getStep() is Saxon.Hej.expr.AxisExpression)))) {
                return null;
            }
            Saxon.Hej.expr.AxisExpression mid = (Saxon.Hej.expr.AxisExpression)(startPath.getStep());
            if ((mid.getAxis() != Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF)) {
                return null;
            }
            Saxon.Hej.pattern.NodeTest test = mid.getNodeTest();
            if (!((((test == null) || (test is Saxon.Hej.pattern.AnyNodeTest))))) {
                return null;
            }
            int underlyingAxis = ((Saxon.Hej.expr.AxisExpression)(underlyingStep)).getAxis();
            if ((((underlyingAxis == Saxon.Hej.om.AxisInfo.CHILD) || (underlyingAxis == Saxon.Hej.om.AxisInfo.DESCENDANT)) || (underlyingAxis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF))) {
                int newAxis = ((underlyingAxis == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF) ? Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF : Saxon.Hej.om.AxisInfo.DESCENDANT);
                Saxon.Hej.expr.Expression newStep = new Saxon.Hej.expr.AxisExpression(newAxis, ((Saxon.Hej.expr.AxisExpression)(underlyingStep)).getNodeTest());
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newStep);
                underlyingStep = getStep();
                System.Collections.Generic.Stack<Saxon.Hej.expr.Expression> filters = new System.Collections.Generic.Stack<Saxon.Hej.expr.Expression>();
                while ((underlyingStep is Saxon.Hej.expr.FilterExpression)) {
                    filters.Push(((Saxon.Hej.expr.FilterExpression)(underlyingStep)).getFilter());
                    underlyingStep = ((Saxon.Hej.expr.FilterExpression)(underlyingStep)).getSelectExpression();
                }
                while (!((filters.Count==0))) {
                    newStep = new Saxon.Hej.expr.FilterExpression(newStep, filters.Pop());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(getStep(), newStep);
                }
                Saxon.Hej.expr.Expression newPath = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(startPath.getStart(), newStep);
                if (!(((newPath is Saxon.Hej.expr.SlashExpression)))) {
                    return null;
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newPath);
                ((Saxon.Hej.expr.SlashExpression)(newPath)).indexingDisabled = indexingDisabled;
                return (Saxon.Hej.expr.SlashExpression)(newPath);
            }
            if ((underlyingAxis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) {
                Saxon.Hej.expr.Expression newStep = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, newStep);
                Saxon.Hej.expr.Expression e2 = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(startPath.getStart(), newStep);
                Saxon.Hej.expr.Expression e3 = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(e2, getStep());
                if (!(((e3 is Saxon.Hej.expr.SlashExpression)))) {
                    return null;
                }
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, e3);
                return (Saxon.Hej.expr.SlashExpression)(e3);
            }
            return null;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            getLhs().optimize(visitor, contextItemType);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getStart())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(getStart().getItemType(), false);
            cit.setContextSettingExpression(getStart());
            getRhs().optimize(visitor, cit);
            if (Saxon.Hej.expr.Literal.isEmptySequence(getStep())) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if (((getStart() is Saxon.Hej.expr.RootExpression) && th.isSubType(contextItemType.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT))) {
                return getStep();
            }
            Saxon.Hej.expr.Expression e2 = simplifyDescendantPath(visitor.getStaticContext());
            if ((e2 != null)) {
                return e2.optimize(visitor, contextItemType);
            }
            if (!(indexingDisabled)) {
                Saxon.Hej.expr.Expression firstStep = getFirstStep();
                if (!(((firstStep.isCallOn(typeof(Saxon.Hej.functions.Doc)) || firstStep.isCallOn(typeof(Saxon.Hej.functions.DocumentFn)))))) {
                    Saxon.Hej.expr.Expression lastStep = getLastStep();
                    if (((lastStep is Saxon.Hej.expr.FilterExpression) && !(((Saxon.Hej.expr.FilterExpression)(lastStep)).isPositional(th)))) {
                        Saxon.Hej.expr.Expression leading = getLeadingSteps();
                        Saxon.Hej.expr.Expression p2 = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(leading, ((Saxon.Hej.expr.FilterExpression)(lastStep)).getSelectExpression());
                        Saxon.Hej.expr.Expression f2 = new Saxon.Hej.expr.FilterExpression(p2, ((Saxon.Hej.expr.FilterExpression)(lastStep)).getFilter());
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
                        return f2.optimize(visitor, contextItemType);
                    }
                }
                if (!(visitor.isOptimizeForStreaming())) {
                    Saxon.Hej.expr.Expression k = opt.convertPathExpressionToKey(this, visitor);
                    if ((k != null)) {
                        return k.typeCheck(visitor, contextItemType).optimize(visitor, contextItemType);
                    }
                }
            }
            e2 = tryToMakeSorted(visitor, contextItemType);
            if ((e2 != null)) {
                return e2;
            }
            if ((getStep() is Saxon.Hej.expr.AxisExpression)) {
                if (!(Saxon.Hej.value.Cardinality.allowsMany(getStart().getCardinality()))) {
                    Saxon.Hej.expr.SimpleStepExpression sse = new Saxon.Hej.expr.SimpleStepExpression(getStart(), getStep());
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, sse);
                    sse.setParentExpression(getParentExpression());
                    return sse;
                } else {
                    contextFree = true;
                }
            }
            if (((getStart() is Saxon.Hej.expr.RootExpression) && getStep().isCallOn(typeof(Saxon.Hej.functions.KeyFn)))) {
                Saxon.Hej.expr.SystemFunctionCall keyCall = (Saxon.Hej.expr.SystemFunctionCall)(getStep());
                if (((keyCall.getArity() == 3) && (keyCall.getArg(2) is Saxon.Hej.expr.ContextItemExpression))) {
                    keyCall.setArg(2, new Saxon.Hej.expr.RootExpression());
                    keyCall.setParentExpression(getParentExpression());
                    Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(keyCall);
                    return keyCall;
                }
            }
            if (visitor.isOptimizeForStreaming()) {
                Saxon.Hej.expr.Expression rawStep = Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(getStep(), true);
                if (((rawStep is Saxon.Hej.expr.instruct.CopyOf) && (((Saxon.Hej.expr.instruct.CopyOf)(rawStep)).getSelect() is Saxon.Hej.expr.ContextItemExpression))) {
                    ((Saxon.Hej.expr.instruct.CopyOf)(rawStep)).setSelect(getStart());
                    rawStep.resetLocalStaticProperties();
                    getStep().resetLocalStaticProperties();
                    return getStep();
                }
            }
            return this;
        }
        public Saxon.Hej.expr.SlashExpression tryToMakeAbsolute() {
            Saxon.Hej.expr.Expression first = getFirstStep();
            if ((first.getItemType().getPrimitiveType() == Saxon.Hej.type.Type.DOCUMENT)) {
                return this;
            }
            if ((first is Saxon.Hej.expr.AxisExpression)) {
                Saxon.Hej.type.ItemType contextItemType = ((Saxon.Hej.expr.AxisExpression)(first)).getContextItemType();
                if (((contextItemType != null) && (contextItemType.getPrimitiveType() == Saxon.Hej.type.Type.DOCUMENT))) {
                    Saxon.Hej.expr.RootExpression root = new Saxon.Hej.expr.RootExpression();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, root);
                    Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(root, this.copy(new Saxon.Hej.expr.parser.RebindingMap()));
                    if (!(((path is Saxon.Hej.expr.SlashExpression)))) {
                        return null;
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, path);
                    return (Saxon.Hej.expr.SlashExpression)(path);
                }
            }
            if (((first is Saxon.Hej.expr.sort.DocumentSorter) && (((Saxon.Hej.expr.sort.DocumentSorter)(first)).getBaseExpression() is Saxon.Hej.expr.SlashExpression))) {
                Saxon.Hej.expr.SlashExpression se = (Saxon.Hej.expr.SlashExpression)(((Saxon.Hej.expr.sort.DocumentSorter)(first)).getBaseExpression());
                Saxon.Hej.expr.SlashExpression se2 = se.tryToMakeAbsolute();
                if ((se2 != null)) {
                    if ((se2 == se)) {
                        return this;
                    } else {
                        Saxon.Hej.expr.Expression rest = getRemainingSteps();
                        Saxon.Hej.expr.sort.DocumentSorter ds = new Saxon.Hej.expr.sort.DocumentSorter(se2);
                        return new Saxon.Hej.expr.SlashExpression(ds, rest);
                    }
                }
            }
            return null;
        }
        public override double getCost() {
            int factor = (Saxon.Hej.value.Cardinality.allowsMany(getLhsExpression().getCardinality()) ? 5 : 1);
            double lh = (getLhsExpression().getCost()+1);
            double rh = getRhsExpression().getCost();
            double product = (lh+(factor*rh));
            return System.Math.Max(product, MAX_COST);
        }
        public Saxon.Hej.expr.Expression tryToMakeSorted(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.Configuration config = visitor.getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            Saxon.Hej.expr.Expression s1 = Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(getStart(), false);
            if (!((((s1 is Saxon.Hej.expr.AxisExpression) && (((Saxon.Hej.expr.AxisExpression)(s1)).getAxis() == Saxon.Hej.om.AxisInfo.DESCENDANT))))) {
                return null;
            }
            Saxon.Hej.expr.Expression s2 = Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(getStep(), false);
            if (!((((s2 is Saxon.Hej.expr.AxisExpression) && (((Saxon.Hej.expr.AxisExpression)(s2)).getAxis() == Saxon.Hej.om.AxisInfo.CHILD))))) {
                return null;
            }
            Saxon.Hej.expr.Expression x = getStart().copy(new Saxon.Hej.expr.parser.RebindingMap());
            Saxon.Hej.expr.AxisExpression ax = (Saxon.Hej.expr.AxisExpression)(Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(x, false));
            ax.setAxis(Saxon.Hej.om.AxisInfo.PARENT);
            Saxon.Hej.expr.Expression y = getStep().copy(new Saxon.Hej.expr.parser.RebindingMap());
            Saxon.Hej.expr.AxisExpression ay = (Saxon.Hej.expr.AxisExpression)(Saxon.Hej.expr.parser.ExpressionTool.unfilteredExpression(y, false));
            ay.setAxis(Saxon.Hej.om.AxisInfo.DESCENDANT);
            Saxon.Hej.expr.Expression k = new Saxon.Hej.expr.FilterExpression(y, x);
            if (!(th.isSubType(contextItemType.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT))) {
                k = new Saxon.Hej.expr.SlashExpression(new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT), k);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, k);
                opt.trace("Rewrote descendant::X/child::Y as child::*/descendant::Y[parent::X]", k);
            } else {
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, k);
                opt.trace("Rewrote descendant::X/child::Y as descendant::Y[parent::X]", k);
            }
            return k;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            setStart(getStart().unordered(retainAllNodes, forStreaming));
            setStep(getStep().unordered(retainAllNodes, forStreaming));
            return this;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = getStart().addToPathMap(pathMap, pathMapNodeSet);
            return getStep().addToPathMap(pathMap, target);
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression exp = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(getStart().copy(rebindings), getStep().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            if ((exp is Saxon.Hej.expr.SlashExpression)) {
                ((Saxon.Hej.expr.SlashExpression)(exp)).indexingDisabled = indexingDisabled;
            }
            return exp;
        }
        protected internal override int computeSpecialProperties() {
            int startProperties = getStart().getSpecialProperties();
            int stepProperties = getStep().getSpecialProperties();
            if ((((stepProperties&Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED)) != 0)) {
                return ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            }
            int p = 0;
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getStart().getCardinality()))) {
                startProperties |= ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
            }
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getStep().getCardinality()))) {
                stepProperties |= ((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET)|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET);
            }
            if (((((startProperties&stepProperties)&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0)) {
                p |= Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET;
            }
            if ((((((startProperties&Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)) != 0)) && ((((stepProperties&Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)) != 0)))) {
                p |= Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET;
            }
            if (((((startProperties&stepProperties)&Saxon.Hej.expr.StaticProperty.PEER_NODESET)) != 0)) {
                p |= Saxon.Hej.expr.StaticProperty.PEER_NODESET;
            }
            if (((((startProperties&stepProperties)&Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET)) != 0)) {
                p |= Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET;
            }
            if (testNaturallySorted(startProperties, stepProperties)) {
                p |= Saxon.Hej.expr.StaticProperty.ORDERED_NODESET;
            }
            if (testNaturallyReverseSorted()) {
                p |= Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER;
            }
            if (((((startProperties&stepProperties)&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) != 0)) {
                p |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
            }
            return p;
        }
        private bool testNaturallySorted(int startProperties, int stepProperties) {
            if ((((stepProperties&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) == 0)) {
                return false;
            }
            if (Saxon.Hej.value.Cardinality.allowsMany(getStart().getCardinality())) {
                if ((((startProperties&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) == 0)) {
                    return false;
                }
            } else {
                return true;
            }
            if ((((stepProperties&Saxon.Hej.expr.StaticProperty.ATTRIBUTE_NS_NODESET)) != 0)) {
                return true;
            }
            if ((((stepProperties&Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED)) != 0)) {
                return true;
            }
            return (((((startProperties&Saxon.Hej.expr.StaticProperty.PEER_NODESET)) != 0)) && ((((stepProperties&Saxon.Hej.expr.StaticProperty.SUBTREE_NODESET)) != 0)));
        }
        private bool testNaturallyReverseSorted() {
            if ((!(Saxon.Hej.value.Cardinality.allowsMany(getStart().getCardinality())) && ((getStep() is Saxon.Hej.expr.AxisExpression)))) {
                return !(Saxon.Hej.om.AxisInfo.isForwards[((Saxon.Hej.expr.AxisExpression)(getStep())).getAxis()]);
            }
            return ((!(Saxon.Hej.value.Cardinality.allowsMany(getStep().getCardinality())) && ((getStart() is Saxon.Hej.expr.AxisExpression))) && !(Saxon.Hej.om.AxisInfo.isForwards[((Saxon.Hej.expr.AxisExpression)(getStart())).getAxis()]));
        }
        protected internal override int computeCardinality() {
            int c1 = getStart().getCardinality();
            int c2 = getStep().getCardinality();
            return Saxon.Hej.value.Cardinality.multiply(c1, c2);
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.expr.Expression head = getLeadingSteps();
            Saxon.Hej.expr.Expression tail = getLastStep();
            if ((head is Saxon.Hej.expr.ItemChecker)) {
                Saxon.Hej.expr.ItemChecker checker = (Saxon.Hej.expr.ItemChecker)(head);
                if ((checker.getBaseExpression() is Saxon.Hej.expr.ContextItemExpression)) {
                    return tail.toPattern(config);
                }
            }
            Saxon.Hej.pattern.Pattern tailPattern = tail.toPattern(config);
            if ((tailPattern is Saxon.Hej.pattern.NodeTestPattern)) {
                if ((tailPattern.getItemType() is Saxon.Hej.type.ErrorType)) {
                    return tailPattern;
                }
            } else {
                if ((tailPattern is Saxon.Hej.pattern.GeneralNodePattern)) {
                    return new Saxon.Hej.pattern.GeneralNodePattern(this, (Saxon.Hej.pattern.NodeTest)(tailPattern.getItemType()));
                }
            }
            int axis = Saxon.Hej.om.AxisInfo.PARENT;
            Saxon.Hej.pattern.Pattern headPattern = null;
            if ((head is Saxon.Hej.expr.SlashExpression)) {
                Saxon.Hej.expr.SlashExpression start = (Saxon.Hej.expr.SlashExpression)(head);
                if ((start.getActionExpression() is Saxon.Hej.expr.AxisExpression)) {
                    Saxon.Hej.expr.AxisExpression mid = (Saxon.Hej.expr.AxisExpression)(start.getActionExpression());
                    if (((mid.getAxis() == Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF) && (((mid.getNodeTest() == null) || (mid.getNodeTest() is Saxon.Hej.pattern.AnyNodeTest))))) {
                        axis = Saxon.Hej.om.AxisInfo.ANCESTOR;
                        headPattern = start.getSelectExpression().toPattern(config);
                    }
                }
            }
            if ((headPattern == null)) {
                if ((tail is Saxon.Hej.expr.VennExpression)) {
                    Saxon.Hej.expr.Expression lhExpansion = new Saxon.Hej.expr.SlashExpression(head.copy(new Saxon.Hej.expr.parser.RebindingMap()), ((Saxon.Hej.expr.VennExpression)(tail)).getLhsExpression());
                    Saxon.Hej.expr.Expression rhExpansion = new Saxon.Hej.expr.SlashExpression(head.copy(new Saxon.Hej.expr.parser.RebindingMap()), ((Saxon.Hej.expr.VennExpression)(tail)).getRhsExpression());
                    Saxon.Hej.expr.VennExpression topExpansion = new Saxon.Hej.expr.VennExpression(lhExpansion, ((Saxon.Hej.expr.VennExpression)(tail)).@operator, rhExpansion);
                    return topExpansion.toPattern(config);
                } else {
                    axis = Saxon.Hej.pattern.PatternMaker.getAxisForPathStep(tail);
                    headPattern = head.toPattern(config);
                }
            }
            return new Saxon.Hej.pattern.AncestorQualifiedPattern(tailPattern, headPattern, axis);
        }
        public bool isContextFree() {
            return contextFree;
        }
        public void setContextFree(bool free) {
            this.contextFree = free;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.expr.SlashExpression)))) {
                return false;
            }
            Saxon.Hej.expr.SlashExpression p = (Saxon.Hej.expr.SlashExpression)(other);
            return (getStart().isEqual(p.getStart()) && getStep().isEqual(p.getStep()));
        }
        protected internal override int computeHashCode() {
            return (("SlashExpression".GetHashCode()+getStart().GetHashCode())+getStep().GetHashCode());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.Expression step = getStep();
            if ((contextFree && (step is Saxon.Hej.expr.AxisExpression))) {
                return Saxon.Hej.expr.MappingIterator.map(getStart().iterate(context), item => ((Saxon.Hej.expr.AxisExpression)(step)).iterate((Saxon.Hej.om.NodeInfo)(item)));
            }
            Saxon.Hej.expr.XPathContext context2 = context.newMinorContext();
            context2.trackFocus(getStart().iterate(context));
            return new Saxon.Hej.expr.ContextMappingIterator(c1 => getStep().iterate(c1), context2);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("slash", this);
            if ((this is Saxon.Hej.expr.SimpleStepExpression)) {
                destination.emitAttribute("simple", "1");
            } else {
                if (isContextFree()) {
                    destination.emitAttribute("simple", "2");
                }
            }
            getStart().export(destination);
            getStep().export(destination);
            destination.endElement();
        }
        public override string ToString() {
            return ((Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getStart())+"/")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getStep()));
        }
        public override string toShortString() {
            return ((Saxon.Hej.expr.parser.ExpressionTool.parenthesizeShort(getStart())+"/")+Saxon.Hej.expr.parser.ExpressionTool.parenthesizeShort(getStep()));
        }
        public Saxon.Hej.expr.Expression getFirstStep() {
            if ((getStart() is Saxon.Hej.expr.SlashExpression)) {
                return ((Saxon.Hej.expr.SlashExpression)(getStart())).getFirstStep();
            } else {
                return getStart();
            }
        }
        public Saxon.Hej.expr.Expression getRemainingSteps() {
            if ((getStart() is Saxon.Hej.expr.SlashExpression)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(8);
                gatherSteps(list);
                Saxon.Hej.expr.Expression rem = rebuildSteps(Saxon.Impl.Helpers.ListUtils.SubList(list, 1, list.Count));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, rem);
                return rem;
            } else {
                return getStep();
            }
        }
        private void gatherSteps(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if ((getStart() is Saxon.Hej.expr.SlashExpression)) {
                ((Saxon.Hej.expr.SlashExpression)(getStart())).gatherSteps(list);
            } else {
                list.Add(getStart());
            }
            if ((getStep() is Saxon.Hej.expr.SlashExpression)) {
                ((Saxon.Hej.expr.SlashExpression)(getStep())).gatherSteps(list);
            } else {
                list.Add(getStep());
            }
        }
        private Saxon.Hej.expr.Expression rebuildSteps(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list) {
            if ((list.Count == 1)) {
                return list[0].copy(new Saxon.Hej.expr.parser.RebindingMap());
            } else {
                return new Saxon.Hej.expr.SlashExpression(list[0].copy(new Saxon.Hej.expr.parser.RebindingMap()), rebuildSteps(Saxon.Impl.Helpers.ListUtils.SubList(list, 1, list.Count)));
            }
        }
        public Saxon.Hej.expr.Expression getLastStep() {
            if ((getStep() is Saxon.Hej.expr.SlashExpression)) {
                return ((Saxon.Hej.expr.SlashExpression)(getStep())).getLastStep();
            } else {
                return getStep();
            }
        }
        public Saxon.Hej.expr.Expression getLeadingSteps() {
            if ((getStep() is Saxon.Hej.expr.SlashExpression)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(8);
                gatherSteps(list);
                Saxon.Hej.expr.Expression rem = rebuildSteps(Saxon.Impl.Helpers.ListUtils.SubList(list, 0, (list.Count-1)));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, rem);
                return rem;
            } else {
                return getStart();
            }
        }
        public bool isAbsolute() {
            return (getFirstStep().getItemType().getPrimitiveType() == Saxon.Hej.type.Type.DOCUMENT);
        }
        public override string getStreamerName() {
            return "ForEach";
        }
    
    }
}
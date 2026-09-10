using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Choose : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ConditionalInstruction {
        private readonly Saxon.Hej.expr.Operand[] conditionOps;
        private readonly Saxon.Hej.expr.Operand[] actionOps;
        private bool _isInstruction;
        public readonly static Saxon.Hej.expr.OperandRole CHOICE_ACTION = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.IN_CHOICE_GROUP, Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public Choose(Saxon.Hej.expr.Expression[] conditions, Saxon.Hej.expr.Expression[] actions)  {
            conditionOps = new Saxon.Hej.expr.Operand[conditions.Length];
            for (int i = 0; (i<conditions.Length); i++) {
                conditionOps[i] = new Saxon.Hej.expr.Operand(this, conditions[i], Saxon.Hej.expr.OperandRole.INSPECT);
            }
            actionOps = new Saxon.Hej.expr.Operand[actions.Length];
            for (int i = 0; (i<actions.Length); i++) {
                actionOps[i] = new Saxon.Hej.expr.Operand(this, actions[i], CHOICE_ACTION);
            }
        }
        public static Saxon.Hej.expr.Expression makeConditional(Saxon.Hej.expr.Expression condition, Saxon.Hej.expr.Expression thenExp, Saxon.Hej.expr.Expression elseExp) {
            if (Saxon.Hej.expr.Literal.isEmptySequence(elseExp)) {
                Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{condition};
                Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{thenExp};
                return new Saxon.Hej.expr.instruct.Choose(conditions, actions);
            } else {
                Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{condition, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, condition)};
                Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{thenExp, elseExp};
                return new Saxon.Hej.expr.instruct.Choose(conditions, actions);
            }
        }
        public static Saxon.Hej.expr.Expression makeConditional(Saxon.Hej.expr.Expression condition, Saxon.Hej.expr.Expression thenExp) {
            Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{condition};
            Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{thenExp};
            return new Saxon.Hej.expr.instruct.Choose(conditions, actions);
        }
        public void setInstruction(bool inst) {
            _isInstruction = inst;
        }
        public override bool isInstruction() {
            return _isInstruction;
        }
        public int size() {
            return conditionOps.Length;
        }
        public static bool isSingleBranchChoice(Saxon.Hej.expr.Expression exp) {
            return ((exp is Saxon.Hej.expr.instruct.Choose) && (((Saxon.Hej.expr.instruct.Choose)(exp)).size() == 1));
        }
        public int getNumberOfConditions() {
            return size();
        }
        public Saxon.Hej.expr.Expression getCondition(int i) {
            return conditionOps[i].getChildExpression();
        }
        public void setCondition(int i, Saxon.Hej.expr.Expression condition) {
            conditionOps[i].setChildExpression(condition);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> conditions() {
            return Saxon.Ejava.util.Arrays.asList(conditionOps);
        }
        public Saxon.Hej.expr.Operand getActionOperand(int i) {
            return actionOps[i];
        }
        public Saxon.Hej.expr.Expression getAction(int i) {
            return actionOps[i].getChildExpression();
        }
        public void setAction(int i, Saxon.Hej.expr.Expression action) {
            actionOps[i].setChildExpression(action);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> actions() {
            return Saxon.Ejava.util.Arrays.asList(actionOps);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operanda = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>((size()*2));
            for (int i = 0; (i<size()); i++) {
                operanda.Add(conditionOps[i]);
                operanda.Add(actionOps[i]);
            }
            return operanda;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public void atomizeActions() {
            for (int i = 0; (i<size()); i++) {
                setAction(i, Saxon.Hej.expr.Atomizer.makeAtomizer(getAction(i), null));
            }
        }
        public override int getInstructionNameCode() {
            return ((size() == 1) ? Saxon.Hej.om.StandardNames.XSL_IF : Saxon.Hej.om.StandardNames.XSL_CHOOSE);
        }
        public override Saxon.Hej.expr.Expression simplify() {
            for (int i = 0; (i<size()); i++) {
                setCondition(i, getCondition(i).simplify());
                try {
                    setAction(i, getAction(i).simplify());
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (err.isTypeError()) {
                        throw;
                    } else {
                        setAction(i, new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err)));
                    }
                }
            }
            return this;
        }
        private Saxon.Hej.expr.Expression removeRedundantBranches(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.Expression result = removeRedundantBranches0(visitor);
            if ((result != this)) {
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
            }
            return result;
        }
        private Saxon.Hej.expr.Expression removeRedundantBranches0(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            bool compress = false;
            for (int i = 0; (i<size()); i++) {
                Saxon.Hej.expr.Expression condition = getCondition(i);
                if ((condition is Saxon.Hej.expr.Literal)) {
                    compress = true;
                    break ;
                }
            }
            int count = size();
            if (compress) {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> conditions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(count);
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> actions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(count);
                for (int i = 0; (i<count); i++) {
                    Saxon.Hej.expr.Expression condition = getCondition(i);
                    if (!(Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(condition, false))) {
                        conditions.Add(condition);
                        actions.Add(getAction(i));
                    }
                    if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(condition, true)) {
                        break ;
                    }
                }
                if (conditions.Count==0) {
                    Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeEmptySequence();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                    return lit;
                } else {
                    if (((conditions.Count == 1) && Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(conditions[0], true))) {
                        return actions[0];
                    } else {
                        if ((conditions.Count != count)) {
                            Saxon.Hej.expr.Expression[] c = conditions.ToArray();
                            Saxon.Hej.expr.Expression[] a = actions.ToArray();
                            Saxon.Hej.expr.instruct.Choose result = new Saxon.Hej.expr.instruct.Choose(c, a);
                            result.setRetainedStaticContext(getRetainedStaticContext());
                            return result;
                        }
                    }
                }
            }
            if (((size() == 1) && Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(0), true))) {
                return getAction(0);
            }
            if (Saxon.Hej.expr.Literal.isEmptySequence(getAction((size()-1)))) {
                if ((size() == 1)) {
                    Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeEmptySequence();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, lit);
                    return lit;
                } else {
                    Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[(count-1)];
                    Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[(count-1)];
                    for (int i = 0; (i<(count-1)); i++) {
                        conditions[i] = getCondition(i);
                        actions[i] = getAction(i);
                    }
                    return new Saxon.Hej.expr.instruct.Choose(conditions, actions);
                }
            }
            if ((Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition((count-1)), true) && (getAction((count-1)) is Saxon.Hej.expr.instruct.Choose))) {
                Saxon.Hej.expr.instruct.Choose choose2 = (Saxon.Hej.expr.instruct.Choose)(getAction((count-1)));
                int newLen = ((count+choose2.size())-1);
                Saxon.Hej.expr.Expression[] c2 = new Saxon.Hej.expr.Expression[newLen];
                Saxon.Hej.expr.Expression[] a2 = new Saxon.Hej.expr.Expression[newLen];
                for (int i = 0; (i<(count-1)); i++) {
                    c2[i] = getCondition(i);
                    a2[i] = getAction(i);
                }
                for (int i = 0; (i<choose2.size()); i++) {
                    c2[((i+count)-1)] = choose2.getCondition(i);
                    a2[((i+count)-1)] = choose2.getAction(i);
                }
                return new Saxon.Hej.expr.instruct.Choose(c2, a2);
            }
            if (((((count == 2) && Saxon.Hej.expr.Literal.isConstantBoolean(getAction(0), true)) && Saxon.Hej.expr.Literal.isConstantBoolean(getAction(1), false)) && Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(1), true))) {
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                if ((th.isSubType(getCondition(0).getItemType(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN) && (getCondition(0).getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                    return getCondition(0);
                } else {
                    return Saxon.Hej.functions.SystemFunction.makeCall("boolean", getRetainedStaticContext(), getCondition(0));
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            for (int i = 0; (i<size()); i++) {
                conditionOps[i].typeCheck(visitor, contextInfo);
                Saxon.Hej.trans.XPathException err = Saxon.Hej.expr.parser.TypeChecker.ebvError(getCondition(i), th);
                if ((err != null)) {
                    err.setLocator(getCondition(i).getLocation());
                    err.maybeSetFailingExpression(getCondition(i));
                    throw err;
                }
            }
            for (int i = 0; (i<size()); i++) {
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), false)) {
                    continue ;
                }
                try {
                    actionOps[i].typeCheck(visitor, contextInfo);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(getLocation());
                    err.maybeSetFailingExpression(getAction(i));
                    if (err.isStaticError()) {
                        throw;
                    } else {
                        if (err.isTypeError()) {
                            if ((Saxon.Hej.expr.Literal.isEmptySequence(getAction(i)) || Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), false))) {
                                setAction(i, new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err)));
                            } else {
                                throw;
                            }
                        } else {
                            setAction(i, new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err)));
                        }
                    }
                }
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), true)) {
                    break ;
                }
            }
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                Saxon.Hej.expr.Expression reduced = removeRedundantBranches(visitor);
                if ((reduced != this)) {
                    return reduced.typeCheck(visitor, contextInfo);
                }
                return reduced;
            }
            return this;
        }
        public override bool implementsStaticTypeCheck() {
            return true;
        }
        public override Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.value.SequenceType req, bool backwardsCompatible, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            int count = size();
            Saxon.Hej.expr.parser.TypeChecker tc = getConfiguration().getTypeChecker(backwardsCompatible);
            for (int i = 0; (i<count); i++) {
                try {
                    setAction(i, tc.staticTypeCheck(getAction(i), req, role, visitor));
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (err.isStaticError()) {
                        throw;
                    }
                    Saxon.Hej.expr.ErrorExpression ee = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(getAction(i), ee);
                    setAction(i, ee);
                }
            }
            if ((!(Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition((count-1)), true)) && !(Saxon.Hej.value.Cardinality.allowsZero(req.getCardinality())))) {
                Saxon.Hej.expr.Expression[] c = new Saxon.Hej.expr.Expression[(count+1)];
                Saxon.Hej.expr.Expression[] a = new Saxon.Hej.expr.Expression[(count+1)];
                for (int i = 0; (i<count); i++) {
                    c[i] = getCondition(i);
                    a[i] = getAction(i);
                }
                c[count] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, this);
                string cond = ((count == 1) ? "The condition is not" : "None of the conditions is");
                string message = (((("Conditional expression: "+cond)+" satisfied, so an empty sequence is returned, ")+"but this is not allowed as the ")+role.getMessage());
                Saxon.Hej.expr.ErrorExpression errExp = new Saxon.Hej.expr.ErrorExpression(message, role.getErrorCode(), true);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, errExp);
                a[count] = errExp;
                return new Saxon.Hej.expr.instruct.Choose(c, a);
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            int count = size();
            for (int i = 0; (i<count); i++) {
                conditionOps[i].optimize(visitor, contextItemType);
                Saxon.Hej.expr.Expression ebv = Saxon.Hej.functions.BooleanFn.rewriteEffectiveBooleanValue(getCondition(i), visitor, contextItemType);
                if (((ebv != null) && (ebv != getCondition(i)))) {
                    setCondition(i, ebv);
                }
                if (((getCondition(i) is Saxon.Hej.expr.Literal) && !(((((Saxon.Hej.expr.Literal)(getCondition(i))).getGroundedValue() is Saxon.Hej.value.BooleanValue))))) {
                    bool b;
                    try {
                        b = ((Saxon.Hej.expr.Literal)(getCondition(i))).getGroundedValue().effectiveBooleanValue();
                    } catch (Saxon.Hej.trans.XPathException err) {
                        err.setLocation(getLocation());
                        throw;
                    }
                    setCondition(i, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(b), this));
                }
            }
            for (int i = 0; (i<count); i++) {
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), false)) {
                    continue ;
                }
                try {
                    actionOps[i].optimize(visitor, contextItemType);
                } catch (Saxon.Hej.trans.XPathException err) {
                    if ((err.isTypeError() && !(visitor.isInliningFunctions()))) {
                        throw;
                    } else {
                        Saxon.Hej.expr.ErrorExpression ee = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(actionOps[i].getChildExpression(), ee);
                        setAction(i, ee);
                    }
                }
                if (((((getAction(i) is Saxon.Hej.expr.ErrorExpression) && ((Saxon.Hej.expr.ErrorExpression)(getAction(i))).isTypeError()) && !(Saxon.Hej.expr.Literal.isConstantBoolean(getCondition(i), false))) && !(Saxon.Hej.expr.Literal.isConstantBoolean(getCondition(i), true)))) {
                    visitor.issueWarning(((("Branch "+((i+1)))+" of conditional will fail with a type error if executed. ")+((Saxon.Hej.expr.ErrorExpression)(getAction(i))).getMessage()), getAction(i).getLocation());
                }
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), true)) {
                    break ;
                }
            }
            if ((count == 0)) {
                return Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if (opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.CONSTANT_FOLDING)) {
                Saxon.Hej.expr.Expression e = removeRedundantBranches(visitor);
                if ((e is Saxon.Hej.expr.instruct.Choose)) {
                    return visitor.obtainOptimizer().trySwitch((Saxon.Hej.expr.instruct.Choose)(e), visitor);
                } else {
                    return e;
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            int count = size();
            Saxon.Hej.expr.Expression[] c2 = new Saxon.Hej.expr.Expression[count];
            Saxon.Hej.expr.Expression[] a2 = new Saxon.Hej.expr.Expression[count];
            for (int c = 0; (c<count); c++) {
                c2[c] = getCondition(c).copy(rebindings);
                a2[c] = getAction(c).copy(rebindings);
            }
            Saxon.Hej.expr.instruct.Choose ch2 = new Saxon.Hej.expr.instruct.Choose(c2, a2);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ch2);
            ch2.setInstruction(isInstruction());
            return ch2;
        }
        public override void checkForUpdatingSubexpressions() {
            foreach (Saxon.Hej.expr.Operand o in conditions()) {
                Saxon.Hej.expr.Expression condition = o.getChildExpression();
                condition.checkForUpdatingSubexpressions();
                if (condition.isUpdatingExpression()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Updating expression appears in a context where it is not permitted", "XUST0001");
                    err.setLocator(condition.getLocation());
                    throw err;
                }
            }
            bool updating = false;
            bool nonUpdating = false;
            foreach (Saxon.Hej.expr.Operand o in actions()) {
                Saxon.Hej.expr.Expression act = o.getChildExpression();
                act.checkForUpdatingSubexpressions();
                if (Saxon.Hej.expr.parser.ExpressionTool.isNotAllowedInUpdatingContext(act)) {
                    if (updating) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("If any branch of a conditional is an updating expression, then all must be updating expressions (or vacuous)", "XUST0001");
                        err.setLocator(act.getLocation());
                        throw err;
                    }
                    nonUpdating = true;
                }
                if (act.isUpdatingExpression()) {
                    if (nonUpdating) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("If any branch of a conditional is an updating expression, then all must be updating expressions (or vacuous)", "XUST0001");
                        err.setLocator(act.getLocation());
                        throw err;
                    }
                    updating = true;
                }
            }
        }
        public override bool isUpdatingExpression() {
            foreach (Saxon.Hej.expr.Operand o in actions()) {
                if (o.getChildExpression().isUpdatingExpression()) {
                    return true;
                }
            }
            return false;
        }
        public override bool isVacuousExpression() {
            foreach (Saxon.Hej.expr.Operand action in actions()) {
                if (!(action.getChildExpression().isVacuousExpression())) {
                    return false;
                }
            }
            return true;
        }
        public override int getImplementationMethod() {
            int m = ((Saxon.Hej.expr.Expression.PROCESS_METHOD|Saxon.Hej.expr.Expression.ITERATE_METHOD)|Saxon.Hej.expr.Expression.WATCH_METHOD);
            if (!(Saxon.Hej.value.Cardinality.allowsMany(getCardinality()))) {
                m |= Saxon.Hej.expr.Expression.EVALUATE_METHOD;
            }
            return m;
        }
        public override int markTailFunctionCalls(Saxon.Hej.om.StructuredQName qName, int arity) {
            int result = Saxon.Hej.expr.UserFunctionCall.NOT_TAIL_CALL;
            foreach (Saxon.Hej.expr.Operand action in actions()) {
                result = System.Math.Max(result, action.getChildExpression().markTailFunctionCalls(qName, arity));
            }
            return result;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType type = getAction(0).getItemType();
            for (int i = 1; (i<size()); i++) {
                type = Saxon.Hej.type.Type.getCommonSuperType(type, getAction(i).getItemType(), th);
            }
            return type;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if (isInstruction()) {
                return base.getStaticUType(contextItemType);
            } else {
                Saxon.Hej.type.UType type = getAction(0).getStaticUType(contextItemType);
                for (int i = 1; (i<size()); i++) {
                    type = type.union(getAction(i).getStaticUType(contextItemType));
                }
                return type;
            }
        }
        protected internal override int computeCardinality() {
            int card = 0;
            bool includesTrue = false;
            for (int i = 0; (i<size()); i++) {
                card = Saxon.Hej.value.Cardinality.union(card, getAction(i).getCardinality());
                if (Saxon.Hej.expr.Literal.hasEffectiveBooleanValue(getCondition(i), true)) {
                    includesTrue = true;
                }
            }
            if (!(includesTrue)) {
                card = Saxon.Hej.value.Cardinality.union(card, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
            }
            return card;
        }
        protected internal override int computeSpecialProperties() {
            int props = getAction(0).getSpecialProperties();
            for (int i = 1; (i<size()); i++) {
                props &= getAction(i).getSpecialProperties();
            }
            return props;
        }
        public override bool mayCreateNewNodes() {
            foreach (Saxon.Hej.expr.Operand action in actions()) {
                int props = action.getChildExpression().getSpecialProperties();
                if ((((props&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0)) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            for (int i = 0; (i<size()); i++) {
                setAction(i, getAction(i).unordered(retainAllNodes, forStreaming));
            }
            return this;
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            foreach (Saxon.Hej.expr.Operand action in actions()) {
                action.getChildExpression().checkPermittedContents(parentType, whole);
            }
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            foreach (Saxon.Hej.expr.Operand condition in conditions()) {
                condition.getChildExpression().addToPathMap(pathMap, pathMapNodeSet);
            }
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
            foreach (Saxon.Hej.expr.Operand action in actions()) {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet temp = action.getChildExpression().addToPathMap(pathMap, pathMapNodeSet);
                result.addNodeSet(temp);
            }
            return result;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
            sb.append("if (");
            for (int i = 0; (i<size()); i++) {
                sb.append(getCondition(i).ToString());
                sb.append(") then (");
                sb.append(getAction(i).ToString());
                if ((i == (size()-1))) {
                    sb.append(")");
                } else {
                    sb.append(") else if (");
                }
            }
            return sb.ToString();
        }
        public override string toShortString() {
            return (("if("+getCondition(0).toShortString())+") then ... else ...");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("choose", this);
            for (int i = 0; (i<size()); i++) {
                getCondition(i).export(@out);
                getAction(i).export(@out);
            }
            @out.endElement();
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            int i = choose(context);
            if ((i>=0)) {
                Saxon.Hej.expr.Expression action = getAction(i);
                if ((action is Saxon.Hej.expr.instruct.TailCallReturner)) {
                    return ((Saxon.Hej.expr.instruct.TailCallReturner)(action)).processLeavingTail(output, context);
                } else {
                    action.process(output, context);
                    return null;
                }
            }
            return null;
        }
        private int choose(Saxon.Hej.expr.XPathContext context) {
            int count = size();
            for (int i = 0; (i<count); i++) {
                bool b;
                try {
                    b = getCondition(i).effectiveBooleanValue(context);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetFailingExpression(getCondition(i));
                    throw;
                }
                if (b) {
                    return i;
                }
            }
            return -1;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            int i = choose(context);
            return ((i<0) ? null : getAction(i).evaluateItem(context));
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            int i = choose(context);
            return ((i<0) ? Saxon.Hej.tree.iter.EmptyIterator.emptyIterator() : getAction(i).iterate(context));
        }
        public override void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            int i = choose(context);
            if ((i>=0)) {
                getAction(i).evaluatePendingUpdates(context, pul);
            }
        }
        public override string getExpressionName() {
            return "choose";
        }
        public override string getStreamerName() {
            return "Choose";
        }
    
    }
}
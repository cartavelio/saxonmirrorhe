using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class ForClause : Saxon.Hej.expr.flwor.Clause {
        protected internal Saxon.Hej.expr.flwor.LocalVariableBinding rangeVariable;
        protected internal Saxon.Hej.expr.flwor.LocalVariableBinding positionVariable;
        protected internal Saxon.Hej.expr.Operand sequenceOp;
        protected internal Saxon.Hej.expr.elab.PullEvaluator sequenceOperandEvaluator;
        protected internal bool allowsEmpty;
        public ForClause()  {
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.FOR;
        }
        public override Saxon.Hej.expr.flwor.ForClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.ForClause f2 = new Saxon.Hej.expr.flwor.ForClause();
            f2.setLocation(getLocation());
            f2.setPackageData(getPackageData());
            f2.rangeVariable = rangeVariable.copy();
            if ((positionVariable != null)) {
                f2.positionVariable = positionVariable.copy();
            }
            f2.initSequence(flwor, getSequence().copy(rebindings));
            f2.allowsEmpty = allowsEmpty;
            return f2;
        }
        public void initSequence(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression sequence) {
            sequenceOp = new Saxon.Hej.expr.Operand(flwor, sequence, (isRepeated() ? Saxon.Hej.expr.OperandRole.REPEAT_NAVIGATE : Saxon.Hej.expr.OperandRole.NAVIGATE));
        }
        public void setSequence(Saxon.Hej.expr.Expression sequence) {
            sequenceOp.setChildExpression(sequence);
        }
        public Saxon.Hej.expr.Expression getSequence() {
            return sequenceOp.getChildExpression();
        }
        public Saxon.Hej.expr.Operand getSequenceOp() {
            return this.sequenceOp;
        }
        protected internal virtual Saxon.Hej.om.SequenceIterator getIterator(Saxon.Hej.expr.XPathContext context) {
            if ((sequenceOperandEvaluator == null)) {
                sequenceOperandEvaluator = getSequence().makeElaborator().elaborateForPull();
            }
            return /*Delegate.iterate()*/sequenceOperandEvaluator(context);
        }
        public void setRangeVariable(Saxon.Hej.expr.flwor.LocalVariableBinding binding) {
            rangeVariable = binding;
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding getRangeVariable() {
            return rangeVariable;
        }
        public void setPositionVariable(Saxon.Hej.expr.flwor.LocalVariableBinding binding) {
            positionVariable = binding;
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding getPositionVariable() {
            return positionVariable;
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            if ((positionVariable == null)) {
                return new Saxon.Hej.expr.flwor.LocalVariableBinding[]{rangeVariable};
            } else {
                return new Saxon.Hej.expr.flwor.LocalVariableBinding[]{rangeVariable, positionVariable};
            }
        }
        public void setAllowingEmpty(bool option) {
            allowsEmpty = option;
        }
        public bool isAllowingEmpty() {
            return allowsEmpty;
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.value.SequenceType decl = rangeVariable.getRequiredType();
            if ((allowsEmpty && !(Saxon.Hej.value.Cardinality.allowsZero(decl.getCardinality())))) {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> emptyRole = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, rangeVariable.getVariableQName().getDisplayName(), 0);
                Saxon.Hej.expr.Expression checker = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(getSequence(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE, emptyRole);
                setSequence(checker);
            }
            Saxon.Hej.value.SequenceType sequenceType = Saxon.Hej.value.SequenceType.makeSequenceType(decl.getPrimaryType(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, rangeVariable.getVariableQName().getDisplayName(), 0);
            setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), sequenceType, role, visitor.getStaticContext()));
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            if (allowsEmpty) {
                return new Saxon.Hej.expr.flwor.ForClauseOuterPull(@base, this);
            } else {
                return new Saxon.Hej.expr.flwor.ForClausePull(@base, this);
            }
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if (allowsEmpty) {
                return new Saxon.Hej.expr.flwor.ForClauseOuterPush(output, destination, this);
            } else {
                return new Saxon.Hej.expr.flwor.ForClausePush(output, destination, this);
            }
        }
        public virtual bool addPredicate(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType, Saxon.Hej.expr.Expression condition) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            bool debug = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.TRACE_OPTIMIZER_DECISIONS);
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            Saxon.Hej.expr.Expression head = null;
            Saxon.Hej.expr.Expression selection = getSequence();
            Saxon.Hej.type.ItemType selectionContextItemType = ((contextItemType == null) ? null : contextItemType.getItemType());
            if ((getSequence() is Saxon.Hej.expr.SlashExpression)) {
                if (((Saxon.Hej.expr.SlashExpression)(getSequence())).isAbsolute()) {
                    head = ((Saxon.Hej.expr.SlashExpression)(getSequence())).getFirstStep();
                    selection = ((Saxon.Hej.expr.SlashExpression)(getSequence())).getRemainingSteps();
                    selectionContextItemType = head.getItemType();
                } else {
                    Saxon.Hej.expr.SlashExpression p = ((Saxon.Hej.expr.SlashExpression)(getSequence())).tryToMakeAbsolute();
                    if ((p != null)) {
                        setSequence(p);
                        head = p.getFirstStep();
                        selection = p.getRemainingSteps();
                        selectionContextItemType = head.getItemType();
                    }
                }
            }
            bool changed = false;
            if (((positionVariable != null) && (positionVariable.getNominalReferenceCount() == 0))) {
                positionVariable = null;
            }
            if ((((positionVariable != null) && ((((condition is Saxon.Hej.expr.ValueComparison) || (condition is Saxon.Hej.expr.GeneralComparison)) || (condition is Saxon.Hej.expr.CompareToIntegerConstant)))) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(condition, new Saxon.Hej.expr.Binding[]{positionVariable}))) {
                Saxon.Hej.expr.ComparisonExpression comp = (Saxon.Hej.expr.ComparisonExpression)(condition);
                Saxon.Hej.expr.Expression[] operands = new Saxon.Hej.expr.Expression[]{comp.getLhsExpression(), comp.getRhsExpression()};
                if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(flwor, new Saxon.Hej.expr.Binding[]{positionVariable})) {
                    return false;
                }
                for (int op = 0; (op<2); op++) {
                    Saxon.Hej.expr.Binding[] thisVar = {this.getRangeVariable()};
                    if ((((positionVariable != null) && (operands[op] is Saxon.Hej.expr.VariableReference)) && !(changed))) {
                        System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> varRefs = new System.Collections.Generic.List<Saxon.Hej.expr.VariableReference>();
                        Saxon.Hej.expr.parser.ExpressionTool.gatherVariableReferences(condition, positionVariable, varRefs);
                        if (((((varRefs.Count == 1) && (varRefs[0] == operands[op])) && !(Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(operands[(1-op)]))) && !(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(operands[(1-op)], thisVar)))) {
                            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(visitor.getStaticContext());
                            Saxon.Hej.expr.Expression position = Saxon.Hej.functions.SystemFunction.makeCall("position", rsc);
                            Saxon.Hej.expr.Expression predicate = condition.copy(new Saxon.Hej.expr.parser.RebindingMap());
                            Saxon.Hej.expr.Operand child = ((op == 0) ? ((Saxon.Hej.expr.ComparisonExpression)(predicate)).getLhs() : ((Saxon.Hej.expr.ComparisonExpression)(predicate)).getRhs());
                            child.setChildExpression(position);
                            if (debug) {
                                opt.trace("Replaced positional variable in predicate by position()", predicate);
                            }
                            selection = new Saxon.Hej.expr.FilterExpression(selection, predicate);
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(predicate, selection);
                            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(selectionContextItemType, true);
                            selection = selection.typeCheck(visitor, cit);
                            if (!(Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(flwor, new Saxon.Hej.expr.Binding[]{positionVariable}))) {
                                positionVariable = null;
                            }
                            changed = true;
                            break ;
                        }
                    }
                }
            }
            if ((positionVariable == null)) {
                Saxon.Hej.expr.Binding[] thisVar = {this.getRangeVariable()};
                if (opt.isVariableReplaceableByDot(condition, thisVar)) {
                    Saxon.Hej.expr.Expression replacement = new Saxon.Hej.expr.ContextItemExpression();
                    bool found = Saxon.Hej.expr.parser.ExpressionTool.inlineVariableReferences(condition, this.getRangeVariable(), replacement);
                    if (found) {
                        Saxon.Hej.expr.parser.ContextItemStaticInfo cit = config.makeContextItemStaticInfo(getSequence().getItemType(), true);
                        Saxon.Hej.expr.Expression predicate = condition.typeCheck(visitor, cit);
                        Saxon.Hej.type.Affinity rel = th.relationship(predicate.getItemType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER);
                        if ((rel != Saxon.Hej.type.Affinity.DISJOINT)) {
                            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(visitor.getStaticContext());
                            predicate = Saxon.Hej.functions.SystemFunction.makeCall("boolean", rsc, predicate);
                        }
                        selection = new Saxon.Hej.expr.FilterExpression(selection, predicate);
                        Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(predicate, selection);
                        cit = config.makeContextItemStaticInfo(selectionContextItemType, true);
                        selection = selection.typeCheck(visitor, cit);
                        changed = true;
                    }
                }
            }
            if (changed) {
                if ((head == null)) {
                    setSequence(selection);
                } else {
                    if (((head is Saxon.Hej.expr.RootExpression) && selection.isCallOn(typeof(Saxon.Hej.functions.KeyFn)))) {
                        setSequence(selection);
                    } else {
                        Saxon.Hej.expr.Expression path = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(head, selection);
                        if ((path is Saxon.Hej.expr.SlashExpression)) {
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(condition, path);
                            Saxon.Hej.expr.Expression k = visitor.obtainOptimizer().convertPathExpressionToKey((Saxon.Hej.expr.SlashExpression)(path), visitor);
                            setSequence(((k == null) ? path : k));
                            sequenceOp.typeCheck(visitor, contextItemType);
                            sequenceOp.optimize(visitor, contextItemType);
                        }
                    }
                }
            }
            return changed;
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(sequenceOp);
        }
        public override void gatherVariableReferences(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Binding binding, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references) {
            Saxon.Hej.expr.parser.ExpressionTool.gatherVariableReferences(getSequence(), binding, references);
        }
        public override void refineVariableType(Saxon.Hej.expr.parser.ExpressionVisitor visitor, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, Saxon.Hej.expr.Expression returnExpr) {
            Saxon.Hej.type.ItemType actualItemType = getSequence().getItemType();
            if ((actualItemType is Saxon.Hej.type.ErrorType)) {
                actualItemType = Saxon.Hej.type.AnyItemType.getInstance();
            }
            foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                @ref.refineVariableType(actualItemType, (allowsEmpty ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE : Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), null, getSequence().getSpecialProperties());
            }
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet varPath = getSequence().addToPathMap(pathMap, pathMapNodeSet);
            pathMap.registerPathForVariable(rangeVariable, varPath);
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement(getClauseKey().ToString().ToLower());
            @out.emitAttribute("var", getRangeVariable().getVariableQName());
            @out.emitAttribute("slot", (getRangeVariable().getLocalSlotNumber()+""));
            Saxon.Hej.expr.flwor.LocalVariableBinding posVar = getPositionVariable();
            if ((posVar != null)) {
                @out.emitAttribute("at", posVar.getVariableQName());
                @out.emitAttribute("at-slot", (posVar.getLocalSlotNumber()+""));
            }
            getSequence().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            return stringify(true);
        }
        public override string ToString() {
            return stringify(false);
        }
        private string stringify(bool abbreviate) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append(getClauseKey().ToString().ToLower());
            fsb.append(" $");
            fsb.append(rangeVariable.getVariableQName().getDisplayName());
            fsb.append(' ');
            Saxon.Hej.expr.flwor.LocalVariableBinding posVar = getPositionVariable();
            if ((posVar != null)) {
                fsb.append("at $");
                fsb.append(posVar.getVariableQName().getDisplayName());
                fsb.append(' ');
            }
            fsb.append("in ");
            fsb.append((abbreviate ? getSequence().toShortString() : getSequence().ToString()));
            return fsb.ToString();
        }
    
    }
}
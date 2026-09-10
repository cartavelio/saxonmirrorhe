using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class Assignation : Saxon.Hej.expr.Expression, Saxon.Hej.expr.LocalBinding {
        private readonly Saxon.Hej.expr.Operand sequenceOp;
        private readonly Saxon.Hej.expr.Operand actionOp;
        protected internal int slotNumber = -999;
        protected internal Saxon.Hej.om.StructuredQName variableName;
        protected internal Saxon.Hej.value.SequenceType requiredType;
        protected internal bool indexedVariable = false;
        protected internal bool hasLoopingReference = false;
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references = null;
        private readonly static Saxon.Hej.expr.OperandRole REPEATED_ACTION_ROLE = new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.HIGHER_ORDER, Saxon.Hej.expr.OperandUsage.TRANSMISSION);
        public Assignation()  {
            sequenceOp = new Saxon.Hej.expr.Operand(this, null, Saxon.Hej.expr.OperandRole.NAVIGATE);
            actionOp = new Saxon.Hej.expr.Operand(this, null, ((this is Saxon.Hej.expr.LetExpression) ? Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION : REPEATED_ACTION_ROLE));
        }
        public Saxon.Hej.expr.Operand getSequenceOp() {
            return sequenceOp;
        }
        public Saxon.Hej.expr.Operand getActionOp() {
            return actionOp;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(sequenceOp, actionOp);
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType requiredType) {
            this.requiredType = requiredType;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName variableName) {
            this.variableName = variableName;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableName;
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            return variableName;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
            return getSequence().getIntegerBounds();
        }
        public int getLocalSlotNumber() {
            return slotNumber;
        }
        public override int computeDependencies() {
            int d = base.computeDependencies();
            if (!(Saxon.Hej.expr.parser.ExpressionTool.containsLocalVariableReference(this))) {
                d &= ~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES);
            }
            return d;
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence actual = context.evaluateLocalVariable(slotNumber);
            if (!((((actual is Saxon.Hej.om.GroundedValue) || (actual is Saxon.Hej.om.NodeInfo))))) {
                actual = actual.materialize();
                context.setLocalVariable(slotNumber, actual);
            }
            return actual;
        }
        public void setAction(Saxon.Hej.expr.Expression action) {
            actionOp.setChildExpression(action);
        }
        public bool isGlobal() {
            return false;
        }
        public bool isAssignable() {
            return false;
        }
        public override void checkForUpdatingSubexpressions() {
            getSequence().checkForUpdatingSubexpressions();
            if (getSequence().isUpdatingExpression()) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("An updating expression cannot be used to initialize a variable", "XUST0001");
                err.setLocator(getSequence().getLocation());
                throw err;
            }
            getAction().checkForUpdatingSubexpressions();
        }
        public override bool isUpdatingExpression() {
            return getAction().isUpdatingExpression();
        }
        public Saxon.Hej.expr.Expression getAction() {
            return actionOp.getChildExpression();
        }
        public void setSequence(Saxon.Hej.expr.Expression sequence) {
            sequenceOp.setChildExpression(sequence);
        }
        public Saxon.Hej.expr.Expression getSequence() {
            return sequenceOp.getChildExpression();
        }
        public void setSlotNumber(int nr) {
            slotNumber = nr;
        }
        public int getRequiredSlots() {
            return 1;
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            return (this == binding);
        }
        public override Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            setAction(getAction().unordered(retainAllNodes, forStreaming));
            return this;
        }
        public override double getCost() {
            return (getSequence().getCost()+(5*getAction().getCost()));
        }
        public override void suppressValidation(int validationMode) {
            getAction().suppressValidation(validationMode);
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet varPath = getSequence().addToPathMap(pathMap, pathMapNodeSet);
            pathMap.registerPathForVariable(this, varPath);
            return getAction().addToPathMap(pathMap, pathMapNodeSet);
        }
        public string getVariableName() {
            if ((variableName == null)) {
                return ("zz:var"+computeHashCode());
            } else {
                return variableName.getDisplayName();
            }
        }
        public string getVariableEQName() {
            if ((variableName == null)) {
                return ("Q{http://ns.saxonica.com/anonymous-var}var"+computeHashCode());
            } else {
                if (variableName.hasURI("")) {
                    return variableName.getLocalPart();
                } else {
                    return variableName.getEQName();
                }
            }
        }
        public void refineTypeInformation(Saxon.Hej.type.ItemType type, int cardinality, Saxon.Hej.om.GroundedValue constantValue, int properties, Saxon.Hej.expr.Assignation currentExpression) {
            Saxon.Hej.expr.parser.ExpressionTool.processExpressionTree(currentExpression.getAction(), null, (exp, result) => {
                if (((exp is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(exp)).getBinding() == currentExpression))) {
                    ((Saxon.Hej.expr.VariableReference)(exp)).refineVariableType(type, cardinality, constantValue, properties);
                }
                return false;
            } );
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
            hasLoopingReference |= isLoopingReference;
            if ((references == null)) {
                references = new System.Collections.Generic.List<Saxon.Hej.expr.VariableReference>();
            }
            foreach (Saxon.Hej.expr.VariableReference vr in references) {
                if ((vr == @ref)) {
                    return ;
                }
            }
            references.Add(@ref);
        }
        public int getNominalReferenceCount() {
            if (indexedVariable) {
                return Saxon.Hej.expr.FilterExpression.FILTERED;
            } else {
                if (((references == null) || hasLoopingReference)) {
                    return 10;
                } else {
                    return references.Count;
                }
            }
        }
        protected internal bool removeDeadReferences() {
            bool inLoop = false;
            if ((references != null)) {
                for (int i = (references.Count-1); (i>=0); i--) {
                    bool found = false;
                    inLoop |= references[i].isInLoop();
                    Saxon.Hej.expr.Expression parent = references[i].getParentExpression();
                    while ((parent != null)) {
                        if ((parent == this)) {
                            found = true;
                            break ;
                        } else {
                            parent = parent.getParentExpression();
                        }
                    }
                    if (!(found)) {
                        Saxon.Impl.Helpers.ListUtils.Remove(references, i);
                    }
                }
            }
            return inLoop;
        }
        protected internal void verifyReferences() {
            rebuildReferenceList(false);
        }
        public void rebuildReferenceList(bool force) {
            int[] results = new int[]{0, (force ? System.Int32.MaxValue : 500)};
            System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references = new System.Collections.Generic.List<Saxon.Hej.expr.VariableReference>();
            countReferences(this, getAction(), references, results);
            this.references = ((results[1]<=0) ? null : references);
        }
        private static void countReferences(Saxon.Hej.expr.Binding binding, Saxon.Hej.expr.Expression exp, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, int[] results) {
            if ((exp is Saxon.Hej.expr.LocalVariableReference)) {
                Saxon.Hej.expr.LocalVariableReference @ref = (Saxon.Hej.expr.LocalVariableReference)(exp);
                if ((@ref.getBinding() == binding)) {
                    @ref.recomputeInLoop();
                    results[0] += (@ref.isInLoop() ? 10 : 1);
                    references.Add((Saxon.Hej.expr.LocalVariableReference)(exp));
                }
            } else {
                if ((((exp.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) != 0)) {
                    if ((--results[1]<=0)) {
                        results[0] = 100;
                        results[1] = 0;
                    } else {
                        foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                            countReferences(binding, o.getChildExpression(), references, results);
                        }
                    }
                }
            }
        }
        public bool isIndexedVariable() {
            return indexedVariable;
        }
        public bool replaceVariable(Saxon.Hej.expr.Expression seq) {
            bool done = Saxon.Hej.expr.parser.ExpressionTool.inlineVariableReferences(getAction(), this, seq);
            if (((done && isIndexedVariable()) && (seq is Saxon.Hej.expr.VariableReference))) {
                Saxon.Hej.expr.Binding newBinding = ((Saxon.Hej.expr.VariableReference)(seq)).getBinding();
                if ((newBinding is Saxon.Hej.expr.Assignation)) {
                    ((Saxon.Hej.expr.Assignation)(newBinding)).setIndexedVariable();
                }
            }
            return done;
        }
        public void setIndexedVariable() {
            indexedVariable = true;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class LetExpression : Saxon.Hej.expr.Assignation {
        private Saxon.Hej.expr.elab.SequenceEvaluator evaluator = null;
        private bool needsEagerEvaluation = false;
        private bool needsLazyEvaluation = false;
        private bool _isInstruction;
        public LetExpression()  {
        }
        public void setInstruction(bool inst) {
            _isInstruction = inst;
        }
        public override bool isInstruction() {
            return _isInstruction;
        }
        public override string getExpressionName() {
            return "let";
        }
        public void setNeedsEagerEvaluation(bool req) {
            if ((req && needsLazyEvaluation)) {
            }
            this.needsEagerEvaluation = req;
        }
        public void setNeedsLazyEvaluation(bool req) {
            if ((req && needsEagerEvaluation)) {
                this.needsEagerEvaluation = false;
            }
            this.needsLazyEvaluation = req;
        }
        public bool isNeedsLazyEvaluation() {
            return needsLazyEvaluation;
        }
        public bool isNeedsEagerEvaluation() {
            return needsEagerEvaluation;
        }
        public override bool supportsLazyEvaluation() {
            return !(needsEagerEvaluation);
        }
        public override bool isLiftable(bool forStreaming) {
            return (base.isLiftable(forStreaming) && !(needsEagerEvaluation));
        }
        public override void resetLocalStaticProperties() {
            base.resetLocalStaticProperties();
            references = new System.Collections.Generic.List<Saxon.Hej.expr.VariableReference>();
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            getSequenceOp().typeCheck(visitor, contextInfo);
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, getVariableQName().getDisplayName(), 0);
            setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), requiredType, role, visitor.getStaticContext()));
            Saxon.Hej.type.ItemType actualItemType = getSequence().getItemType();
            refineTypeInformation(actualItemType, getSequence().getCardinality(), ((getSequence() is Saxon.Hej.expr.Literal) ? ((Saxon.Hej.expr.Literal)(getSequence())).getGroundedValue() : null), getSequence().getSpecialProperties(), this);
            getActionOp().typeCheck(visitor, contextInfo);
            return this;
        }
        public override bool implementsStaticTypeCheck() {
            return true;
        }
        public override Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.value.SequenceType req, bool backwardsCompatible, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(backwardsCompatible);
            setAction(tc.staticTypeCheck(getAction(), req, roleSupplier, visitor));
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.Optimizer opt = visitor.obtainOptimizer();
            if ((((getAction() is Saxon.Hej.expr.VariableReference) && (((Saxon.Hej.expr.VariableReference)(getAction())).getBinding() == this)) && !(Saxon.Hej.expr.parser.ExpressionTool.changesXsltContext(getSequence())))) {
                getSequenceOp().optimize(visitor, contextItemType);
                opt.trace(("Eliminated trivial variable "+getVariableName()), getSequence());
                return getSequence();
            }
            if (((getSequence() is Saxon.Hej.expr.Literal) && opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.INLINE_VARIABLES))) {
                opt.trace(("Inlined constant variable "+getVariableName()), getSequence());
                replaceVariable(getSequence());
                return getAction().optimize(visitor, contextItemType);
            }
            if (((getSequence() is Saxon.Hej.expr.instruct.DocumentInstr) && ((Saxon.Hej.expr.instruct.DocumentInstr)(getSequence())).isTextOnly())) {
                verifyReferences();
                if (allReferencesAreFlattened()) {
                    Saxon.Hej.expr.Expression stringValueExpression = ((Saxon.Hej.expr.instruct.DocumentInstr)(getSequence())).getStringValueExpression();
                    stringValueExpression = stringValueExpression.typeCheck(visitor, contextItemType);
                    setSequence(stringValueExpression);
                    requiredType = Saxon.Hej.value.SequenceType.SINGLE_UNTYPED_ATOMIC;
                    adoptChildExpression(getSequence());
                    refineTypeInformation(requiredType.getPrimaryType(), requiredType.getCardinality(), null, 0, this);
                }
            }
            if (getSequence().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS)) {
                needsEagerEvaluation = true;
            }
            hasLoopingReference |= removeDeadReferences();
            if (!(needsEagerEvaluation)) {
                bool considerRemoval = ((((((((references != null) && (references.Count<2))) || (getSequence() is Saxon.Hej.expr.VariableReference))) && !(indexedVariable)) && !(hasLoopingReference)) && !(needsEagerEvaluation));
                if (considerRemoval) {
                    verifyReferences();
                    considerRemoval = (references != null);
                }
                if ((considerRemoval && references.Count==0)) {
                    getActionOp().optimize(visitor, contextItemType);
                    opt.trace(("Eliminated unused variable "+getVariableName()), getAction());
                    return getAction();
                }
                if (((considerRemoval && (references.Count == 1)) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(getSequence()))) {
                    if (visitor.isOptimizeForStreaming()) {
                        considerRemoval = false;
                    }
                    Saxon.Hej.expr.Expression child = references[0];
                    Saxon.Hej.expr.Expression parent = child.getParentExpression();
                    while (((parent != null) && (parent != this))) {
                        Saxon.Hej.expr.Operand operand = Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, child);
                        System.Diagnostics.Debug.Assert((operand != null));
                        if (!(operand.hasSameFocus())) {
                            considerRemoval = false;
                            break ;
                        }
                        child = parent;
                        parent = child.getParentExpression();
                    }
                }
                if ((considerRemoval && (references.Count == 1))) {
                    if (Saxon.Hej.expr.parser.ExpressionTool.changesXsltContext(getSequence())) {
                        considerRemoval = false;
                    } else {
                        if ((((getSequence().getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_GROUP)) != 0)) {
                            considerRemoval = false;
                        } else {
                            if (references[0].isInLoop()) {
                                considerRemoval = false;
                            }
                        }
                    }
                }
                if (((considerRemoval && ((((references.Count == 1) || (getSequence() is Saxon.Hej.expr.Literal)) || (getSequence() is Saxon.Hej.expr.VariableReference)))) && opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.INLINE_VARIABLES))) {
                    inlineReferences();
                    opt.trace(("Inlined references to $"+getVariableName()), getAction());
                    references = null;
                    return getAction().optimize(visitor, contextItemType);
                }
            }
            int tries = 0;
            while ((tries++<5)) {
                Saxon.Hej.expr.Expression seq0 = getSequence();
                getSequenceOp().optimize(visitor, contextItemType);
                if ((((getSequence() is Saxon.Hej.expr.Literal) && !(indexedVariable)) && opt.isOptionSet(Saxon.Hej.expr.parser.OptimizerOptions.INLINE_VARIABLES))) {
                    return optimize(visitor, contextItemType);
                }
                if ((seq0 == getSequence())) {
                    break ;
                }
            }
            tries = 0;
            while ((tries++<5)) {
                Saxon.Hej.expr.Expression act0 = getAction();
                getActionOp().optimize(visitor, contextItemType);
                if ((act0 == getAction())) {
                    break ;
                }
                if ((!(indexedVariable) && !(needsEagerEvaluation))) {
                    verifyReferences();
                    if (((references != null) && (references.Count<2))) {
                        if (references.Count==0) {
                            hasLoopingReference = false;
                            return optimize(visitor, contextItemType);
                        } else {
                            if (!(references[0].isInLoop())) {
                                return optimize(visitor, contextItemType);
                            }
                        }
                    }
                }
            }
            return this;
        }
        public void setEvaluator() {
            if (isIndexedVariable()) {
                Saxon.Hej.expr.elab.PullEvaluator pullEval = getSequence().makeElaborator().elaborateForPull();
                setEvaluator(new Saxon.Hej.expr.elab.IndexedVariableEvaluator(pullEval));
            } else {
                if ((needsEagerEvaluation || !(getSequence().supportsLazyEvaluation()))) {
                    setEvaluator(getSequence().makeElaborator().eagerly());
                } else {
                    if (needsLazyEvaluation) {
                        setEvaluator(getSequence().makeElaborator().lazily((getNominalReferenceCount()>1)));
                    } else {
                        if ((evaluator == null)) {
                            setEvaluator(new Saxon.Hej.expr.elab.LearningEvaluator(getSequence(), getSequence().makeElaborator().lazily((getNominalReferenceCount()>1))));
                        }
                    }
                }
            }
        }
        private void inlineReferences() {
            foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                Saxon.Hej.expr.Expression parent = @ref.getParentExpression();
                if ((parent != null)) {
                    Saxon.Hej.expr.Operand o = Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, @ref);
                    if ((o != null)) {
                        o.setChildExpression(getSequence().copy(new Saxon.Hej.expr.parser.RebindingMap()));
                    }
                    Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(parent);
                }
            }
        }
        public override double getCost() {
            return (getSequence().getCost()+getAction().getCost());
        }
        private bool allReferencesAreFlattened() {
            if ((references != null)) {
                foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                    if (!(@ref.isFlattened())) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        public override bool isVacuousExpression() {
            return getAction().isVacuousExpression();
        }
        public override void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
            getAction().checkPermittedContents(parentType, whole);
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return getAction().getIntegerBounds();
        }
        public override int getImplementationMethod() {
            return getAction().getImplementationMethod();
        }
        public override void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getVariableQName());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.LetExpression let = this;
            while (true) {
                Saxon.Hej.om.Sequence val = let.eval(context);
                context.setLocalVariable(let.getLocalSlotNumber(), val);
                if ((let.getAction() is Saxon.Hej.expr.LetExpression)) {
                    let = (Saxon.Hej.expr.LetExpression)(let.getAction());
                } else {
                    break ;
                }
            }
            return let.getAction().iterate(context);
        }
        public Saxon.Hej.om.Sequence eval(Saxon.Hej.expr.XPathContext context) {
            if ((evaluator == null)) {
                if (needsEagerEvaluation) {
                    setEvaluator(getSequence().makeElaborator().eagerly());
                } else {
                    setEvaluator(new Saxon.Hej.expr.elab.LearningEvaluator(getSequence(), getSequence().makeElaborator().lazily((getNominalReferenceCount()>1))));
                }
            }
            try {
                int savedOutputState = context.getTemporaryOutputState();
                context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                Saxon.Hej.om.Sequence result = evaluator.evaluate(context);
                context.setTemporaryOutputState(savedOutputState);
                return result;
            } catch (System.InvalidCastException e) {
                System.Diagnostics.Debug.Assert(false);
                int savedOutputState = context.getTemporaryOutputState();
                context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                Saxon.Hej.om.Sequence result = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(getSequence(), context);
                context.setTemporaryOutputState(savedOutputState);
                return result;
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.eval()*/makeElaborator().elaborateForItem()(context);
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.LetExpression let = this;
            while (true) {
                Saxon.Hej.om.Sequence val = let.eval(context);
                context.setLocalVariable(let.getLocalSlotNumber(), val);
                if ((let.getAction() is Saxon.Hej.expr.LetExpression)) {
                    let = (Saxon.Hej.expr.LetExpression)(let.getAction());
                } else {
                    break ;
                }
            }
            return let.getAction().effectiveBooleanValue(context);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            dispatchTailCall(/*Delegate.processLeavingTail()*/makeElaborator().elaborateForPush()(output, context));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return getAction().getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if (isInstruction()) {
                return Saxon.Hej.type.UType.ANY;
            }
            return getAction().getStaticUType(contextItemType);
        }
        protected internal override int computeCardinality() {
            return getAction().getCardinality();
        }
        protected internal override int computeSpecialProperties() {
            int props = getAction().getSpecialProperties();
            int seqProps = getSequence().getSpecialProperties();
            if ((((seqProps&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) == 0)) {
                props &= ~(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
            }
            return props;
        }
        public override int markTailFunctionCalls(Saxon.Hej.om.StructuredQName qName, int arity) {
            return Saxon.Hej.expr.parser.ExpressionTool.markTailFunctionCalls(getAction(), qName, arity);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
            rebindings.put(this, let);
            let.indexedVariable = indexedVariable;
            let.hasLoopingReference = hasLoopingReference;
            let.setNeedsEagerEvaluation(needsEagerEvaluation);
            let.setNeedsLazyEvaluation(needsLazyEvaluation);
            let.setVariableQName(variableName);
            let.setRequiredType(requiredType);
            let.setSequence(getSequence().copy(rebindings));
            let.setInstruction(isInstruction());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, let);
            Saxon.Hej.expr.Expression newAction = getAction().copy(rebindings);
            let.setAction(newAction);
            return let;
        }
        public override string ToString() {
            return ((((("let $"+getVariableEQName())+" := ")+getSequence())+" return ")+Saxon.Hej.expr.parser.ExpressionTool.parenthesize(getAction()));
        }
        public override string toShortString() {
            return (("let $"+getVariableName())+" := ...");
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("let", this);
            @out.emitAttribute("var", variableName);
            if ((getRequiredType() != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                @out.emitAttribute("as", getRequiredType().toAlphaCode());
            }
            if (isIndexedVariable()) {
                @out.emitAttribute("indexable", "true");
            }
            @out.emitAttribute("slot", (getLocalSlotNumber()+""));
            if ((needsEagerEvaluation || needsLazyEvaluation)) {
                string flags = (((needsEagerEvaluation ? "e" : ""))+((needsLazyEvaluation ? "l" : "")));
                @out.emitAttribute("flags", flags);
            }
            getSequence().export(@out);
            getAction().export(@out);
            @out.endElement();
        }
        public void setEvaluator(Saxon.Hej.expr.elab.SequenceEvaluator evaluator) {
            this.evaluator = evaluator;
        }
        public Saxon.Hej.expr.elab.SequenceEvaluator getEvaluator() {
            return evaluator;
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.LetExpression.LetExprElaborator();
        }
        public class LetExprElaborator : Saxon.Hej.expr.elab.PullElaborator {
            private Saxon.Hej.expr.Expression finalAction;
            private bool repeatable(Saxon.Hej.expr.LetExpression let) {
                return (let.getNominalReferenceCount()>1);
            }
            private Saxon.Hej.expr.elab.SequenceEvaluator makeSequenceEvaluator(Saxon.Hej.expr.LetExpression let) {
                if ((let.evaluator != null)) {
                    return let.evaluator;
                }
                let.setEvaluator();
                return let.evaluator;
            }
            private Saxon.Hej.expr.elab.ItemEvaluator setAllVariables(Saxon.Hej.expr.LetExpression start) {
                System.Collections.Generic.IList<Saxon.Hej.expr.LetExpression> setters = new System.Collections.Generic.List<Saxon.Hej.expr.LetExpression>();
                setters.Add(start);
                Saxon.Hej.expr.Expression next = start.getAction();
                while ((next is Saxon.Hej.expr.LetExpression)) {
                    setters.Add((Saxon.Hej.expr.LetExpression)(next));
                    next = ((Saxon.Hej.expr.LetExpression)(next)).getAction();
                }
                finalAction = next;
                switch (setters.Count) {
                  case 1:
                    {
                        Saxon.Hej.expr.LetExpression let = setters[0];
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator = makeSequenceEvaluator(let);
                        int slot = setters[0].slotNumber;
                        return context => {
                            context.setLocalVariable(slot, evaluator.evaluate(context));
                            return null;
                        } ;
                    }
                  case 2:
                    {
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator0 = makeSequenceEvaluator(setters[0]);
                        int slot0 = setters[0].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator1 = makeSequenceEvaluator(setters[1]);
                        int slot1 = setters[1].slotNumber;
                        return context => {
                            context.setLocalVariable(slot0, evaluator0.evaluate(context));
                            context.setLocalVariable(slot1, evaluator1.evaluate(context));
                            return null;
                        } ;
                    }
                  case 3:
                    {
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator0 = makeSequenceEvaluator(setters[0]);
                        int slot0 = setters[0].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator1 = makeSequenceEvaluator(setters[1]);
                        int slot1 = setters[1].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator2 = makeSequenceEvaluator(setters[2]);
                        int slot2 = setters[2].slotNumber;
                        return context => {
                            context.setLocalVariable(slot0, evaluator0.evaluate(context));
                            context.setLocalVariable(slot1, evaluator1.evaluate(context));
                            context.setLocalVariable(slot2, evaluator2.evaluate(context));
                            return null;
                        } ;
                    }
                  case 4:
                    {
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator0 = makeSequenceEvaluator(setters[0]);
                        int slot0 = setters[0].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator1 = makeSequenceEvaluator(setters[1]);
                        int slot1 = setters[1].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator2 = makeSequenceEvaluator(setters[2]);
                        int slot2 = setters[2].slotNumber;
                        Saxon.Hej.expr.elab.SequenceEvaluator evaluator3 = makeSequenceEvaluator(setters[3]);
                        int slot3 = setters[3].slotNumber;
                        return context => {
                            context.setLocalVariable(slot0, evaluator0.evaluate(context));
                            context.setLocalVariable(slot1, evaluator1.evaluate(context));
                            context.setLocalVariable(slot2, evaluator2.evaluate(context));
                            context.setLocalVariable(slot3, evaluator3.evaluate(context));
                            return null;
                        } ;
                    }
                  default:
                    {
                        Saxon.Hej.expr.elab.SequenceEvaluator[] evaluators = new Saxon.Hej.expr.elab.SequenceEvaluator[setters.Count];
                        int[] slots = new int[setters.Count];
                        for (int i = 0; (i<setters.Count); i++) {
                            evaluators[i] = makeSequenceEvaluator(setters[i]);
                            slots[i] = setters[i].slotNumber;
                        }
                        return context => {
                            for (int i = 0; (i<slots.Length); i++) {
                                context.setLocalVariable(slots[i], evaluators[i].evaluate(context));
                            }
                            return null;
                        } ;
                    }
                }
            }
            public override Saxon.Hej.expr.elab.SequenceEvaluator eagerly() {
                Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.expr.elab.SequenceEvaluator selectEval = expr.getSequence().makeElaborator().eagerly();
                Saxon.Hej.expr.elab.SequenceEvaluator actionEval = expr.getAction().makeElaborator().eagerly();
                int slot = expr.getLocalSlotNumber();
                return new Saxon.Hej.expr.LetExpression.EagerLocalVariableEvaluator(slot, selectEval, actionEval);
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator setter = setAllVariables(expr);
                Saxon.Hej.expr.elab.PullEvaluator actionPull = finalAction.makeElaborator().elaborateForPull();
                return context => {
                    int savedOutputState = context.getTemporaryOutputState();
                    context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                    /*Delegate.eval()*/setter(context);
                    context.setTemporaryOutputState(savedOutputState);
                    return /*Delegate.iterate()*/actionPull(context);
                } ;
            }
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator setter = setAllVariables(expr);
                Saxon.Hej.expr.elab.PushEvaluator actionPush = finalAction.makeElaborator().elaborateForPush();
                return (@out, context) => {
                    int savedOutputState = context.getTemporaryOutputState();
                    context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                    /*Delegate.eval()*/setter(context);
                    context.setTemporaryOutputState(savedOutputState);
                    return /*Delegate.processLeavingTail()*/actionPush(@out, context);
                } ;
            }
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator setter = setAllVariables(expr);
                Saxon.Hej.expr.elab.ItemEvaluator actionEval = finalAction.makeElaborator().elaborateForItem();
                return context => {
                    int savedOutputState = context.getTemporaryOutputState();
                    context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                    /*Delegate.eval()*/setter(context);
                    context.setTemporaryOutputState(savedOutputState);
                    return /*Delegate.eval()*/actionEval(context);
                } ;
            }
            public override Saxon.Hej.expr.elab.UpdateEvaluator elaborateForUpdate() {
                Saxon.Hej.expr.LetExpression expr = (Saxon.Hej.expr.LetExpression)(getExpression());
                Saxon.Hej.expr.elab.ItemEvaluator setter = setAllVariables(expr);
                Saxon.Hej.expr.elab.UpdateEvaluator actionEval = finalAction.makeElaborator().elaborateForUpdate();
                return (context, pul) => {
                    int savedOutputState = context.getTemporaryOutputState();
                    context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                    /*Delegate.eval()*/setter(context);
                    context.setTemporaryOutputState(savedOutputState);
                    /*Delegate.registerUpdates()*/actionEval(context, pul);
                } ;
            }
        
        }
        private class EagerLocalVariableEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
            private readonly int slot;
            private readonly Saxon.Hej.expr.elab.SequenceEvaluator selectEval;
            private readonly Saxon.Hej.expr.elab.SequenceEvaluator actionEval;
            public EagerLocalVariableEvaluator(int slot, Saxon.Hej.expr.elab.SequenceEvaluator selectEval, Saxon.Hej.expr.elab.SequenceEvaluator actionEval)  {
                this.slot = slot;
                this.selectEval = selectEval;
                this.actionEval = actionEval;
            }
            public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
                int savedOutputState = context.getTemporaryOutputState();
                context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                Saxon.Hej.om.Sequence value = selectEval.evaluate(context);
                context.setLocalVariable(slot, value);
                context.setTemporaryOutputState(savedOutputState);
                return actionEval.evaluate(context);
            }
        
        }
    
    }
}
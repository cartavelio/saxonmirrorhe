using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class VariableReference : Saxon.Hej.expr.Expression, Saxon.Hej.expr.BindingReference {
        protected internal Saxon.Hej.expr.Binding binding = null;
        protected internal Saxon.Hej.value.SequenceType staticType = null;
        protected internal Saxon.Hej.om.GroundedValue constantValue = null;
        private Saxon.Hej.om.StructuredQName variableName = null;
        private bool flattened = false;
        private bool inLoop = false;
        private bool filtered = false;
        public VariableReference(Saxon.Hej.om.StructuredQName name)  {
            variableName = name;
        }
        public VariableReference(Saxon.Hej.expr.Binding binding)  {
            variableName = binding.getVariableQName();
            fixup(binding);
        }
        public void setVariableName(Saxon.Hej.om.StructuredQName name) {
            variableName = name;
        }
        public Saxon.Hej.om.StructuredQName getVariableName() {
            return variableName;
        }
        public abstract override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings);
        public override int getNetCost() {
            return 0;
        }
        protected internal void copyFrom(Saxon.Hej.expr.VariableReference @ref) {
            binding = @ref.binding;
            staticType = @ref.staticType;
            constantValue = @ref.constantValue;
            variableName = @ref.variableName;
            flattened = @ref.flattened;
            inLoop = @ref.inLoop;
            filtered = @ref.filtered;
            /*134*/Saxon.Eej.ee.stream.Streamability.setPostureAndSweep(this, /*134*/Saxon.Eej.ee.stream.Streamability.getPostureAndSweepIfKnown(@ref));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(@ref, this);
        }
        public void setStaticType(Saxon.Hej.value.SequenceType type, Saxon.Hej.om.GroundedValue value, int properties) {
            if ((type == null)) {
                type = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
            staticType = type;
            constantValue = value;
            int dependencies = getDependencies();
            staticProperties = ((((((properties&~(Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET))&~(Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED)))|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|type.getCardinality())|dependencies);
        }
        public override void setFlattened(bool flattened) {
            this.flattened = flattened;
        }
        public bool isFlattened() {
            return flattened;
        }
        public override void setFiltered(bool filtered) {
            this.filtered = filtered;
        }
        public bool isFiltered() {
            return filtered;
        }
        public bool isInLoop() {
            return inLoop;
        }
        public void setInLoop(bool inLoop) {
            this.inLoop = inLoop;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((constantValue != null)) {
                binding = null;
                return Saxon.Hej.expr.Literal.makeLiteral(constantValue, this);
            }
            if ((binding != null)) {
                recomputeInLoop();
                binding.addReference(this, inLoop);
            }
            return this;
        }
        public void recomputeInLoop() {
            inLoop = Saxon.Hej.expr.parser.ExpressionTool.isLoopingReference(this, binding);
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((((binding is Saxon.Hej.expr.LetExpression) && (((Saxon.Hej.expr.LetExpression)(binding)).getSequence() is Saxon.Hej.expr.Literal)) && !(((Saxon.Hej.expr.LetExpression)(binding)).indexedVariable))) {
                Saxon.Hej.expr.Expression val = ((Saxon.Hej.expr.LetExpression)(binding)).getSequence();
                Saxon.Hej.expr.parser.Optimizer.trace(visitor.getConfiguration(), (("Replaced variable "+getDisplayName())+" by its value"), val);
                binding = null;
                return val.copy(new Saxon.Hej.expr.parser.RebindingMap());
            }
            if ((constantValue != null)) {
                binding = null;
                Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeLiteral(constantValue, this);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, result);
                Saxon.Hej.expr.parser.Optimizer.trace(visitor.getConfiguration(), (("Replaced variable "+getDisplayName())+" by its value"), result);
                return result;
            }
            if (((binding is Saxon.Hej.expr.instruct.GlobalParam) && ((Saxon.Hej.expr.instruct.GlobalParam)(binding)).isStatic())) {
                Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.instruct.GlobalParam)(binding)).getBody();
                if ((select is Saxon.Hej.expr.Literal)) {
                    binding = null;
                    Saxon.Hej.expr.parser.Optimizer.trace(visitor.getConfiguration(), (("Replaced static parameter "+getDisplayName())+" by its value"), select);
                    return select.copy(new Saxon.Hej.expr.parser.RebindingMap());
                }
            }
            return this;
        }
        public void fixup(Saxon.Hej.expr.Binding newBinding) {
            bool indexed = ((binding is Saxon.Hej.expr.LocalBinding) && ((Saxon.Hej.expr.LocalBinding)(binding)).isIndexedVariable());
            this.binding = newBinding;
            if ((indexed && (newBinding is Saxon.Hej.expr.LocalBinding))) {
                ((Saxon.Hej.expr.LocalBinding)(newBinding)).setIndexedVariable();
            }
            resetLocalStaticProperties();
        }
        public void refineVariableType(Saxon.Hej.type.ItemType type, int cardinality, Saxon.Hej.om.GroundedValue constantValue, int properties) {
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType oldItemType = getItemType();
            Saxon.Hej.type.ItemType newItemType = oldItemType;
            if (th.isSubType(type, oldItemType)) {
                newItemType = type;
            }
            if (((oldItemType is Saxon.Hej.pattern.NodeTest) && (type is Saxon.Hej.type.AtomicType))) {
                newItemType = type;
            }
            int newcard = (cardinality&getCardinality());
            if ((newcard == 0)) {
                newcard = getCardinality();
            }
            Saxon.Hej.value.SequenceType seqType = Saxon.Hej.value.SequenceType.makeSequenceType(newItemType, newcard);
            setStaticType(seqType, constantValue, properties);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if (((staticType == null) || (staticType.getPrimaryType() == Saxon.Hej.type.AnyItemType.getInstance()))) {
                if ((binding != null)) {
                    Saxon.Hej.value.SequenceType st = binding.getRequiredType();
                    if ((st != null)) {
                        return st.getPrimaryType();
                    }
                }
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                return staticType.getPrimaryType();
            }
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            if ((binding != null)) {
                if ((((binding.isGlobal() || (binding is Saxon.Hej.expr.instruct.LocalParam)) || (((binding is Saxon.Hej.expr.LetExpression) && ((Saxon.Hej.expr.LetExpression)(binding)).isInstruction()))) || (binding is Saxon.Hej.expr.flwor.LocalVariableBinding))) {
                    Saxon.Hej.value.SequenceType st = binding.getRequiredType();
                    if ((st != null)) {
                        return st.getPrimaryType().getUType();
                    } else {
                        return Saxon.Hej.type.UType.ANY;
                    }
                } else {
                    if ((binding is Saxon.Hej.expr.Assignation)) {
                        return ((Saxon.Hej.expr.Assignation)(binding)).getSequence().getStaticUType(contextItemType);
                    }
                }
            }
            return Saxon.Hej.type.UType.ANY;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            if ((binding != null)) {
                return binding.getIntegerBoundsForVariable();
            } else {
                return null;
            }
        }
        protected internal override int computeCardinality() {
            if ((staticType == null)) {
                if ((binding == null)) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                } else {
                    if ((binding is Saxon.Hej.expr.LetExpression)) {
                        return binding.getRequiredType().getCardinality();
                    } else {
                        if ((binding is Saxon.Hej.expr.Assignation)) {
                            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                        } else {
                            if ((binding.getRequiredType() == null)) {
                                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                            } else {
                                return binding.getRequiredType().getCardinality();
                            }
                        }
                    }
                }
            } else {
                return staticType.getCardinality();
            }
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            if (((binding == null) || !(binding.isAssignable()))) {
                p |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
            }
            if ((binding is Saxon.Hej.expr.Assignation)) {
                Saxon.Hej.expr.Expression exp = ((Saxon.Hej.expr.Assignation)(binding)).getSequence();
                if ((exp != null)) {
                    p |= (exp.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.NOT_UNTYPED_ATOMIC);
                }
            }
            if ((((staticType != null) && !(Saxon.Hej.value.Cardinality.allowsMany(staticType.getCardinality()))) && (staticType.getPrimaryType() is Saxon.Hej.pattern.NodeTest))) {
                p |= Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET;
            }
            return (p&~(Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED));
        }
        public override bool supportsLazyEvaluation() {
            return false;
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.expr.VariableReference) && (binding == ((Saxon.Hej.expr.VariableReference)(other)).binding)) && (binding != null));
        }
        protected internal override int computeHashCode() {
            return ((binding == null) ? 73619830 : binding.GetHashCode());
        }
        public override int getIntrinsicDependencies() {
            int d = 0;
            if ((binding == null)) {
                d |= ((Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_ASSIGNABLE_GLOBALS)|Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT);
            } else {
                if (binding.isGlobal()) {
                    if (binding.isAssignable()) {
                        d |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_ASSIGNABLE_GLOBALS;
                    }
                    if ((binding is Saxon.Hej.expr.instruct.GlobalParam)) {
                        d |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT;
                    }
                } else {
                    d |= Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES;
                }
            }
            return d;
        }
        public override int getImplementationMethod() {
            return ((((Saxon.Hej.value.Cardinality.allowsMany(getCardinality()) ? 0 : EVALUATE_METHOD))|ITERATE_METHOD)|PROCESS_METHOD);
        }
        public override Saxon.Hej.expr.Expression getScopingExpression() {
            if ((binding is Saxon.Hej.expr.Expression)) {
                if (((binding is Saxon.Hej.expr.instruct.LocalParam) && (((Saxon.Hej.expr.instruct.LocalParam)(binding)).getParentExpression() is Saxon.Hej.expr.instruct.LocalParamBlock))) {
                    Saxon.Hej.expr.instruct.LocalParamBlock block = (Saxon.Hej.expr.instruct.LocalParamBlock)(((Saxon.Hej.expr.instruct.LocalParam)(binding)).getParentExpression());
                    return block.getParentExpression();
                } else {
                    return (Saxon.Hej.expr.Expression)(binding);
                }
            }
            Saxon.Hej.expr.Expression parent = getParentExpression();
            while ((parent != null)) {
                if (parent.hasVariableBinding(binding)) {
                    return parent;
                }
                parent = parent.getParentExpression();
            }
            return null;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            return pathMap.getPathForVariable(getBinding());
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            try {
                Saxon.Hej.om.Sequence actual = evaluateVariable(c);
                System.Diagnostics.Debug.Assert((actual != null));
                return actual.iterate();
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            } catch (Saxon.Ejava.lang.NullPointerException err) {
                string msg = (((("Internal error: no value for variable $"+getDisplayName())+" at line ")+getLocation().getLineNumber())+(((getLocation().getSystemId() == null) ? "" : (" of "+getLocation().getSystemId()))));
                new Saxon.Hej.lib.StandardDiagnostics().logStackTrace(c, c.getConfiguration().getLogger(), 2);
                throw new Saxon.Ejava.lang.AssertionError(msg);
            } catch (Saxon.Ejava.lang.AssertionError err) {
                string msg = (((((err.Message+". Variable reference $")+getDisplayName())+" at line ")+getLocation().getLineNumber())+(((getLocation().getSystemId() == null) ? "" : (" of "+getLocation().getSystemId()))));
                new Saxon.Hej.lib.StandardDiagnostics().logStackTrace(c, c.getConfiguration().getLogger(), 2);
                throw new Saxon.Ejava.lang.AssertionError(msg);
            }
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext c) {
            try {
                Saxon.Hej.om.Sequence actual = evaluateVariable(c);
                System.Diagnostics.Debug.Assert((actual != null));
                return actual.head();
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext c) {
            try {
                Saxon.Hej.om.SequenceIterator iter = evaluateVariable(c).iterate();
                Saxon.Hej.s9api.Location loc = getLocation();
                Saxon.Hej.om.SequenceTool.supply(iter, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => output.append(item, loc, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES))));
            } catch (Saxon.Hej.trans.UncheckedXPathException uxe) {
                Saxon.Hej.trans.XPathException xpe = uxe.getXPathException();
                xpe.maybeSetLocation(getLocation());
                throw xpe;
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(getLocation());
                throw;
            }
        }
        public virtual Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext c) {
            try {
                return binding.evaluateVariable(c);
            } catch (Saxon.Ejava.lang.NullPointerException err) {
                if ((binding == null)) {
                    throw new Saxon.Ejava.lang.IllegalStateException((("Variable $"+variableName.getDisplayName())+" has not been fixed up"));
                } else {
                    throw;
                }
            }
        }
        public virtual Saxon.Hej.expr.Binding getBinding() {
            return binding;
        }
        public string getDisplayName() {
            if ((binding != null)) {
                return binding.getVariableQName().getDisplayName();
            } else {
                return variableName.getDisplayName();
            }
        }
        public string getEQName() {
            if ((binding != null)) {
                Saxon.Hej.om.StructuredQName q = binding.getVariableQName();
                if (q.hasURI(Saxon.Hej.om.NamespaceUri.NULL)) {
                    return q.getLocalPart();
                } else {
                    return q.getEQName();
                }
            } else {
                return variableName.getEQName();
            }
        }
        public override string ToString() {
            string d = getEQName();
            return ("$"+(((d == null) ? "$" : d)));
        }
        public override string toShortString() {
            return ("$"+getDisplayName());
        }
        public override string getExpressionName() {
            return "varRef";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("varRef", this);
            destination.emitAttribute("name", variableName);
            if ((binding is Saxon.Hej.expr.LocalBinding)) {
                destination.emitAttribute("slot", (""+((Saxon.Hej.expr.LocalBinding)(binding)).getLocalSlotNumber()));
            }
            destination.endElement();
        }
        public override string getStreamerName() {
            return "VariableReference";
        }
    
    }
}
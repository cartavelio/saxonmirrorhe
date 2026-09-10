using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public abstract class Expression : Saxon.Hej.om.IdentityComparable, Saxon.Hej.expr.ExportAgent, Saxon.Hej.expr.Locatable, Saxon.Hej.trace.Traceable {
        public const int EVALUATE_METHOD = 1;
        public const int ITERATE_METHOD = 2;
        public const int PROCESS_METHOD = 4;
        public const int WATCH_METHOD = 8;
        public const int ITEM_FEED_METHOD = 16;
        public const int EFFECTIVE_BOOLEAN_VALUE = 32;
        public const int UPDATE_METHOD = 64;
        protected internal int staticProperties = -1;
        private Saxon.Hej.s9api.Location location = Saxon.Hej.expr.parser.Loc.NONE;
        private Saxon.Hej.expr.Expression parentExpression;
        private Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext;
        private int[] slotsUsed;
        private int evaluationMethod;
        private System.Collections.Generic.IDictionary<string,System.Object> extraProperties;
        private double cost = -1;
        private int cachedHashCode = -1;
        public Expression()  {
        }
        public virtual string getExpressionName() {
            return this.GetType().Name;
        }
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
        }
        public Saxon.Hej.expr.Expression getInterpretedExpression() {
            return this;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> checkedOperands() {
            System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> ops = operands();
            foreach (Saxon.Hej.expr.Operand o in ops) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                bool badOperand = (o.getParentExpression() != this);
                bool badExpression = (child.getParentExpression() != this);
                if ((badOperand || badExpression)) {
                    string message = ((((((("*** Bad parent pointer found in "+((badOperand ? "operand " : "expression ")))+child.toShortString())+" at ")+child.getLocation().getSystemId())+"#")+child.getLocation().getLineNumber())+" ***");
                    try {
                        Saxon.Hej.Configuration config = getConfiguration();
                        Saxon.Hej.lib.Logger logger = ((config == null) ? null : config.getLogger());
                        if ((logger != null)) {
                            logger.warning(message);
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException(message);
                        }
                    } catch (System.Exception err) {
                        throw new Saxon.Ejava.lang.IllegalStateException(message);
                    }
                    child.setParentExpression(this);
                }
                if ((child.getRetainedStaticContext() == null)) {
                    child.setRetainedStaticContext(getRetainedStaticContext());
                }
            }
            return ops;
        }
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operandList(params Saxon.Hej.expr.Operand[]  a) {
            return Saxon.Ejava.util.Arrays.asList(a);
        }
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operandSparseList(params Saxon.Hej.expr.Operand[]  a) {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operanda = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>();
            foreach (Saxon.Hej.expr.Operand o in a) {
                if ((o != null)) {
                    operanda.Add(o);
                }
            }
            return operanda;
        }
        public Saxon.Hej.expr.Expression getParentExpression() {
            return parentExpression;
        }
        public void setParentExpression(Saxon.Hej.expr.Expression parent) {
            parentExpression = parent;
        }
        public Saxon.Hej.expr.Expression verifyParentPointers() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression parent = o.getChildExpression().getParentExpression();
                if ((parent != this)) {
                    throw new Saxon.Ejava.lang.IllegalStateException(((("Invalid parent pointer in "+parent.toShortString())+" subexpression ")+o.getChildExpression().toShortString()));
                }
                if ((o.getParentExpression() != this)) {
                    throw new Saxon.Ejava.lang.IllegalStateException(((("Invalid parent pointer in operand object "+parent.toShortString())+" subexpression ")+o.getChildExpression().toShortString()));
                }
                if ((Saxon.Hej.expr.parser.ExpressionTool.findOperand(parent, o.getChildExpression()) == null)) {
                    throw new Saxon.Ejava.lang.IllegalStateException(((("Incorrect parent pointer in "+parent.toShortString())+" subexpression ")+o.getChildExpression().toShortString()));
                }
                o.getChildExpression().verifyParentPointers();
            }
            return this;
        }
        public void restoreParentPointers() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                child.setParentExpression(this);
                child.restoreParentPointers();
            }
        }
        public abstract int getImplementationMethod();
        public virtual bool implementsStaticTypeCheck() {
            return false;
        }
        public virtual bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            return false;
        }
        public virtual bool isLiftable(bool forStreaming) {
            int p = getSpecialProperties();
            int d = getDependencies();
            return (((((((p&Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)) != 0) && (((p&Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS)) == 0)) && ((((d&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_ASSIGNABLE_GLOBALS)) == 0))) && ((((d&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_POSITION)) == 0))) && ((((d&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LAST)) == 0)));
        }
        public virtual Saxon.Hej.expr.Expression getScopingExpression() {
            int d = (getIntrinsicDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS);
            if ((d != 0)) {
                if ((d == Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT)) {
                    return Saxon.Hej.expr.parser.ExpressionTool.getContextDocumentSettingContainer(this);
                } else {
                    return Saxon.Hej.expr.parser.ExpressionTool.getFocusSettingContainer(this);
                }
            } else {
                return null;
            }
        }
        public virtual bool isMultiThreaded(Saxon.Hej.Configuration config) {
            return false;
        }
        public virtual bool allowExtractingCommonSubexpressions() {
            return true;
        }
        public virtual Saxon.Hej.expr.Expression simplify() {
            simplifyChildren();
            return this;
        }
        protected internal void simplifyChildren() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if ((o != null)) {
                    Saxon.Hej.expr.Expression e = o.getChildExpression();
                    if ((e != null)) {
                        Saxon.Hej.expr.Expression f = e.simplify();
                        o.setChildExpression(f);
                    }
                }
            }
        }
        public virtual void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            if ((rsc != null)) {
                retainedStaticContext = rsc;
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    if ((o != null)) {
                        Saxon.Hej.expr.Expression child = o.getChildExpression();
                        if (((child != null) && (child.retainedStaticContext == null))) {
                            child.setRetainedStaticContext(rsc);
                        }
                    }
                }
            }
        }
        public void setRetainedStaticContextThoroughly(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            if ((rsc != null)) {
                retainedStaticContext = rsc;
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    if ((o != null)) {
                        Saxon.Hej.expr.Expression child = o.getChildExpression();
                        if ((child != null)) {
                            if ((child.getLocalRetainedStaticContext() == null)) {
                                child.setRetainedStaticContextThoroughly(rsc);
                            } else {
                                rsc = child.getLocalRetainedStaticContext();
                                foreach (Saxon.Hej.expr.Operand p in child.operands()) {
                                    Saxon.Hej.expr.Expression grandchild = p.getChildExpression();
                                    if ((grandchild != null)) {
                                        grandchild.setRetainedStaticContextThoroughly(rsc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void setRetainedStaticContextLocally(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            if ((rsc != null)) {
                retainedStaticContext = rsc;
            }
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext getRetainedStaticContext() {
            if ((retainedStaticContext == null)) {
                Saxon.Hej.expr.Expression parent = getParentExpression();
                System.Diagnostics.Debug.Assert((parent != null));
                retainedStaticContext = parent.getRetainedStaticContext();
                System.Diagnostics.Debug.Assert((retainedStaticContext != null));
            }
            return retainedStaticContext;
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext getLocalRetainedStaticContext() {
            return retainedStaticContext;
        }
        public string getStaticBaseURIString() {
            return getRetainedStaticContext().getStaticBaseUriString();
        }
        public System.Uri getStaticBaseURI() {
            return getRetainedStaticContext().getStaticBaseUri();
        }
        public virtual bool isCallOn(System.Type function) {
            return false;
        }
        public virtual Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            return this;
        }
        protected internal void typeCheckChildren(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.typeCheck(visitor, contextInfo);
            }
        }
        public virtual Saxon.Hej.expr.Expression staticTypeCheck(Saxon.Hej.value.SequenceType req, bool backwardsCompatible, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("staticTypeCheck");
        }
        public virtual Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if (visitor.incrementAndTestDepth()) {
                optimizeChildren(visitor, contextInfo);
                visitor.decrementDepth();
            }
            return this;
        }
        protected internal void optimizeChildren(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.optimize(visitor, contextInfo);
            }
        }
        public virtual void prepareForStreaming() {
        }
        public virtual double getCost() {
            if ((cost<0)) {
                double i = getNetCost();
                foreach (Saxon.Hej.expr.Operand o in operands()) {
                    i += o.getChildExpression().getCost();
                    if ((i>MAX_COST)) {
                        break ;
                    }
                }
                cost = i;
            }
            return cost;
        }
        public const double MAX_COST = 1e9;
        public virtual int getNetCost() {
            return 1;
        }
        public virtual Saxon.Hej.expr.Expression unordered(bool retainAllNodes, bool forStreaming) {
            return this;
        }
        public int getSpecialProperties() {
            if ((staticProperties == -1)) {
                computeStaticProperties();
            }
            return (staticProperties&Saxon.Hej.expr.StaticProperty.SPECIAL_PROPERTY_MASK);
        }
        public bool hasSpecialProperty(int property) {
            return (((getSpecialProperties()&property)) != 0);
        }
        public virtual int getCardinality() {
            if ((staticProperties == -1)) {
                computeStaticProperties();
            }
            return (staticProperties&Saxon.Hej.expr.StaticProperty.CARDINALITY_MASK);
        }
        public abstract Saxon.Hej.type.ItemType getItemType();
        public Saxon.Hej.value.SequenceType getStaticType() {
            return Saxon.Hej.value.SequenceType.makeSequenceType(getItemType(), getCardinality());
        }
        public virtual Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.ANY;
        }
        public virtual int getDependencies() {
            if ((staticProperties == -1)) {
                computeStaticProperties();
            }
            return (staticProperties&Saxon.Hej.expr.StaticProperty.DEPENDENCY_MASK);
        }
        public virtual Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return null;
        }
        public static readonly Saxon.Hej.value.IntegerValue UNBOUNDED_LOWER = (Saxon.Hej.value.IntegerValue)(Saxon.Hej.value.IntegerValue.fromDouble(-(1e100)));
        public static readonly Saxon.Hej.value.IntegerValue UNBOUNDED_UPPER = (Saxon.Hej.value.IntegerValue)(Saxon.Hej.value.IntegerValue.fromDouble(+(1e100)));
        public static readonly Saxon.Hej.value.IntegerValue MAX_STRING_LENGTH = Saxon.Hej.value.Int64Value.makeIntegerValue(System.Int32.MaxValue);
        public static readonly Saxon.Hej.value.IntegerValue MAX_SEQUENCE_LENGTH = Saxon.Hej.value.Int64Value.makeIntegerValue(System.Int32.MaxValue);
        public virtual void setFlattened(bool flattened) {
        }
        public virtual void setFiltered(bool filtered) {
        }
        public virtual Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return iterate(context).next();
        }
        public virtual Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item value = evaluateItem(context);
            return Saxon.Hej.om.SequenceTool.itemOrEmpty(value).iterate();
        }
        public virtual bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            try {
                return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate(context));
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetFailingExpression(this);
                e.maybeSetContext(context);
                throw;
            }
        }
        public virtual Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item o = evaluateItem(context);
            if ((o == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return o.getUnicodeStringValue();
        }
        public virtual void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            int m = getImplementationMethod();
            bool hasEvaluateMethod = (((m&EVALUATE_METHOD)) != 0);
            bool hasIterateMethod = (((m&ITERATE_METHOD)) != 0);
            try {
                if ((hasEvaluateMethod && ((!(hasIterateMethod) || !(Saxon.Hej.value.Cardinality.allowsMany(getCardinality())))))) {
                    Saxon.Hej.om.Item item = evaluateItem(context);
                    if ((item != null)) {
                        output.append(item, getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
                    }
                } else {
                    if (hasIterateMethod) {
                        Saxon.Hej.om.SequenceTool.supply(iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((it => output.append(it, getLocation(), Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES))));
                    } else {
                        throw new Saxon.Ejava.lang.AssertionError(("process() is not implemented in the subclass "+this.GetType()));
                    }
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException unxe) {
                Saxon.Hej.trans.XPathException e = unxe.getXPathException();
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw e;
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        public virtual void evaluatePendingUpdates(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.PendingUpdateList pul) {
            if (isVacuousExpression()) {
                iterate(context).next();
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException((("Expression "+this.GetType())+" is not an updating expression"));
            }
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
            string className = this.GetType().Name;
            while (true) {
                int dot = className.IndexOf('.');
                if ((dot>=0)) {
                    className = className.Substring((dot+1));
                } else {
                    break ;
                }
            }
            buff.append(className);
            bool first = true;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                buff.append((first ? "(" : ", "));
                buff.append(o.getChildExpression().ToString());
                first = false;
            }
            if (!(first)) {
                buff.append(")");
            }
            return buff.ToString();
        }
        public virtual string toShortString() {
            return getExpressionName();
        }
        public abstract void export(Saxon.Hej.trace.ExpressionPresenter @out);
        public void explain(Saxon.Hej.lib.Logger @out) {
            Saxon.Hej.trace.ExpressionPresenter ep = new Saxon.Hej.trace.ExpressionPresenter(getConfiguration(), @out);
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = new Saxon.Hej.trace.ExpressionPresenter.ExportOptions();
            options.explaining = true;
            ep.setOptions(options);
            try {
                export(ep);
            } catch (Saxon.Hej.trans.XPathException e) {
                ep.startElement("failure");
                ep.emitAttribute("message", e.Message);
                ep.endElement();
            }
            ep.close();
        }
        public virtual void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, bool whole) {
        }
        public void adoptChildExpression(Saxon.Hej.expr.Expression child) {
            if ((child == null)) {
                return ;
            }
            child.setParentExpression(this);
            if ((child.retainedStaticContext == null)) {
                child.retainedStaticContext = retainedStaticContext;
            }
            if (((getLocation() == null) || (getLocation() == Saxon.Hej.expr.parser.Loc.NONE))) {
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(child, this);
            } else {
                if (((child.getLocation() == null) || (child.getLocation() == Saxon.Hej.expr.parser.Loc.NONE))) {
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, child);
                }
            }
            resetLocalStaticProperties();
        }
        public virtual void setLocation(Saxon.Hej.s9api.Location id) {
            location = id;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            int limit = 0;
            Saxon.Hej.expr.Expression exp = this;
            while ((limit<10)) {
                if (((((exp.location == null) || (exp.location == Saxon.Hej.expr.parser.Loc.NONE))) && (exp.getParentExpression() != null))) {
                    exp = exp.getParentExpression();
                    limit++;
                } else {
                    return exp.location;
                }
            }
            return exp.location;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            try {
                return getRetainedStaticContext().getConfiguration();
            } catch (Saxon.Ejava.lang.NullPointerException e) {
                throw new Saxon.Ejava.lang.NullPointerException((("Internal error: expression "+toShortString())+" has no retained static context"));
            }
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            try {
                return getRetainedStaticContext().getPackageData();
            } catch (Saxon.Ejava.lang.NullPointerException e) {
                throw new Saxon.Ejava.lang.NullPointerException((("Internal error: expression "+toShortString())+" has no retained static context"));
            }
        }
        public virtual bool isInstruction() {
            return false;
        }
        public void computeStaticProperties() {
            staticProperties = ((computeDependencies()|computeCardinality())|computeSpecialProperties());
        }
        public virtual void resetLocalStaticProperties() {
            staticProperties = -1;
            cachedHashCode = -1;
        }
        public bool isStaticPropertiesKnown() {
            return (staticProperties != -1);
        }
        protected internal abstract int computeCardinality();
        protected internal virtual int computeSpecialProperties() {
            return 0;
        }
        public virtual int computeDependencies() {
            int dependencies = getIntrinsicDependencies();
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if (o.hasSameFocus()) {
                    dependencies |= o.getChildExpression().getDependencies();
                } else {
                    dependencies |= (o.getChildExpression().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS));
                }
            }
            return dependencies;
        }
        public virtual int getIntrinsicDependencies() {
            return 0;
        }
        public void setStaticProperty(int prop) {
            if ((staticProperties == -1)) {
                computeStaticProperties();
            }
            staticProperties |= prop;
        }
        public virtual void checkForUpdatingSubexpressions() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.Expression sub = o.getChildExpression();
                if ((sub == null)) {
                    throw new Saxon.Ejava.lang.NullPointerException();
                }
                sub.checkForUpdatingSubexpressions();
                if (sub.isUpdatingExpression()) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Updating expression appears in a context where it is not permitted", "XUST0001");
                    err.setLocation(sub.getLocation());
                    throw err;
                }
            }
        }
        public virtual bool isUpdatingExpression() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if (o.getChildExpression().isUpdatingExpression()) {
                    return true;
                }
            }
            return false;
        }
        public virtual bool isVacuousExpression() {
            return false;
        }
        public abstract Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings);
        public virtual void suppressValidation(int parentValidationMode) {
        }
        public virtual int markTailFunctionCalls(Saxon.Hej.om.StructuredQName qName, int arity) {
            return Saxon.Hej.expr.UserFunctionCall.NOT_TAIL_CALL;
        }
        public virtual Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            Saxon.Hej.type.ItemType type = getItemType();
            if ((((((getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_NON_DOCUMENT_FOCUS)) == 0)) && (((type is Saxon.Hej.pattern.NodeTest) || (this is Saxon.Hej.expr.VariableReference))))) {
                return new Saxon.Hej.pattern.NodeSetPattern(this);
            }
            if ((isCallOn(typeof(Saxon.Hej.functions.KeyFn)) || isCallOn(typeof(Saxon.Hej.functions.SuperId)))) {
                return new Saxon.Hej.pattern.NodeSetPattern(this);
            }
            throw new Saxon.Hej.trans.XPathException((("Cannot convert the expression {"+this)+"} to a pattern"));
        }
        public int[] getSlotsUsed() { lock(this) {
            if ((slotsUsed != null)) {
                return slotsUsed;
            }
            Saxon.Hej.z.IntHashSet slots = new Saxon.Hej.z.IntHashSet(10);
            gatherSlotsUsed(this, slots);
            slotsUsed = new int[slots.size()];
            int i = 0;
            Saxon.Hej.z.IntIterator iter = slots.iterator();
            while (iter.hasNext()) {
                slotsUsed[i++] = iter.next();
            }
            Saxon.Ejava.util.Arrays.sort(slotsUsed);
            return slotsUsed;
        }}
        private static void gatherSlotsUsed(Saxon.Hej.expr.Expression exp, Saxon.Hej.z.IntHashSet slots) {
            exp = exp.getInterpretedExpression();
            if ((exp is Saxon.Hej.expr.LocalVariableReference)) {
                slots.add(((Saxon.Hej.expr.LocalVariableReference)(exp)).getSlotNumber());
            } else {
                if ((exp is Saxon.Hej.expr.SuppliedParameterReference)) {
                    int slot = ((Saxon.Hej.expr.SuppliedParameterReference)(exp)).getSlotNumber();
                    slots.add(slot);
                } else {
                    foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                        gatherSlotsUsed(o.getChildExpression(), slots);
                    }
                }
            }
        }
        protected internal void dynamicError(string message, string code, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message, code, getLocation());
            err.setXPathContext(context);
            err.setFailingExpression(this);
            throw err;
        }
        protected internal void typeError(string message, string errorCode, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message, errorCode, getLocation());
            e.setIsTypeError(true);
            e.setXPathContext(context);
            e.setFailingExpression(this);
            throw e;
        }
        public string getTracingTag() {
            return getExpressionName();
        }
        public virtual Saxon.Hej.om.StructuredQName getObjectName() {
            return null;
        }
        public virtual System.Object getProperty(string name) {
            if (name.Equals("expression")) {
                return getLocation();
            } else {
                return null;
            }
        }
        public virtual System.Collections.Generic.IEnumerator<string> getProperties() {
            return new Saxon.Hej.tree.jiter.MonoIterator<string>("expression");
        }
        public virtual Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            bool dependsOnFocus = Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(this);
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet attachmentPoint;
            if ((pathMapNodeSet == null)) {
                if (dependsOnFocus) {
                    Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cie);
                    pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(cie));
                }
                attachmentPoint = pathMapNodeSet;
            } else {
                attachmentPoint = (dependsOnFocus ? pathMapNodeSet : null);
            }
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.OperandUsage usage = o.getUsage();
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet target = child.addToPathMap(pathMap, attachmentPoint);
                if ((usage == Saxon.Hej.expr.OperandUsage.NAVIGATION)) {
                    target = target.createArc(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    target = target.createArc(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                }
                result.addNodeSet(target);
            }
            if ((getItemType() is Saxon.Hej.type.AtomicType)) {
                return null;
            } else {
                return result;
            }
        }
        public virtual bool isSubtreeExpression() {
            if (Saxon.Hej.expr.parser.ExpressionTool.dependsOnFocus(this)) {
                if ((((getIntrinsicDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS)) != 0)) {
                    return false;
                } else {
                    foreach (Saxon.Hej.expr.Operand o in operands()) {
                        if (!(o.getChildExpression().isSubtreeExpression())) {
                            return false;
                        }
                    }
                    return true;
                }
            } else {
                return true;
            }
        }
        public void setEvaluationMethod(int method) {
            this.evaluationMethod = method;
        }
        public virtual int getEvaluationMethod() {
            return evaluationMethod;
        }
        public override bool Equals(System.Object obj) {
            return base.Equals(obj);
        }
        public bool isEqual(Saxon.Hej.expr.Expression other) {
            return ((this == other) || (((GetHashCode() == other.GetHashCode()) && Equals(other))));
        }
        public override int GetHashCode() {
            if ((cachedHashCode == -1)) {
                cachedHashCode = computeHashCode();
            }
            return cachedHashCode;
        }
        protected internal bool hasCompatibleStaticContext(Saxon.Hej.expr.Expression other) {
            bool d1 = (((getIntrinsicDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT)) != 0);
            bool d2 = (((other.getIntrinsicDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_STATIC_CONTEXT)) != 0);
            if ((d1 != d2)) {
                return false;
            }
            if (d1) {
                return getRetainedStaticContext().Equals(other.getRetainedStaticContext());
            }
            return true;
        }
        protected internal virtual int computeHashCode() {
            return base.GetHashCode();
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (this == other);
        }
        public int identityHashCode() {
            return Saxon.Ejava.lang.System.identityHashCode(getLocation());
        }
        public void setExtraProperty(string name, System.Object value) {
            if ((extraProperties == null)) {
                if ((value == null)) {
                    return ;
                }
                extraProperties = new System.Collections.Generic.Dictionary<string, System.Object>(4);
            }
            if ((value == null)) {
                extraProperties.Remove(name);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(extraProperties, name, value);
            }
        }
        public System.Object getExtraProperty(string name) {
            if ((extraProperties == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(extraProperties, name);
            }
        }
        public virtual string getStreamerName() {
            return null;
        }
    
        /* Expanding default method gatherProperties from interface Traceable */
        public virtual void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
        }

    }
}
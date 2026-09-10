using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class GlobalVariable : Saxon.Hej.expr.instruct.Actor, Saxon.Hej.expr.Binding, Saxon.Hej.query.Declaration, Saxon.Hej.trace.TraceableComponent, Saxon.Hej.expr.ContextOriginator {
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.BindingReference> references = new System.Collections.Generic.List<Saxon.Hej.expr.BindingReference>(10);
        private Saxon.Hej.om.StructuredQName variableQName;
        private Saxon.Hej.value.SequenceType requiredType;
        private bool _indexed;
        private bool _isPrivate = false;
        private bool _isAssignable = false;
        private Saxon.Hej.expr.instruct.GlobalVariable originalVariable;
        private int binderySlotNumber;
        private bool _isRequiredParam;
        private bool _isStatic;
        public GlobalVariable()  {
        }
        public void init(Saxon.Hej.expr.Expression select, Saxon.Hej.om.StructuredQName qName) {
            variableQName = qName;
            setBody(select);
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, variableQName);
        }
        public override string getTracingTag() {
            return "xsl:variable";
        }
        public void gatherProperties(Saxon.Ejava.util.function.BiConsumer<string,System.Object> consumer) {
            consumer("name", getVariableQName());
        }
        public void setStatic(bool declaredStatic) {
            _isStatic = declaredStatic;
        }
        public bool isStatic() {
            return this._isStatic;
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType required) {
            requiredType = required;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        private Saxon.Hej.Configuration getConfiguration() {
            return getPackageData().getConfiguration();
        }
        public void setOriginalVariable(Saxon.Hej.expr.instruct.GlobalVariable var) {
            originalVariable = var;
        }
        public Saxon.Hej.expr.instruct.GlobalVariable getOriginalVariable() {
            return originalVariable;
        }
        public Saxon.Hej.expr.instruct.GlobalVariable getUltimateOriginalVariable() {
            if ((originalVariable == null)) {
                return this;
            } else {
                return originalVariable.getUltimateOriginalVariable();
            }
        }
        public void setUnused(bool unused) {
            this.binderySlotNumber = -9234;
        }
        public bool isUnused() {
            return (this.binderySlotNumber == -9234);
        }
        public bool isPrivate() {
            return _isPrivate;
        }
        public void setPrivate(bool b) {
            _isPrivate = b;
        }
        public void setAssignable(bool assignable) {
            _isAssignable = assignable;
        }
        public bool isAssignable() {
            return _isAssignable;
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return getVariableQName();
        }
        public override System.Object getProperty(string name) {
            return null;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return getPackageData().getHostLanguage();
        }
        public void setIndexedVariable() {
            _indexed = true;
        }
        public bool isIndexedVariable() {
            return _indexed;
        }
        public void setContainsLocals(Saxon.Hej.expr.instruct.SlotManager map) {
            setStackFrameMap(map);
        }
        public bool isGlobal() {
            return true;
        }
        public void registerReference(Saxon.Hej.expr.BindingReference @ref) {
            references.Add(@ref);
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.expr.BindingReference> iterateReferences() {
            return references.GetEnumerator();
        }
        public int getBinderySlotNumber() {
            return binderySlotNumber;
        }
        public void setBinderySlotNumber(int s) {
            if (!(isUnused())) {
                binderySlotNumber = s;
            }
        }
        public void setRequiredParam(bool requiredParam) {
            this._isRequiredParam = requiredParam;
        }
        public bool isRequiredParam() {
            return this._isRequiredParam;
        }
        public virtual void compile(Saxon.Hej.expr.instruct.Executable exec, int slot) {
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            setBinderySlotNumber(slot);
            if ((this is Saxon.Hej.expr.instruct.GlobalParam)) {
                setRequiredParam((getBody() == null));
            }
            Saxon.Hej.value.SequenceType type = getRequiredType();
            foreach (Saxon.Hej.expr.BindingReference @ref in references) {
                @ref.fixup(this);
                Saxon.Hej.om.GroundedValue constantValue = null;
                int properties = 0;
                Saxon.Hej.expr.Expression select = getBody();
                if (((select is Saxon.Hej.expr.Literal) && !(((this is Saxon.Hej.expr.instruct.GlobalParam))))) {
                    Saxon.Hej.type.Affinity relation = th.relationship(select.getItemType(), type.getPrimaryType());
                    if (((relation == Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                        constantValue = ((Saxon.Hej.expr.Literal)(select)).getGroundedValue();
                        type = Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.om.SequenceTool.getItemType(constantValue, th), Saxon.Hej.om.SequenceTool.getCardinality(constantValue));
                    }
                }
                if ((select != null)) {
                    properties = select.getSpecialProperties();
                }
                properties |= Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
                @ref.setStaticType(type, constantValue, properties);
            }
            if (isRequiredParam()) {
                exec.registerGlobalParameter((Saxon.Hej.expr.instruct.GlobalParam)(this));
            }
        }
        public void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Hej.expr.Expression value = getBody();
            if ((value != null)) {
                value.checkForUpdatingSubexpressions();
                if (value.isUpdatingExpression()) {
                    throw new Saxon.Hej.trans.XPathException("Initializing expression for global variable must not be an updating expression", "XUST0001");
                }
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, getVariableQName().getDisplayName(), 0);
                Saxon.Hej.expr.parser.ContextItemStaticInfo cit = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true);
                Saxon.Hej.expr.Expression value2 = Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(value.simplify().typeCheck(visitor, cit), getRequiredType(), role, visitor.getStaticContext());
                value2 = value2.optimize(visitor, cit);
                setBody(value2);
                Saxon.Hej.expr.instruct.SlotManager map = getConfiguration().makeSlotManager();
                int slots = Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(value2, 0, map);
                if ((slots>0)) {
                    setContainsLocals(map);
                }
                if (((getRequiredType() == Saxon.Hej.value.SequenceType.ANY_SEQUENCE) && !(((this is Saxon.Hej.expr.instruct.GlobalParam))))) {
                    try {
                        Saxon.Hej.type.ItemType itemType = value.getItemType();
                        int cardinality = value.getCardinality();
                        setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(itemType, cardinality));
                        Saxon.Hej.om.GroundedValue constantValue = null;
                        if ((value2 is Saxon.Hej.expr.Literal)) {
                            constantValue = ((Saxon.Hej.expr.Literal)(value2)).getGroundedValue();
                        }
                        foreach (Saxon.Hej.expr.BindingReference reference in references) {
                            if ((reference is Saxon.Hej.expr.VariableReference)) {
                                ((Saxon.Hej.expr.VariableReference)(reference)).refineVariableType(itemType, cardinality, constantValue, value.getSpecialProperties());
                            }
                        }
                    } catch (System.Exception err) {
                    }
                }
            }
        }
        public void lookForCycles(Saxon.Hej.tree.util.IndexedStack<System.Object> referees, Saxon.Hej.query.XQueryFunctionLibrary globalFunctionLibrary) {
            if (referees.contains(this)) {
                int s = referees.indexOf(this);
                referees.push(this);
                Saxon.Ejava.lang.StringBuilder messageBuilder = new Saxon.Ejava.lang.StringBuilder(("Circular definition of global variable: $"+getVariableQName().getDisplayName()));
                for (int i = s; (i<(referees.size()-1)); i++) {
                    if ((i != s)) {
                        messageBuilder.append(", which");
                    }
                    if ((referees.get((i+1)) is Saxon.Hej.expr.instruct.GlobalVariable)) {
                        Saxon.Hej.expr.instruct.GlobalVariable next = (Saxon.Hej.expr.instruct.GlobalVariable)(referees.get((i+1)));
                        messageBuilder.append(" uses $").append(next.getVariableQName().getDisplayName());
                    } else {
                        if ((referees.get((i+1)) is Saxon.Hej.query.XQueryFunction)) {
                            Saxon.Hej.query.XQueryFunction next = (Saxon.Hej.query.XQueryFunction)(referees.get((i+1)));
                            messageBuilder.append(" calls ").append(next.getFunctionName().getDisplayName()).append("#").append(next.getNumberOfParameters()).append("()");
                        }
                    }
                }
                string message = messageBuilder.ToString();
                message += '.';
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                string errorCode;
                if (getPackageData().isXSLT()) {
                    errorCode = "XTDE0640";
                } else {
                    if (((s == 0) && (referees.size() == 2))) {
                        errorCode = "XPST0008";
                    } else {
                        errorCode = "XQDY0054";
                    }
                }
                err.setErrorCode(errorCode);
                err.setIsStaticError(true);
                err.setLocation(getLocation());
                throw err;
            }
            Saxon.Hej.expr.Expression select = getBody();
            if ((select != null)) {
                referees.push(this);
                System.Collections.Generic.IList<Saxon.Hej.expr.Binding> list = new System.Collections.Generic.List<Saxon.Hej.expr.Binding>(10);
                Saxon.Hej.expr.parser.ExpressionTool.gatherReferencedVariables(select, list);
                foreach (Saxon.Hej.expr.Binding b in list) {
                    if ((b is Saxon.Hej.expr.instruct.GlobalVariable)) {
                        ((Saxon.Hej.expr.instruct.GlobalVariable)(b)).lookForCycles(referees, globalFunctionLibrary);
                    }
                }
                System.Collections.Generic.IList<Saxon.Hej.trans.SymbolicName> flist = new System.Collections.Generic.List<Saxon.Hej.trans.SymbolicName>();
                Saxon.Hej.expr.parser.ExpressionTool.gatherCalledFunctionNames(select, flist);
                foreach (Saxon.Hej.trans.SymbolicName s in flist) {
                    Saxon.Hej.query.XQueryFunction f = globalFunctionLibrary.getDeclarationByKey(s);
                    if (!(referees.contains(f))) {
                        lookForFunctionCycles(f, referees, globalFunctionLibrary);
                    }
                }
                referees.pop();
            }
        }
        private static void lookForFunctionCycles(Saxon.Hej.query.XQueryFunction f, Saxon.Hej.tree.util.IndexedStack<System.Object> referees, Saxon.Hej.query.XQueryFunctionLibrary globalFunctionLibrary) {
            Saxon.Hej.expr.Expression body = f.getBody();
            referees.push(f);
            System.Collections.Generic.IList<Saxon.Hej.expr.Binding> list = new System.Collections.Generic.List<Saxon.Hej.expr.Binding>(10);
            Saxon.Hej.expr.parser.ExpressionTool.gatherReferencedVariables(body, list);
            foreach (Saxon.Hej.expr.Binding b in list) {
                if ((b is Saxon.Hej.expr.instruct.GlobalVariable)) {
                    ((Saxon.Hej.expr.instruct.GlobalVariable)(b)).lookForCycles(referees, globalFunctionLibrary);
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.trans.SymbolicName> flist = new System.Collections.Generic.List<Saxon.Hej.trans.SymbolicName>();
            Saxon.Hej.expr.parser.ExpressionTool.gatherCalledFunctionNames(body, flist);
            foreach (Saxon.Hej.trans.SymbolicName s in flist) {
                Saxon.Hej.query.XQueryFunction qf = globalFunctionLibrary.getDeclarationByKey(s);
                if (!(referees.contains(qf))) {
                    lookForFunctionCycles(qf, referees, globalFunctionLibrary);
                }
            }
            referees.pop();
        }
        public Saxon.Hej.om.GroundedValue getSelectValue(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Component target) {
            Saxon.Hej.expr.Expression select = getBody();
            if ((select == null)) {
                throw new Saxon.Ejava.lang.AssertionError((("*** No select expression for global variable $"+getVariableQName().getDisplayName())+"!!"));
            } else {
                if ((select is Saxon.Hej.expr.Literal)) {
                    return ((Saxon.Hej.expr.Literal)(select)).getGroundedValue();
                } else {
                    try {
                        Saxon.Hej.Controller controller = context.getController();
                        Saxon.Hej.expr.instruct.Executable exec = controller.getExecutable();
                        bool hasAccessToGlobalContext = true;
                        if ((exec is Saxon.Hej.PreparedStylesheet)) {
                            hasAccessToGlobalContext = ((target == null) || (target.getDeclaringPackage() == ((Saxon.Hej.PreparedStylesheet)(exec)).getTopLevelPackage()));
                        }
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
                        c2.setOrigin(this);
                        if (hasAccessToGlobalContext) {
                            Saxon.Hej.tree.iter.ManualIterator mi = new Saxon.Hej.tree.iter.ManualIterator(context.getController().getGlobalContextItem());
                            c2.setCurrentIterator(mi);
                        } else {
                            c2.setCurrentIterator(null);
                        }
                        if ((getStackFrameMap() != null)) {
                            c2.openStackFrame(getStackFrameMap());
                        }
                        c2.setCurrentComponent(target);
                        int savedOutputState = c2.getTemporaryOutputState();
                        c2.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_VARIABLE);
                        c2.setCurrentOutputUri(null);
                        Saxon.Hej.om.GroundedValue result;
                        if (_indexed) {
                            result = c2.getConfiguration().makeSequenceExtent(select, Saxon.Hej.expr.FilterExpression.FILTERED, c2);
                        } else {
                            result = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(select, c2);
                        }
                        c2.setTemporaryOutputState(savedOutputState);
                        return result;
                    } catch (Saxon.Hej.trans.UncheckedXPathException unxe) {
                        Saxon.Hej.trans.XPathException xe = unxe.getXPathException();
                        if (!(getVariableQName().hasURI(Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE))) {
                            xe.setIsGlobalError(true);
                        }
                        throw xe;
                    } catch (Saxon.Hej.trans.XPathException e) {
                        if (!(getVariableQName().hasURI(Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE))) {
                            e.setIsGlobalError(true);
                        }
                        throw;
                    }
                }
            }
        }
        public virtual Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.instruct.Bindery b = controller.getBindery(getPackageData());
            Saxon.Hej.om.GroundedValue v = b.getGlobalVariable(getBinderySlotNumber());
            if ((v != null)) {
                return v;
            } else {
                return actuallyEvaluate(context, null);
            }
        }
        public virtual Saxon.Hej.om.GroundedValue evaluateVariable(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Component target) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.instruct.Bindery b = controller.getBindery(getPackageData());
            if ((b == null)) {
                throw new Saxon.Ejava.lang.AssertionError();
            }
            Saxon.Hej.om.GroundedValue v = b.getGlobalVariable(getBinderySlotNumber());
            if ((v != null)) {
                if ((v is Saxon.Hej.expr.instruct.Bindery.FailureValue)) {
                    throw ((Saxon.Hej.expr.instruct.Bindery.FailureValue)(v)).getObject();
                }
                return v;
            } else {
                return actuallyEvaluate(context, target);
            }
        }
        protected internal Saxon.Hej.om.GroundedValue actuallyEvaluate(Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Component target) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.expr.instruct.Bindery b = controller.getBindery(getPackageData());
            try {
                setDependencies(this, context);
                bool go = b.setExecuting(this);
                if (!(go)) {
                    return b.getGlobalVariable(getBinderySlotNumber());
                }
                Saxon.Hej.om.GroundedValue value = getSelectValue(context, target);
                if (_indexed) {
                    value = controller.getConfiguration().obtainOptimizer().makeIndexedValue(value.iterate());
                }
                return b.saveGlobalVariableValue(this, value);
            } catch (Saxon.Hej.trans.XPathException err) {
                b.setNotExecuting(this);
                if ((err is Saxon.Hej.trans.XPathException.Circularity)) {
                    err.setErrorCode((getPackageData().isXSLT() ? "XTDE0640" : "XQDY0054"));
                    err.setXPathContext(context);
                    err.setIsGlobalError(true);
                    b.setGlobalVariable(this, new Saxon.Hej.expr.instruct.Bindery.FailureValue(err));
                    err.setLocation(getLocation());
                    throw;
                } else {
                    throw;
                }
            }
        }
        protected internal static void setDependencies(Saxon.Hej.expr.instruct.GlobalVariable var, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            context = context.getMajorContext();
            while ((context != null)) {
                do {
                    Saxon.Hej.expr.ContextOriginator origin = ((Saxon.Hej.expr.XPathContextMajor)(context)).getOrigin();
                    if ((origin is Saxon.Hej.expr.instruct.GlobalVariable)) {
                        controller.registerGlobalVariableDependency((Saxon.Hej.expr.instruct.GlobalVariable)(origin), var);
                        return ;
                    }
                    context = context.getCaller();
                    if ((context != null)) {
                        context = context.getMajorContext();
                    }
                } while ((context != null));
            }
        }
        public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
            return ((getBody() == null) ? null : getBody().getIntegerBounds());
        }
        public int getLocalSlotNumber() {
            return 0;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName s) {
            variableQName = s;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableQName;
        }
        public string getDescription() {
            if (variableQName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE)) {
                return (("optimizer-generated global variable select=\""+getBody().toShortString())+'"');
            } else {
                return ("global variable "+getVariableQName().getDisplayName());
            }
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            bool asParam = ((this is Saxon.Hej.expr.instruct.GlobalParam) && !(isStatic()));
            presenter.startElement((asParam ? "globalParam" : "globalVariable"));
            presenter.emitAttribute("name", getVariableQName());
            presenter.emitAttribute("as", getRequiredType().toAlphaCode());
            presenter.emitAttribute("line", (getLineNumber()+""));
            presenter.emitAttribute("module", getSystemId());
            if ((getStackFrameMap() != null)) {
                presenter.emitAttribute("slots", (getStackFrameMap().getNumberOfVariables()+""));
            }
            if ((getDeclaringComponent() != null)) {
                Saxon.Hej.trans.Visibility vis = getDeclaringComponent().getVisibility();
                if ((vis != Saxon.Hej.trans.Visibility.UNDEFINED)) {
                    presenter.emitAttribute("visibility", vis.ToString());
                }
            }
            string flags = getFlags();
            if (!(flags.Length==0)) {
                presenter.emitAttribute("flags", flags);
            }
            if ((getBody() != null)) {
                getBody().export(presenter);
            }
            presenter.endElement();
        }
        protected internal virtual string getFlags() {
            string flags = "";
            if (_isAssignable) {
                flags += "a";
            }
            if (_indexed) {
                flags += "x";
            }
            if (_isRequiredParam) {
                flags += "r";
            }
            if (_isStatic) {
                flags += "s";
            }
            return flags;
        }
    
    }
}
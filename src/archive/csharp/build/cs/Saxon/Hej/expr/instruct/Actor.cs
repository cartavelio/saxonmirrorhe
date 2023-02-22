using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class Actor : Saxon.Hej.expr.ExpressionOwner, Saxon.Hej.s9api.Location {
        protected internal Saxon.Hej.expr.Expression body;
        private string systemId;
        private int lineNumber;
        private int columnNumber;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private Saxon.Hej.expr.PackageData packageData;
        private Saxon.Hej.expr.Component declaringComponent;
        private Saxon.Hej.trans.Visibility declaredVisibility = Saxon.Hej.trans.Visibility.UNDEFINED;
        private Saxon.Hej.expr.parser.RetainedStaticContext retainedStaticContext;
        private Saxon.Hej.expr.elab.PushEvaluator bodyEvaluator;
        public Actor()  {
        }
        public abstract Saxon.Hej.trans.SymbolicName getSymbolicName();
        public Saxon.Hej.om.StructuredQName getComponentName() {
            return getSymbolicName().getComponentName();
        }
        public virtual string getTracingTag() {
            return Saxon.Hej.om.StandardNames.getLocalName(getSymbolicName().getComponentKind());
        }
        public void setPackageData(Saxon.Hej.expr.PackageData packageData) {
            this.packageData = packageData;
        }
        public Saxon.Hej.expr.PackageData getPackageData() {
            return packageData;
        }
        public Saxon.Hej.expr.Component makeDeclaringComponent(Saxon.Hej.trans.Visibility visibility, Saxon.Hej.style.StylesheetPackage declaringPackage) {
            if ((declaringComponent == null)) {
                declaringComponent = Saxon.Hej.expr.Component.makeComponent(this, visibility, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED, declaringPackage, declaringPackage);
            }
            return declaringComponent;
        }
        public Saxon.Hej.expr.Component obtainDeclaringComponent(Saxon.Hej.style.StyleElement declaration) {
            if ((declaringComponent == null)) {
                Saxon.Hej.style.StylesheetPackage declaringPackage = declaration.getContainingPackage();
                Saxon.Hej.trans.Visibility defaultVisibility = ((declaration is Saxon.Hej.style.XSLGlobalParam) ? Saxon.Hej.trans.Visibility.PUBLIC : Saxon.Hej.trans.Visibility.PRIVATE);
                Saxon.Hej.trans.Visibility declaredVisibility = declaration.getDeclaredVisibility();
                Saxon.Hej.trans.Visibility actualVisibility = ((declaredVisibility == Saxon.Hej.trans.Visibility.UNDEFINED) ? defaultVisibility : declaredVisibility);
                Saxon.Hej.trans.VisibilityProvenance provenance = ((declaredVisibility == Saxon.Hej.trans.Visibility.UNDEFINED) ? Saxon.Hej.trans.VisibilityProvenance.DEFAULTED : Saxon.Hej.trans.VisibilityProvenance.EXPLICIT);
                declaringComponent = Saxon.Hej.expr.Component.makeComponent(this, actualVisibility, provenance, declaringPackage, declaringPackage);
            }
            return declaringComponent;
        }
        public virtual Saxon.Hej.expr.Component getDeclaringComponent() {
            return declaringComponent;
        }
        public void setDeclaringComponent(Saxon.Hej.expr.Component comp) {
            declaringComponent = comp;
        }
        public virtual void allocateAllBindingSlots(Saxon.Hej.style.StylesheetPackage pack) {
            if ((((getBody() != null) && (getDeclaringComponent().getDeclaringPackage() == pack)) && packageData.isXSLT())) {
                allocateBindingSlotsRecursive(pack, this, getBody(), getDeclaringComponent().getComponentBindings());
            }
        }
        public static void allocateBindingSlotsRecursive(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.expr.instruct.Actor p, Saxon.Hej.expr.Expression exp, System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> bindings) {
            if ((exp is Saxon.Hej.expr.ComponentInvocation)) {
                p.processComponentReference(pack, (Saxon.Hej.expr.ComponentInvocation)(exp), bindings);
            }
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                allocateBindingSlotsRecursive(pack, p, o.getChildExpression(), bindings);
            }
        }
        private void processComponentReference(Saxon.Hej.style.StylesheetPackage pack, Saxon.Hej.expr.ComponentInvocation invocation, System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> bindings) {
            Saxon.Hej.trans.SymbolicName name = invocation.getSymbolicName();
            if ((name == null)) {
                return ;
            }
            Saxon.Hej.expr.Component target = pack.getComponent(name);
            if ((((target == null) && name.getComponentName().hasURI(Saxon.Hej.om.NamespaceUri.XSLT)) && name.getComponentName().getLocalPart().Equals("original"))) {
                target = pack.getOverriddenComponent(getSymbolicName());
            }
            if ((target == null)) {
                throw new Saxon.Ejava.lang.AssertionError((("Target of component reference "+name)+" is undefined"));
            }
            if ((invocation.getBindingSlot()>=0)) {
                throw new Saxon.Ejava.lang.AssertionError((("**** Component reference "+name)+" is already bound"));
            }
            int slot = bindings.Count;
            Saxon.Hej.expr.ComponentBinding cb = new Saxon.Hej.expr.ComponentBinding(name, target);
            bindings.Add(cb);
            invocation.setBindingSlot(slot);
        }
        public virtual void setBody(Saxon.Hej.expr.Expression body) {
            this.body = body;
            if ((body != null)) {
                body.setParentExpression(null);
            }
        }
        public Saxon.Hej.expr.Expression getBody() {
            return body;
        }
        public Saxon.Hej.expr.Expression getChildExpression() {
            return getBody();
        }
        public virtual void setStackFrameMap(Saxon.Hej.expr.instruct.SlotManager map) {
            stackFrameMap = map;
        }
        public Saxon.Hej.expr.instruct.SlotManager getStackFrameMap() {
            return stackFrameMap;
        }
        public void setLineNumber(int lineNumber) {
            this.lineNumber = lineNumber;
        }
        public void setColumnNumber(int col) {
            this.columnNumber = col;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return this;
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public string getSystemId() {
            return systemId;
        }
        public int getColumnNumber() {
            return columnNumber;
        }
        public string getPublicId() {
            return null;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public void setRetainedStaticContext(Saxon.Hej.expr.parser.RetainedStaticContext rsc) {
            this.retainedStaticContext = rsc;
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext getRetainedStaticContext() {
            return retainedStaticContext;
        }
        public virtual System.Object getProperty(string name) {
            return null;
        }
        public void setDeclaredVisibility(Saxon.Hej.trans.Visibility visibility) {
            this.declaredVisibility = visibility;
        }
        public Saxon.Hej.trans.Visibility getDeclaredVisibility() {
            return declaredVisibility;
        }
        public abstract void export(Saxon.Hej.trace.ExpressionPresenter presenter);
        public virtual bool isExportable() {
            return true;
        }
        public void setChildExpression(Saxon.Hej.expr.Expression expr) {
            setBody(expr);
        }
        protected internal Saxon.Hej.expr.instruct.TailCall process(Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context) {
            lock (this) {
                if ((bodyEvaluator == null)) {
                    bodyEvaluator = body.makeElaborator().elaborateForPush();
                }
            }
            return /*Delegate.processLeavingTail()*/bodyEvaluator(@out, context);
        }
    
    }
}
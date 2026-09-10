using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class UserFunctionReference : Saxon.Hej.expr.Expression, Saxon.Hej.expr.ComponentInvocation, Saxon.Hej.expr.UserFunctionResolvable, Saxon.Hej.expr.Callable {
        private readonly Saxon.Hej.trans.SymbolicName functionName;
        private Saxon.Hej.expr.instruct.UserFunction nominalTarget;
        private int bindingSlot = -1;
        private int optimizeCounter = 0;
        private int typeCheckCounter = 0;
        public UserFunctionReference(Saxon.Hej.expr.instruct.UserFunction target)  {
            this.nominalTarget = target;
            this.functionName = target.getSymbolicName();
        }
        public UserFunctionReference(Saxon.Hej.trans.SymbolicName name)  {
            this.functionName = name;
        }
        public void setFunction(Saxon.Hej.expr.instruct.UserFunction function) {
            if (!(function.getSymbolicName().Equals(functionName))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Function name does not match");
            }
            this.nominalTarget = function;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((nominalTarget.getFunctionName().hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS) && (typeCheckCounter == 0))) {
                typeCheckCounter++;
                nominalTarget.typeCheck(visitor);
                typeCheckCounter--;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((nominalTarget.getFunctionName().hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS) && (optimizeCounter == 0))) {
                optimizeCounter++;
                Saxon.Hej.expr.Expression o;
                o = nominalTarget.getBody().optimize(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
                nominalTarget.setBody(o);
                Saxon.Hej.expr.instruct.SlotManager slotManager = visitor.getConfiguration().makeSlotManager();
                for (int i = 0; (i<getArity()); i++) {
                    slotManager.allocateSlotNumber(nominalTarget.getParameterDefinitions()[i].getVariableQName());
                }
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(o, getArity(), slotManager);
                nominalTarget.setStackFrameMap(slotManager);
                optimizeCounter--;
            }
            return this;
        }
        public int getBindingSlot() {
            return bindingSlot;
        }
        public Saxon.Hej.expr.Component getFixedTarget() {
            return nominalTarget.getDeclaringComponent();
        }
        public Saxon.Hej.expr.instruct.UserFunction getNominalTarget() {
            return nominalTarget;
        }
        public void setBindingSlot(int slot) {
            bindingSlot = slot;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return functionName;
        }
        public Saxon.Hej.type.FunctionItemType getFunctionItemType(Saxon.Hej.type.TypeHierarchy th) {
            return nominalTarget.getFunctionItemType();
        }
        public Saxon.Hej.om.StructuredQName getFunctionName() {
            return nominalTarget.getFunctionName();
        }
        public int getArity() {
            return nominalTarget.getArity();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return nominalTarget.getFunctionItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.FUNCTION;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.functions.hof.UserFunctionReference @ref = new Saxon.Hej.functions.hof.UserFunctionReference(nominalTarget);
            @ref.optimizeCounter = optimizeCounter;
            @ref.typeCheckCounter = typeCheckCounter;
            return @ref;
        }
        public override Saxon.Hej.om.Function evaluateItem(Saxon.Hej.expr.XPathContext context) {
            if ((bindingSlot == -1)) {
                return new Saxon.Hej.functions.hof.UserFunctionReference.BoundUserFunction(this, nominalTarget, nominalTarget.getDeclaringComponent(), context.getController());
            } else {
                Saxon.Hej.expr.Component targetComponent = context.getTargetComponent(bindingSlot);
                return new Saxon.Hej.functions.hof.UserFunctionReference.BoundUserFunction(this, (Saxon.Hej.expr.instruct.UserFunction)(targetComponent.getActor()), targetComponent, context.getController());
            }
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evaluateItem(context);
        }
        public override string getExpressionName() {
            return "UserFunctionReference";
        }
        public override string ToString() {
            return ((getFunctionName().getEQName()+"#")+getArity());
        }
        public override string toShortString() {
            return ((getFunctionName().getDisplayName()+"#")+getArity());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = (Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(@out.getOptions());
            if (("JS".Equals(options.target) && (options.targetVersion == 1))) {
                throw new Saxon.Hej.trans.XPathException("Higher-order functions are not available in Saxon-JS v1.*", "XTSE3540", getLocation());
            }
            if ((nominalTarget.getDeclaringComponent() == null)) {
                @out.startElement("inlineFn");
                nominalTarget.export(@out);
                @out.endElement();
            } else {
                Saxon.Hej.style.StylesheetPackage rootPackage = options.rootPackage;
                Saxon.Hej.style.StylesheetPackage containingPackage = nominalTarget.getDeclaringComponent().getContainingPackage();
                if (((rootPackage != null) && !((((rootPackage == containingPackage) || rootPackage.contains(containingPackage)))))) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot export a package containing a reference to a user-defined function ("+toShortString())+") that is not present in the package being exported"));
                }
                @out.startElement("ufRef");
                @out.emitAttribute("name", nominalTarget.getFunctionName());
                @out.emitAttribute("arity", (nominalTarget.getArity()+""));
                @out.emitAttribute("bSlot", (""+getBindingSlot()));
                @out.endElement();
            }
        }
        public class BoundUserFunction : Saxon.Hej.functions.AbstractFunction, Saxon.Hej.expr.ContextOriginator {
            private readonly Saxon.Hej.expr.ExportAgent agent;
            private readonly Saxon.Hej.om.Function function;
            private readonly Saxon.Hej.expr.Component component;
            private readonly Saxon.Hej.Controller controller;
            public BoundUserFunction(Saxon.Hej.expr.ExportAgent agent, Saxon.Hej.om.Function function, Saxon.Hej.expr.Component component, Saxon.Hej.Controller controller)  {
                this.agent = agent;
                this.function = function;
                this.component = component;
                this.controller = controller;
            }
            public Saxon.Hej.om.Function getTargetFunction() {
                return function;
            }
            public Saxon.Hej.Controller getController() {
                return controller;
            }
            public override Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext oldContext, Saxon.Hej.expr.ContextOriginator originator) {
                if ((controller.getConfiguration() != oldContext.getConfiguration())) {
                    throw new Saxon.Ejava.lang.IllegalStateException("A function created under one Configuration cannot be called under a different Configuration");
                }
                Saxon.Hej.expr.XPathContextMajor c2;
                c2 = controller.newXPathContext();
                c2.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_FUNCTION);
                c2.setCurrentOutputUri(null);
                c2.setCurrentComponent(component);
                c2.setURIResolver(oldContext.getURIResolver());
                c2.setOrigin(originator);
                return function.makeNewContext(c2, originator);
            }
            public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                Saxon.Hej.expr.XPathContext c2 = function.makeNewContext(context, this);
                if (((c2 is Saxon.Hej.expr.XPathContextMajor) && (component != null))) {
                    ((Saxon.Hej.expr.XPathContextMajor)(c2)).setCurrentComponent(component);
                }
                return function.call(c2, args);
            }
            public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
                return function.getFunctionItemType();
            }
            public override Saxon.Hej.query.AnnotationList getAnnotations() {
                return function.getAnnotations();
            }
            public override Saxon.Hej.om.StructuredQName getFunctionName() {
                return function.getFunctionName();
            }
            public override int getArity() {
                return function.getArity();
            }
            public override string getDescription() {
                return function.getDescription();
            }
            public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
                agent.export(@out);
            }
        
        }
    
    }
}
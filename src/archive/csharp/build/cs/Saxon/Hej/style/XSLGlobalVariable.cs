using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLGlobalVariable : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private Saxon.Hej.expr.instruct.SlotManager slotManager;
        protected internal Saxon.Hej.style.SourceBinding sourceBinding;
        protected internal Saxon.Hej.expr.instruct.GlobalVariable compiledVariable = null;
        public XSLGlobalVariable()  {
            sourceBinding = new Saxon.Hej.style.SourceBinding(this);
            sourceBinding.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.GLOBAL, true);
        }
        public Saxon.Hej.style.SourceBinding getSourceBinding() {
            return sourceBinding;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return sourceBinding.getVariableQName();
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            return sourceBinding.getVariableQName();
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public bool isGlobal() {
            return isTopLevel();
        }
        public override void postValidate() {
            sourceBinding.postValidate();
        }
        public Saxon.Hej.expr.instruct.GlobalVariable getCompiledVariable() {
            return compiledVariable;
        }
        protected internal virtual Saxon.Hej.style.SourceBinding.BindingProperty getPermittedAttributes() {
            return (Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE|Saxon.Hej.style.SourceBinding.BindingProperty.SELECT|Saxon.Hej.style.SourceBinding.BindingProperty.AS|Saxon.Hej.style.SourceBinding.BindingProperty.STATIC|Saxon.Hej.style.SourceBinding.BindingProperty.VISIBILITY);
        }
        private int state = 0;
        protected internal bool redundant = false;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            Saxon.Hej.expr.instruct.GlobalVariable gv = getCompiledVariable();
            if ((gv == null)) {
                gv = ((this is Saxon.Hej.style.XSLGlobalParam) ? new Saxon.Hej.expr.instruct.GlobalParam() : new Saxon.Hej.expr.instruct.GlobalVariable());
                gv.setPackageData(getCompilation().getPackageData());
                gv.obtainDeclaringComponent(this);
                gv.setRequiredType(sourceBinding.getDeclaredType());
                gv.setDeclaredVisibility(getDeclaredVisibility());
                gv.setVariableQName(sourceBinding.getVariableQName());
                gv.setSystemId(getSystemId());
                gv.setLineNumber(getLineNumber());
                gv.setColumnNumber(getColumnNumber());
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = makeRetainedStaticContext();
                gv.setRetainedStaticContext(rsc);
                if ((gv.getBody() != null)) {
                    gv.getBody().setRetainedStaticContext(rsc);
                }
                compiledVariable = gv;
            }
            return gv;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_VARIABLE, getObjectName());
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
            Saxon.Hej.value.SequenceType st1 = getSourceBinding().getDeclaredType();
            if ((st1 == null)) {
                st1 = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
            Saxon.Hej.expr.instruct.GlobalVariable other = (Saxon.Hej.expr.instruct.GlobalVariable)(component.getActor());
            Saxon.Hej.type.TypeHierarchy th = component.getDeclaringPackage().getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.Affinity relation = th.sequenceTypeRelationship(st1, other.getRequiredType());
            if ((relation != Saxon.Hej.type.Affinity.SAME_TYPE)) {
                compileError((("The declared type of the overriding variable $"+getVariableQName().getDisplayName())+" is different from that of the overridden variable"), "XTSE3070");
            }
        }
        public override Saxon.Hej.style.SourceBinding getBindingInformation(Saxon.Hej.om.StructuredQName name) {
            if (name.Equals(sourceBinding.getVariableQName())) {
                return sourceBinding;
            } else {
                return null;
            }
        }
        protected internal override void prepareAttributes() {
            if ((state == 2)) {
                return ;
            }
            if ((state == 1)) {
                compileError("Circular reference to variable", "XTDE0640");
            }
            state = 1;
            sourceBinding.prepareAttributes(getPermittedAttributes());
            state = 2;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            top.indexVariableDeclaration(decl);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            slotManager = getConfiguration().makeSlotManager();
            sourceBinding.validate();
        }
        public bool isAssignable() {
            return sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE);
        }
        public override bool isDeclaration() {
            return true;
        }
        public override bool isInstruction() {
            return false;
        }
        public virtual Saxon.Hej.value.SequenceType getRequiredType() {
            return sourceBinding.getInferredType(true);
        }
        public override void fixupReferences() {
            sourceBinding.fixupReferences(compiledVariable);
            base.fixupReferences();
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(redundant)) {
                sourceBinding.handleSequenceConstructor(compilation, decl);
                Saxon.Hej.expr.instruct.GlobalVariable inst = getCompiledVariable();
                if ((inst == null)) {
                    inst = new Saxon.Hej.expr.instruct.GlobalVariable();
                    inst.setPackageData(getCompilation().getPackageData());
                    inst.obtainDeclaringComponent(this);
                    inst.setVariableQName(sourceBinding.getVariableQName());
                }
                if (sourceBinding.isStatic()) {
                    inst.setStatic(true);
                    Saxon.Hej.om.GroundedValue value = compilation.getStaticVariable(sourceBinding.getVariableQName());
                    if ((value == null)) {
                        throw new Saxon.Ejava.lang.AssertionError();
                    }
                    Saxon.Hej.expr.Expression select = Saxon.Hej.expr.Literal.makeLiteral(value);
                    select.setRetainedStaticContext(makeRetainedStaticContext());
                    inst.setBody(select);
                } else {
                    Saxon.Hej.expr.Expression select = sourceBinding.getSelectExpression();
                    inst.setBody(select);
                    if ((compilation.getCompilerInfo().getCodeInjector() != null)) {
                        compilation.getCompilerInfo().getCodeInjector().process(inst);
                    }
                }
                inst.setRetainedStaticContext(makeRetainedStaticContext());
                initializeBinding(inst);
                inst.setAssignable(isAssignable());
                inst.setRequiredType(getRequiredType());
                sourceBinding.fixupBinding(inst);
                compiledVariable = inst;
                Saxon.Hej.expr.Component overridden = getOverriddenComponent();
                if ((overridden != null)) {
                    checkCompatibility(overridden);
                }
            }
        }
        protected internal void initializeBinding(Saxon.Hej.expr.instruct.GlobalVariable var) {
            Saxon.Hej.expr.Expression select = var.getBody();
            Saxon.Hej.expr.Expression exp2 = select;
            if ((exp2 != null)) {
                try {
                    Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                    Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = getPackageData().getContextItemRequirements();
                    Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = ((gcr == null) ? getConfiguration().getDefaultContextItemStaticInfo() : gcr.makeGlobalContextInfo(getConfiguration()));
                    exp2 = select.simplify().typeCheck(visitor, cisi);
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
                setInstructionLocation(this, exp2);
                allocateLocalSlots(exp2);
            }
            if (((slotManager != null) && (slotManager.getNumberOfVariables()>0))) {
                var.setContainsLocals(slotManager);
            }
            if ((exp2 != select)) {
                var.setBody(exp2);
            }
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return slotManager;
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
            if ((!(redundant) && (compiledVariable.getBody() != null))) {
                Saxon.Hej.expr.Expression exp2 = compiledVariable.getBody();
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                exp2 = Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(exp2, getCompilation(), visitor, getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true), false);
                allocateLocalSlots(exp2);
                if (((slotManager != null) && (slotManager.getNumberOfVariables()>0))) {
                    compiledVariable.setContainsLocals(slotManager);
                }
                if ((exp2 != compiledVariable.getBody())) {
                    compiledVariable.setBody(exp2);
                }
            }
        }
        public void setRedundant(bool redundant) {
            this.redundant = redundant;
        }
    
    }
}
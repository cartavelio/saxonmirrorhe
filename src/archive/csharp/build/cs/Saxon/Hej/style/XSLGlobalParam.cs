using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLGlobalParam : Saxon.Hej.style.XSLGlobalVariable {
        protected internal override Saxon.Hej.style.SourceBinding.BindingProperty getPermittedAttributes() {
            return (Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED|Saxon.Hej.style.SourceBinding.BindingProperty.SELECT|Saxon.Hej.style.SourceBinding.BindingProperty.AS|Saxon.Hej.style.SourceBinding.BindingProperty.STATIC);
        }
        internal Saxon.Hej.expr.Expression conversion = null;
        public XSLGlobalParam()  {
            sourceBinding.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM, true);
        }
        public override Saxon.Hej.trans.Visibility getVisibility() {
            string statik = getAttributeValue("static");
            if ((statik == null)) {
                return Saxon.Hej.trans.Visibility.PUBLIC;
            } else {
                bool isStatic = processBooleanAttribute("static", statik);
                return (isStatic ? Saxon.Hej.trans.Visibility.PRIVATE : Saxon.Hej.trans.Visibility.PUBLIC);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED)) {
                if ((sourceBinding.getSelectExpression() != null)) {
                    compileError("The select attribute must be absent when required='yes'", "XTSE0010");
                }
                if (hasChildNodes()) {
                    compileError("A parameter specifying required='yes' must have empty content", "XTSE0010");
                }
                Saxon.Hej.trans.Visibility vis = getVisibility();
                if ((!(sourceBinding.isStatic()) && !(((((vis == Saxon.Hej.trans.Visibility.PUBLIC) || (vis == Saxon.Hej.trans.Visibility.FINAL)) || (vis == Saxon.Hej.trans.Visibility.ABSTRACT)))))) {
                    compileError("The visibility of a required non-static parameter must be public, final, or abstract", "XTSE3370");
                }
            }
            base.validate(decl);
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if (sourceBinding.isStatic()) {
                base.compileDeclaration(compilation, decl);
            } else {
                if (!(redundant)) {
                    sourceBinding.handleSequenceConstructor(compilation, decl);
                    Saxon.Hej.expr.instruct.GlobalParam binding = (Saxon.Hej.expr.instruct.GlobalParam)(compiledVariable);
                    binding.setPackageData(getCompilation().getPackageData());
                    binding.obtainDeclaringComponent(this);
                    Saxon.Hej.expr.Expression select = sourceBinding.getSelectExpression();
                    binding.setBody(select);
                    binding.setVariableQName(sourceBinding.getVariableQName());
                    initializeBinding(binding);
                    if (((select != null) && (compilation.getCompilerInfo().getCodeInjector() != null))) {
                        compilation.getCompilerInfo().getCodeInjector().process(binding);
                    }
                    binding.setRequiredType(getRequiredType());
                    binding.setRequiredParam(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED));
                    binding.setImplicitlyRequiredParam(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.IMPLICITLY_REQUIRED));
                    sourceBinding.fixupBinding(binding);
                    Saxon.Hej.expr.Component overridden = getOverriddenComponent();
                    if ((overridden != null)) {
                        checkCompatibility(overridden);
                    }
                }
            }
        }
        public override Saxon.Hej.value.SequenceType getRequiredType() {
            Saxon.Hej.value.SequenceType declaredType = sourceBinding.getDeclaredType();
            if ((declaredType != null)) {
                return declaredType;
            } else {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
        }
        public void insertBytecodeCandidate(Saxon.Hej.expr.parser.Optimizer opt) {
        }
    
    }
}
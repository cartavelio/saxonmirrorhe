using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLLocalVariable : Saxon.Hej.style.XSLGeneralVariable {
        private static readonly Saxon.Hej.style.SourceBinding.BindingProperty permittedAttributes = (Saxon.Hej.style.SourceBinding.BindingProperty.SELECT|Saxon.Hej.style.SourceBinding.BindingProperty.AS);
        public override Saxon.Hej.style.SourceBinding getBindingInformation(Saxon.Hej.om.StructuredQName name) {
            if (name.Equals(sourceBinding.getVariableQName())) {
                return sourceBinding;
            } else {
                return null;
            }
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            sourceBinding.prepareAttributes(permittedAttributes);
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return sourceBinding.getInferredType(true);
        }
        public override void fixupReferences() {
            sourceBinding.fixupReferences(null);
            base.fixupReferences();
        }
        public void compileLocalVariable(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            sourceBinding.handleSequenceConstructor(exec, decl);
        }
    
    }
}
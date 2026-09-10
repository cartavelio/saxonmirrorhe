using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLGeneralVariable : Saxon.Hej.style.StyleElement {
        protected internal Saxon.Hej.style.SourceBinding sourceBinding;
        public XSLGeneralVariable()  {
            sourceBinding = new Saxon.Hej.style.SourceBinding(this);
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
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            sourceBinding.validate();
        }
        public override void postValidate() {
            sourceBinding.postValidate();
        }
    
    }
}
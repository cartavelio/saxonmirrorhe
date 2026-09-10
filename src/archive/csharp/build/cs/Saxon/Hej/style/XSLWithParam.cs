using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLWithParam : Saxon.Hej.style.XSLGeneralVariable {
        private readonly Saxon.Hej.style.SourceBinding.BindingProperty allowedAttributes = (Saxon.Hej.style.SourceBinding.BindingProperty.SELECT|Saxon.Hej.style.SourceBinding.BindingProperty.AS|Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL);
        protected internal override void prepareAttributes() {
            sourceBinding.prepareAttributes(allowedAttributes);
        }
        public bool isTunnelParam() {
            return sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            base.validate(decl);
            Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
            Saxon.Hej.om.Item prev;
            while (((prev = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                if ((prev is Saxon.Hej.style.XSLWithParam)) {
                    if (sourceBinding.getVariableQName().Equals(((Saxon.Hej.style.XSLWithParam)(prev)).sourceBinding.getVariableQName())) {
                        compileError("Duplicate parameter name", "XTSE0670");
                    }
                }
            }
        }
        public void checkAgainstRequiredType(Saxon.Hej.value.SequenceType required) {
            sourceBinding.checkAgainstRequiredType(required);
        }
        public Saxon.Hej.expr.instruct.WithParam compileWithParam(Saxon.Hej.expr.Expression parent, Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            sourceBinding.handleSequenceConstructor(exec, decl);
            Saxon.Hej.expr.instruct.WithParam inst = new Saxon.Hej.expr.instruct.WithParam();
            inst.setSelectExpression(parent, sourceBinding.getSelectExpression());
            inst.setVariableQName(sourceBinding.getVariableQName());
            inst.setRequiredType(sourceBinding.getInferredType(true));
            return inst;
        }
    
    }
}
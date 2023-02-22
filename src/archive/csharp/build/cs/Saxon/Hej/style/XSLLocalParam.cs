using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLLocalParam : Saxon.Hej.style.XSLGeneralVariable {
        private Saxon.Hej.style.SourceBinding.BindingProperty permittedAttributes = (Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL|Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED|Saxon.Hej.style.SourceBinding.BindingProperty.SELECT|Saxon.Hej.style.SourceBinding.BindingProperty.AS);
        internal Saxon.Hej.expr.Expression conversion = null;
        private int slotNumber = -9876;
        private Saxon.Hej.expr.instruct.LocalParam compiledParam;
        private bool prepared = false;
        public override Saxon.Hej.style.SourceBinding getBindingInformation(Saxon.Hej.om.StructuredQName name) {
            if (name.Equals(sourceBinding.getVariableQName())) {
                return sourceBinding;
            } else {
                return null;
            }
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        protected internal override void prepareAttributes() {
            if (!(prepared)) {
                prepared = true;
                sourceBinding.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM, true);
                if ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLFunction)) {
                    permittedAttributes &= ~(Saxon.Hej.style.SourceBinding.BindingProperty.SELECT);
                    sourceBinding.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.DISALLOWS_CONTENT, true);
                }
                sourceBinding.prepareAttributes(permittedAttributes);
                if ((sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL) && !(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLTemplate))))) {
                    compileError((("For attribute 'tunnel' within an "+((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getDisplayName())+" parameter, the only permitted value is 'no'"), "XTSE0020");
                }
                if (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLFunction) && (getAttributeValue("", "required") != null))) {
                    if (!(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED))) {
                        compileError((("For attribute 'required' within an "+((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getDisplayName())+" parameter, the only permitted value is 'yes'"), "XTSE0020");
                    }
                }
            }
        }
        public void prepareTemplateSignatureAttributes() {
            if (!(prepared)) {
                sourceBinding.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM, true);
                sourceBinding.prepareTemplateSignatureAttributes();
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.om.StructuredQName name = sourceBinding.getVariableQName();
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            if (!(((((parent is Saxon.Hej.style.StyleElement)) && ((Saxon.Hej.style.StyleElement)(parent)).mayContainParam())))) {
                compileError("xsl:param must be immediately within a template, function or stylesheet", "XTSE0010");
            }
            if ((hasChildNodes() && (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLFunction))) {
                compileError("Function parameters cannot have a default value", "XTSE0760");
            }
            Saxon.Hej.om.SequenceTool.supply(iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((node => {
                if ((node is Saxon.Hej.style.XSLLocalParam)) {
                    if (name.Equals(((Saxon.Hej.style.XSLLocalParam)(node)).sourceBinding.getVariableQName())) {
                        compileError("The name of the parameter is not unique", "XTSE0580");
                    }
                } else {
                    if (((node is Saxon.Hej.style.StyleElement) && (((Saxon.Hej.style.StyleElement)(node)).getFingerprint() != Saxon.Hej.om.StandardNames.XSL_CONTEXT_ITEM))) {
                        compileError("xsl:param must not be preceded by other instructions", "XTSE0010");
                    } else {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(node.getUnicodeStringValue()))) {
                            compileError("xsl:param must not be preceded by text", "XTSE0010");
                        }
                    }
                }
            } )));
            Saxon.Hej.expr.instruct.SlotManager p = getContainingSlotManager();
            if ((p == null)) {
                compileError("Local variable must be declared within a template or function", "XTSE0010");
            } else {
                slotNumber = p.allocateSlotNumber(name);
            }
            if (sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED)) {
                if ((sourceBinding.getSelectExpression() != null)) {
                    compileError("The select attribute must be omitted when required='yes'", "XTSE0010");
                }
                if (hasChildNodes()) {
                    compileError("A parameter specifying required='yes' must have empty content", "XTSE0010");
                }
            }
            base.validate(decl);
        }
        public bool isTunnelParam() {
            return sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL);
        }
        public bool isRequiredParam() {
            return sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED);
        }
        public override void fixupReferences() {
            sourceBinding.fixupReferences(null);
            base.fixupReferences();
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLFunction)) {
                return null;
            } else {
                Saxon.Hej.value.SequenceType declaredType = getRequiredType();
                Saxon.Hej.om.StructuredQName name = sourceBinding.getVariableQName();
                int slot = getSlotNumber();
                if ((declaredType != null)) {
                    Saxon.Hej.expr.SuppliedParameterReference pref = new Saxon.Hej.expr.SuppliedParameterReference(slot);
                    pref.setRetainedStaticContext(makeRetainedStaticContext());
                    pref.setLocation(allocateLocation());
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.PARAM, name.getDisplayName(), 0);
                    role.setErrorCode("XTTE0590");
                    conversion = exec.getConfiguration().getTypeChecker(false).staticTypeCheck(pref, declaredType, role, makeExpressionVisitor());
                }
                sourceBinding.handleSequenceConstructor(exec, decl);
                Saxon.Hej.expr.instruct.LocalParam binding = new Saxon.Hej.expr.instruct.LocalParam();
                binding.setSelectExpression(sourceBinding.getSelectExpression());
                binding.setConversion(conversion);
                binding.setVariableQName(name);
                binding.setSlotNumber(slot);
                binding.setRequiredType(getRequiredType());
                binding.setRequiredParam(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED));
                binding.setImplicitlyRequiredParam(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.IMPLICITLY_REQUIRED));
                binding.setTunnel(sourceBinding.hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL));
                sourceBinding.fixupBinding(binding);
                return compiledParam = binding;
            }
        }
        public Saxon.Hej.expr.instruct.LocalParam getCompiledParam() {
            return compiledParam;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            Saxon.Hej.value.SequenceType declaredType = sourceBinding.getDeclaredType();
            if ((declaredType != null)) {
                return declaredType;
            } else {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            }
        }
    
    }
}
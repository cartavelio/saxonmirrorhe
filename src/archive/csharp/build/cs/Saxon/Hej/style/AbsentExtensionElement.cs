using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class AbsentExtensionElement : Saxon.Hej.style.StyleElement {
        internal Saxon.Hej.expr.instruct.CallTemplate instruction;
        internal bool useTailRecursion;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override void processAllAttributes() {
            if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.IGNORED_INSTRUCTION)) {
                return ;
            }
            if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS)) {
                compileError(validationError);
            }
            if ((getEffectiveVersion()>=40)) {
                Saxon.Hej.style.PrincipalStylesheetModule pack = getPrincipalStylesheetModule();
                Saxon.Hej.expr.instruct.NamedTemplate template = pack.getNamedTemplate(getNodeName().getStructuredQName());
                if ((template != null)) {
                    Saxon.Hej.expr.instruct.CallTemplate insn = new Saxon.Hej.expr.instruct.CallTemplate(template, template.getTemplateName(), false, ((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).isWithinDeclaredStreamableConstruct());
                    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> declaredParams = template.getLocalParamDetails();
                    foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo param in declaredParams) {
                        if ((param.isRequired && !(param.isTunnel))) {
                            if ((attributes().get(param.name.getURI(), param.name.getLocalPart()) == null)) {
                                compileError(("No value supplied for required parameter "+Saxon.Hej.trans.Err.wrap(param.name.getDisplayName(), Saxon.Hej.trans.Err.VARIABLE)), "XTSE0690");
                            }
                        }
                    }
                    System.Collections.Generic.IList<Saxon.Hej.expr.instruct.WithParam> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.WithParam>();
                    foreach (Saxon.Hej.om.AttributeInfo att in attributes().asList()) {
                        Saxon.Hej.om.StructuredQName name = att.getNodeName().getStructuredQName();
                        if ((!(name.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT)) && !(name.hasURI(Saxon.Hej.lib.NamespaceConstant.XML)))) {
                            foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo param in declaredParams) {
                                if ((param.name.Equals(name) && !(param.isTunnel))) {
                                    Saxon.Hej.value.SequenceType required = param.requiredType;
                                    Saxon.Hej.expr.instruct.WithParam withParam = new Saxon.Hej.expr.instruct.WithParam();
                                    withParam.setVariableQName(name);
                                    withParam.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                                    if (((required.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && required.getPrimaryType().isPlainType())) {
                                        if ((required.getPrimaryType() == Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)) {
                                            Saxon.Hej.expr.Expression avt = makeAttributeValueTemplate(att.getValue(), att);
                                            Saxon.Hej.expr.Expression toBool = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("yes-no-boolean", 1).makeFunctionCall(avt);
                                            withParam.setSelectExpression(insn, toBool);
                                        } else {
                                            Saxon.Hej.expr.Expression avt = makeAttributeValueTemplate(att.getValue(), att);
                                            if ((required.getPrimaryType() != Saxon.Hej.type.BuiltInAtomicType.STRING)) {
                                                avt = new Saxon.Hej.expr.AtomicSequenceConverter(avt, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                                            }
                                            withParam.setSelectExpression(insn, avt);
                                        }
                                    } else {
                                        Saxon.Hej.expr.Expression select = makeExpression(att.getValue(), att);
                                        withParam.setSelectExpression(insn, select);
                                    }
                                    @params.Add(withParam);
                                }
                            }
                        }
                    }
                    insn.setActualParameters(@params.ToArray(), new Saxon.Hej.expr.instruct.WithParam[]{});
                    this.instruction = insn;
                }
            }
            if ((isTopLevel() && forwardsCompatibleModeIsEnabled())) {
            } else {
                base.processAllAttributes();
            }
        }
        protected internal override void prepareAttributes() {
        }
        protected internal override bool markTailCalls() {
            useTailRecursion = true;
            if ((instruction != null)) {
                instruction.setTailRecursive(true);
            }
            return true;
        }
        public override void validateSubtree(Saxon.Hej.style.ComponentDeclaration decl, bool excludeStylesheet) {
            if (((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.IGNORED_INSTRUCTION) || ((isTopLevel() && forwardsCompatibleModeIsEnabled())))) {
            } else {
                base.validateSubtree(decl, excludeStylesheet);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((instruction != null)) {
                return instruction;
            }
            if ((isTopLevel() || (reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.IGNORED_INSTRUCTION))) {
                return null;
            }
            if ((validationError == null)) {
                validationError = new Saxon.Hej.trans.XmlProcessingIncident("Unknown instruction");
            }
            return fallbackProcessing(exec, decl, this);
        }
    
    }
}
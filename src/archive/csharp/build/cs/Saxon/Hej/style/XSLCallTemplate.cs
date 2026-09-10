using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLCallTemplate : Saxon.Hej.style.StyleElement {
        private static readonly Saxon.Hej.om.StructuredQName ERROR_TEMPLATE_NAME = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, "error-template");
        private Saxon.Hej.om.StructuredQName calledTemplateName;
        private Saxon.Hej.expr.instruct.NamedTemplate template = null;
        private bool useTailRecursion = false;
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string nameAttribute = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("name")) {
                    nameAttribute = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((nameAttribute == null)) {
                calledTemplateName = ERROR_TEMPLATE_NAME;
                reportAbsence("name");
                return ;
            }
            calledTemplateName = makeQName(nameAttribute, null, "name");
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLWithParam)) {
                } else {
                    if (((child is Saxon.Hej.style.XSLFallback) && mayContainFallback())) {
                    } else {
                        if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                            if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                                compileError("No character data is allowed within xsl:call-template", "XTSE0010");
                            }
                        } else {
                            compileError((("Child element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of xsl:call-template"), "XTSE0010");
                        }
                    }
                }
            }
            if ((calledTemplateName == null)) {
                calledTemplateName = ERROR_TEMPLATE_NAME;
            }
            if (!(calledTemplateName.Equals(ERROR_TEMPLATE_NAME))) {
                template = findTemplate(calledTemplateName);
            }
        }
        public override void postValidate() {
            if ((template != null)) {
                checkParams();
            } else {
                throw new Saxon.Ejava.lang.AssertionError("Target template not known");
            }
        }
        private void checkParams() {
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> declaredParams = template.getLocalParamDetails();
            foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo param in declaredParams) {
                if ((param.isRequired && !(param.isTunnel))) {
                    bool ok = false;
                    foreach (Saxon.Hej.om.NodeInfo withParam in children(x => (x is Saxon.Hej.style.XSLWithParam))) {
                        if (((Saxon.Hej.style.XSLWithParam)(withParam)).getVariableQName().Equals(param.name)) {
                            ok = true;
                            break ;
                        }
                    }
                    if (!(ok)) {
                        compileError(("No value supplied for required parameter "+Saxon.Hej.trans.Err.wrap(param.name.getDisplayName(), Saxon.Hej.trans.Err.VARIABLE)), "XTSE0690");
                    }
                }
            }
            foreach (Saxon.Hej.om.NodeInfo w in children()) {
                if (((w is Saxon.Hej.style.XSLWithParam) && !(((Saxon.Hej.style.XSLWithParam)(w)).isTunnelParam()))) {
                    Saxon.Hej.style.XSLWithParam withParam = (Saxon.Hej.style.XSLWithParam)(w);
                    bool ok = false;
                    foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo param in declaredParams) {
                        if ((param.name.Equals(withParam.getVariableQName()) && !(param.isTunnel))) {
                            ok = true;
                            Saxon.Hej.value.SequenceType required = param.requiredType;
                            withParam.checkAgainstRequiredType(required);
                            break ;
                        }
                    }
                    if ((!(ok) && !(xPath10ModeIsEnabled()))) {
                        compileError((("Parameter "+withParam.getVariableQName().getDisplayName())+" is not declared in the called template"), "XTSE0680");
                    }
                }
            }
        }
        private Saxon.Hej.expr.instruct.NamedTemplate findTemplate(Saxon.Hej.om.StructuredQName templateName) {
            Saxon.Hej.style.PrincipalStylesheetModule pack = getPrincipalStylesheetModule();
            Saxon.Hej.expr.instruct.NamedTemplate template = pack.getNamedTemplate(templateName);
            if ((template == null)) {
                if ((templateName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT) && templateName.getLocalPart().Equals("original"))) {
                    return (Saxon.Hej.expr.instruct.NamedTemplate)(getXslOriginal(Saxon.Hej.om.StandardNames.XSL_TEMPLATE));
                }
                compileError(("Cannot find a template named "+calledTemplateName), "XTSE0650");
            }
            return template;
        }
        protected internal override bool markTailCalls() {
            useTailRecursion = true;
            return true;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((template == null)) {
                return null;
            }
            Saxon.Hej.expr.instruct.CallTemplate call = new Saxon.Hej.expr.instruct.CallTemplate(template, calledTemplateName, useTailRecursion, isWithinDeclaredStreamableConstruct());
            call.setLocation(allocateLocation());
            call.setActualParameters(getWithParamInstructions(call, exec, decl, false), getWithParamInstructions(call, exec, decl, true));
            return call;
        }
    
    }
}
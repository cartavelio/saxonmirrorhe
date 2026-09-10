using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLNextIteration : Saxon.Hej.style.XSLBreakOrContinue {
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            validatePosition();
            if ((xslIterate == null)) {
                compileError("xsl:next-iteration must be a descendant of an xsl:iterate instruction");
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLWithParam)) {
                    if (((Saxon.Hej.style.XSLWithParam)(child)).isTunnelParam()) {
                        compileError("An xsl:with-param element within xsl:iterate must not specify tunnel='yes'", "XTSE0020");
                    }
                } else {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            compileError("No character data is allowed within xsl:next-iteration", "XTSE0010");
                        }
                    } else {
                        compileError((("Child element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of xsl:next-iteration"), "XTSE0010");
                    }
                }
            }
        }
        public override void postValidate() {
            if ((xslIterate == null)) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo w in children(x => (x is Saxon.Hej.style.XSLWithParam))) {
                Saxon.Hej.style.XSLWithParam withParam = (Saxon.Hej.style.XSLWithParam)(w);
                Saxon.Hej.tree.iter.AxisIterator formalParams = xslIterate.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                bool ok = false;
                Saxon.Hej.om.NodeInfo param;
                while (((param = ((/*1430*/Saxon.Hej.om.NodeInfo)formalParams.next())) != null)) {
                    if (((param is Saxon.Hej.style.XSLLocalParam) && ((Saxon.Hej.style.XSLLocalParam)(param)).getVariableQName().Equals(withParam.getVariableQName()))) {
                        ok = true;
                        Saxon.Hej.value.SequenceType required = ((Saxon.Hej.style.XSLLocalParam)(param)).getRequiredType();
                        withParam.checkAgainstRequiredType(required);
                        break ;
                    }
                }
                if (!(ok)) {
                    compileError((("Parameter "+withParam.getVariableQName().getDisplayName())+" is not declared in the containing xsl:iterate instruction"), "XTSE3130");
                }
            }
        }
        public Saxon.Hej.value.SequenceType getDeclaredParamType(Saxon.Hej.om.StructuredQName name) {
            foreach (Saxon.Hej.om.NodeInfo param in xslIterate.children(Saxon.Hej.pattern.NodeSelector.of(x => (x is Saxon.Hej.style.XSLLocalParam)))) {
                if (((Saxon.Hej.style.XSLLocalParam)(param)).getVariableQName().Equals(name)) {
                    return ((Saxon.Hej.style.XSLLocalParam)(param)).getRequiredType();
                }
            }
            return null;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.NextIteration call = new Saxon.Hej.expr.instruct.NextIteration();
            call.setRetainedStaticContext(makeRetainedStaticContext());
            Saxon.Hej.expr.instruct.WithParam[] actualParams = getWithParamInstructions(call, exec, decl, false);
            call.setParameters(actualParams);
            if ((xslIterate != null)) {
                Saxon.Hej.tree.iter.AxisIterator declaredParams = xslIterate.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                Saxon.Hej.om.NodeInfo param;
                while (((param = ((/*1430*/Saxon.Hej.om.NodeInfo)declaredParams.next())) != null)) {
                    if ((param is Saxon.Hej.style.XSLLocalParam)) {
                        Saxon.Hej.style.XSLLocalParam pdecl = (Saxon.Hej.style.XSLLocalParam)(param);
                        Saxon.Hej.om.StructuredQName paramName = pdecl.getVariableQName();
                        Saxon.Hej.expr.instruct.LocalParam lp = pdecl.getCompiledParam();
                        bool found = false;
                        foreach (Saxon.Hej.expr.instruct.WithParam actualParam in actualParams) {
                            if (paramName.Equals(actualParam.getVariableQName())) {
                                actualParam.setSlotNumber(lp.getSlotNumber());
                                found = true;
                                break ;
                            }
                        }
                        if (!(found)) {
                            Saxon.Hej.expr.instruct.WithParam wp = new Saxon.Hej.expr.instruct.WithParam();
                            wp.setVariableQName(paramName);
                            Saxon.Hej.expr.VariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(lp);
                            wp.setSelectExpression(call, @ref);
                            wp.setSlotNumber(lp.getSlotNumber());
                            @ref.setStaticType(pdecl.getRequiredType(), null, 0);
                            Saxon.Hej.expr.instruct.WithParam[] p2 = new Saxon.Hej.expr.instruct.WithParam[(actualParams.Length+1)];
                            p2[0] = wp;
                            Saxon.Ejava.lang.System.arraycopy(actualParams, 0, p2, 1, actualParams.Length);
                            actualParams = p2;
                        }
                    }
                }
            }
            return call;
        }
    
    }
}
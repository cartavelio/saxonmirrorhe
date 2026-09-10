using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLNextMatch : Saxon.Hej.style.StyleElement {
        private bool useTailRecursion = false;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainFallback() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if (((child is Saxon.Hej.style.XSLWithParam) || (child is Saxon.Hej.style.XSLFallback))) {
                } else {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            compileError("No character data is allowed within xsl:next-match", "XTSE0010");
                        }
                    } else {
                        compileError((("Child element "+child.getDisplayName())+" is not allowed as a child of xsl:next-match"), "XTSE0010");
                    }
                }
            }
        }
        protected internal override bool markTailCalls() {
            useTailRecursion = true;
            return true;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.NextMatch inst = new Saxon.Hej.expr.instruct.NextMatch(useTailRecursion);
            inst.setLocation(allocateLocation());
            inst.setActualParams(getWithParamInstructions(inst, exec, decl, false));
            inst.setTunnelParams(getWithParamInstructions(inst, exec, decl, true));
            return inst;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLApplyImports : Saxon.Hej.style.StyleElement {
        public override bool isInstruction() {
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
                if ((child is Saxon.Hej.style.XSLWithParam)) {
                } else {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            compileError("No character data is allowed within xsl:apply-imports", "XTSE0010");
                        }
                    } else {
                        compileError((("Child element "+child.getDisplayName())+" is not allowed as a child of xsl:apply-imports"), "XTSE0010");
                    }
                }
            }
            Saxon.Hej.tree.linked.NodeImpl parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            while ((parent != null)) {
                if ((parent is Saxon.Hej.style.XSLOverride)) {
                    compileError("xsl:apply-imports cannot be used in a template rule declared within xsl:override", "XTSE3460");
                }
                parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)parent.getParent());
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.ApplyImports inst = new Saxon.Hej.expr.instruct.ApplyImports();
            Saxon.Hej.expr.instruct.WithParam[] nonTunnels = getWithParamInstructions(inst, exec, decl, false);
            Saxon.Hej.expr.instruct.WithParam[] tunnels = getWithParamInstructions(inst, exec, decl, true);
            inst.setActualParams(nonTunnels);
            inst.setTunnelParams(tunnels);
            return inst;
        }
    
    }
}
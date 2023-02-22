using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLFork : Saxon.Hej.style.StyleElement {
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            int foundGroup = 0;
            int foundSequence = 0;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLSequence)) {
                    foundSequence++;
                } else {
                    if ((child is Saxon.Hej.style.XSLForEachGroup)) {
                        foundGroup++;
                    } else {
                        if ((child is Saxon.Hej.style.XSLFallback)) {
                        } else {
                            compileError((child.getDisplayName()+" cannot appear as a child of xsl:fork"));
                        }
                    }
                }
            }
            if ((foundGroup>1)) {
                compileError("xsl:fork contains more than one xsl:for-each-group instruction");
            }
            if (((foundGroup>0) && (foundSequence>0))) {
                compileError("Cannot mix xsl:sequence and xsl:for-each-group within xsl:fork");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((content is Saxon.Hej.expr.instruct.Block)) {
                return new Saxon.Hej.expr.instruct.Fork(((Saxon.Hej.expr.instruct.Block)(content)).getOperanda());
            } else {
                return content;
            }
        }
    
    }
}
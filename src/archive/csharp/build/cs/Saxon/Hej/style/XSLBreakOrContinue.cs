using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLBreakOrContinue : Saxon.Hej.style.StyleElement {
        protected internal Saxon.Hej.style.XSLIterate xslIterate = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        protected internal void validatePosition() {
            Saxon.Hej.om.NodeInfo inst = this;
            bool isLast = true;
            while (true) {
                if (!(((inst is Saxon.Hej.style.XSLWhen)))) {
                    Saxon.Hej.tree.iter.AxisIterator sibs = inst.iterateAxis(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING);
                    while (true) {
                        Saxon.Hej.om.NodeInfo sib = ((/*1430*/Saxon.Hej.om.NodeInfo)sibs.next());
                        if ((sib == null)) {
                            break ;
                        }
                        if (((sib is Saxon.Hej.style.XSLFallback) || (sib is Saxon.Hej.style.XSLCatch))) {
                            continue ;
                        }
                        isLast = false;
                    }
                }
                inst = inst.getParent();
                if ((inst is Saxon.Hej.style.XSLIterate)) {
                    xslIterate = (Saxon.Hej.style.XSLIterate)(inst);
                    break ;
                } else {
                    if (((inst is Saxon.Hej.style.XSLTry) || (inst is Saxon.Hej.style.XSLCatch))) {
                    } else {
                        if (((((inst is Saxon.Hej.style.XSLWhen) || (inst is Saxon.Hej.style.XSLOtherwise)) || (inst is Saxon.Hej.style.XSLIf)) || (inst is Saxon.Hej.style.XSLChoose))) {
                        } else {
                            if ((inst == null)) {
                                compileError((getDisplayName()+" is not allowed at outermost level"), "XTSE3120");
                                return ;
                            } else {
                                compileError(((getDisplayName()+" is not allowed within ")+inst.getDisplayName()), "XTSE3120");
                                return ;
                            }
                        }
                    }
                }
            }
            if (!(isLast)) {
                compileError((getDisplayName()+" must be the last instruction in the xsl:iterate loop"), "XTSE3120");
            }
        }
    
    }
}
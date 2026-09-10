using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLChooseOrSwitch : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.style.StyleElement otherwise;
        private int numberOfWhens = 0;
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
            Saxon.Hej.style.XSLFallback fallback = null;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr is Saxon.Hej.style.XSLWhen)) {
                    if ((otherwise != null)) {
                        otherwise.compileError("xsl:otherwise must come last", "XTSE0010");
                    } else {
                        if ((fallback != null)) {
                            fallback.compileError("xsl:fallback must come last", "XTSE0010");
                        }
                    }
                    numberOfWhens++;
                } else {
                    if ((curr is Saxon.Hej.style.XSLOtherwise)) {
                        if ((otherwise != null)) {
                            ((Saxon.Hej.style.XSLOtherwise)(curr)).compileError(("Only one xsl:otherwise is allowed in an "+getDisplayName()), "XTSE0010");
                        } else {
                            if ((fallback != null)) {
                                fallback.compileError("xsl:fallback must come last", "XTSE0010");
                            } else {
                                otherwise = (Saxon.Hej.style.StyleElement)(curr);
                            }
                        }
                    } else {
                        if (((curr is Saxon.Hej.style.XSLFallback) && (this is Saxon.Hej.style.XSLSwitch))) {
                            fallback = (Saxon.Hej.style.XSLFallback)(curr);
                        } else {
                            if ((curr is Saxon.Hej.style.StyleElement)) {
                                ((Saxon.Hej.style.StyleElement)(curr)).compileError("Only xsl:when and xsl:otherwise are allowed here", "XTSE0010");
                            } else {
                                compileError(("Only xsl:when and xsl:otherwise are allowed within "+getDisplayName()), "XTSE0010");
                            }
                        }
                    }
                }
            }
            if ((numberOfWhens == 0)) {
                compileError((getDisplayName()+" must contain at least one xsl:when"), "XTSE0010");
            }
        }
        protected internal override bool markTailCalls() {
            bool found = false;
            foreach (Saxon.Hej.om.NodeInfo curr in children(x => (x is Saxon.Hej.style.StyleElement))) {
                found |= ((Saxon.Hej.style.StyleElement)(curr)).markTailCalls();
            }
            return found;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            int entries = (numberOfWhens+(((otherwise == null) ? 0 : 1)));
            Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[entries];
            Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[entries];
            compileActions(exec, decl, actions);
            compileConditions(exec, decl, conditions);
            Saxon.Hej.expr.instruct.Choose choose = new Saxon.Hej.expr.instruct.Choose(conditions, actions);
            choose.setInstruction(true);
            return choose;
        }
        protected internal abstract void compileConditions(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.Expression[] conditions);
        protected internal void compileActions(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.Expression[] actions) {
            int w = 0;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if (((curr is Saxon.Hej.style.XSLWhen) || (curr is Saxon.Hej.style.XSLOtherwise))) {
                    Saxon.Hej.expr.Expression b = ((Saxon.Hej.style.StyleElement)(curr)).compileSequenceConstructor(exec, decl, true);
                    if ((b == null)) {
                        b = Saxon.Hej.expr.Literal.makeEmptySequence();
                        b.setRetainedStaticContext(makeRetainedStaticContext());
                    }
                    try {
                        b = b.simplify();
                        actions[w] = b;
                    } catch (Saxon.Hej.trans.XPathException e) {
                        compileError(e);
                    }
                    setInstructionLocation((Saxon.Hej.style.StyleElement)(curr), actions[w]);
                    w++;
                }
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLScheduleAction : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression wait;
        private Saxon.Hej.expr.Expression document;
        private Saxon.Hej.expr.Expression httpRequest;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            int c = 0, d = 0;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("wait")) {
                    wait = makeExpression(att.getValue(), att);
                    c++;
                } else {
                    if (f.Equals("document")) {
                        document = makeAttributeValueTemplate(att.getValue(), att);
                        c++;
                        d++;
                    } else {
                        if (f.Equals("http-request")) {
                            httpRequest = makeExpression(att.getValue(), att);
                            c++;
                            d++;
                        } else {
                            checkUnknownAttribute(attName);
                        }
                    }
                }
            }
            if ((c == 0)) {
                compileError("At least one attribute should be set on ixsl:schedule-action", "XTSE0010");
            }
            if ((d == 2)) {
                compileError("Combination of attributes set on ixsl:schedule-action is not permitted", "XTSE0010");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            wait = typeCheck("wait", wait);
            document = typeCheck("document", document);
            httpRequest = typeCheck("http-request", httpRequest);
            if ((wait == null)) {
                wait = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.ZERO);
            }
            if ((document == null)) {
                document = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.tree.iter.AxisIterator kids = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
            Saxon.Hej.om.NodeInfo child1 = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
            if (((child1 != null) && !(((child1 is Saxon.Hej.style.XSLCallTemplate))))) {
                compileError("Only xsl:call-template is allowed within ixsl:schedule-action", "XTSE0010");
            }
            Saxon.Hej.om.NodeInfo child2 = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
            if ((child2 != null)) {
                compileError("Only one child permitted within ixsl:schedule-action", "XTSE0010");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, true);
            if ((action == null)) {
                action = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.Expression e;
            if ((httpRequest == null)) {
                e = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("schedule-action", 3).makeFunctionCall(wait, document, action);
            } else {
                e = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("schedule-action", 4).makeFunctionCall(wait, document, action, httpRequest);
            }
            e.setRetainedStaticContext(makeRetainedStaticContext());
            return e;
        }
    
    }
}
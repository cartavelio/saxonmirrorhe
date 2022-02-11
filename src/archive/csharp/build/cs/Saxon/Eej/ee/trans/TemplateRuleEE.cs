using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class TemplateRuleEE : Saxon.Hej.expr.instruct.TemplateRule {
        private bool declaredStreamable;
        private Saxon.Eej.ee.trans.TemplateRuleInitializer initializer = null;
        public TemplateRuleEE()  {
        }
        public override void setDeclaredStreamable(bool streamable) {
            this.declaredStreamable = streamable;
        }
        public override bool isDeclaredStreamable() {
            return declaredStreamable;
        }
        public void setInitializer(Saxon.Eej.ee.trans.TemplateRuleInitializer initializer) {
            this.initializer = initializer;
        }
        public Saxon.Eej.ee.trans.TemplateRuleInitializer getInitializer() {
            return initializer;
        }
        public override void prepareInitializer(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.om.StructuredQName modeName) {
            if ((getInitializer() == null)) {
                setInitializer(new Saxon.Eej.ee.trans.TemplateRuleInitializer(compilation, decl, modeName));
            }
        }
        public override void initialize() { lock(this) {
            System.Diagnostics.Debug.Assert(!((((body == null) && (initializer == null)))));
            if ((initializer != null)) {
                initializer.init();
                initializer = null;
            }
        }}
        public override Saxon.Hej.expr.instruct.TemplateRule copy() {
            Saxon.Hej.expr.instruct.TemplateRule tr = new Saxon.Eej.ee.trans.TemplateRuleEE();
            if (((body == null) || (matchPattern == null))) {
                slaveCopies.Add(tr);
            } else {
                copyTo(tr);
            }
            return tr;
        }
        public bool isActuallyStreamable(System.Collections.Generic.IList<string> reasons) {
            if ((matchPattern == null)) {
                if ((reasons != null)) {
                    reasons.Add("There is no match pattern");
                }
                return false;
            }
            if (!(matchPattern.isMotionless())) {
                if ((reasons != null)) {
                    reasons.Add("The match pattern is not motionless");
                }
                return false;
            }
            if ((body == null)) {
                if ((reasons != null)) {
                    reasons.Add("The template has no body");
                }
                return false;
            }
            Saxon.Eej.ee.trans.ContextItemStaticInfoEE info = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(body.getConfiguration().makeContextItemStaticInfo(matchPattern.getItemType(), false));
            if (matchPattern.getItemType().getUType().overlaps(Saxon.Hej.type.UType.ANY_NODE)) {
                info.setContextPostureStriding();
            } else {
                info.setContextPostureGrounded();
            }
            Saxon.Eej.ee.stream.PostureAndSweep s = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(body, info, reasons);
            if ((s.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED)) {
                return true;
            } else {
                reasons.Add("The result of the template rule can contain streamed nodes");
                return false;
            }
        }
        public void makeInversion() {
            if ((inversion == null)) {
                Saxon.Eej.ee.stream.ComponentInversion inv = new Saxon.Eej.ee.stream.ComponentInversion(getBody());
                inv.setStackFrame(getStackFrameMap());
                inv.invert(isActuallyStreamable(null));
                inversion = inv;
            }
        }
        private Saxon.Eej.ee.stream.ComponentInversion inversion;
        public Saxon.Eej.ee.stream.ComponentInversion getInversion() {
            if ((inversion == null)) {
                makeInversion();
            }
            return inversion;
        }
    
    }
}
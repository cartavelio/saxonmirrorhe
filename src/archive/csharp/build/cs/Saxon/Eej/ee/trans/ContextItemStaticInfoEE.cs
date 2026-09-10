using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class ContextItemStaticInfoEE : Saxon.Hej.expr.parser.ContextItemStaticInfo {
        private Saxon.Eej.ee.stream.Posture explicitPosture = Saxon.Eej.ee.stream.Posture.UNKNOWN;
        private bool strictStreamabilityRules;
        private Saxon.Hej.expr.instruct.Actor container;
        private Saxon.Hej.functions.AccumulatorFn.Phase accumulatorPhase = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
        public ContextItemStaticInfoEE(Saxon.Hej.type.ItemType itemType, bool maybeUndefined) : base (itemType, maybeUndefined) {
        }
        public ContextItemStaticInfoEE(Saxon.Hej.type.ItemType itemType, bool maybeUndefined, Saxon.Eej.ee.stream.Posture contextItemPosture) : base (itemType, maybeUndefined) {
            this.explicitPosture = contextItemPosture;
        }
        public override void setContextPostureStriding() {
            this.explicitPosture = Saxon.Eej.ee.stream.Posture.STRIDING;
        }
        public override void setContextPostureGrounded() {
            this.explicitPosture = Saxon.Eej.ee.stream.Posture.GROUNDED;
        }
        public Saxon.Hej.expr.instruct.Actor getContainer() {
            return container;
        }
        public void setContainer(Saxon.Hej.expr.instruct.Actor container) {
            this.container = container;
        }
        public Saxon.Eej.ee.stream.Posture getContextItemPosture() {
            if ((explicitPosture == Saxon.Eej.ee.stream.Posture.UNKNOWN)) {
                Saxon.Hej.expr.Expression contextSetter = getContextSettingExpression();
                if ((contextSetter != null)) {
                    return /*134*/Saxon.Eej.ee.stream.Streamability.getPosture(contextSetter);
                } else {
                    Saxon.Hej.expr.instruct.Actor actor = getContainer();
                    if ((actor is Saxon.Hej.expr.instruct.AttributeSet)) {
                        return (((Saxon.Hej.expr.instruct.AttributeSet)(actor)).isDeclaredStreamable() ? Saxon.Eej.ee.stream.Posture.STRIDING : Saxon.Eej.ee.stream.Posture.ROAMING);
                    } else {
                        return Saxon.Eej.ee.stream.Posture.ROAMING;
                    }
                }
            } else {
                return explicitPosture;
            }
        }
        public override bool isStrictStreamabilityRules() {
            return strictStreamabilityRules;
        }
        public void setStrictStreamabilityRules(bool strictStreamabilityRules) {
            this.strictStreamabilityRules = strictStreamabilityRules;
        }
        public void setAccumulatorPhase(Saxon.Hej.functions.AccumulatorFn.Phase phase) {
            this.accumulatorPhase = phase;
        }
        public Saxon.Hej.functions.AccumulatorFn.Phase getAccumulatorPhase() {
            return accumulatorPhase;
        }
        public static readonly new Saxon.Eej.ee.trans.ContextItemStaticInfoEE DEFAULT = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.type.AnyItemType.getInstance(), true, Saxon.Eej.ee.stream.Posture.STRIDING);
        public static readonly new Saxon.Eej.ee.trans.ContextItemStaticInfoEE ABSENT = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.type.ErrorType.getInstance(), true, Saxon.Eej.ee.stream.Posture.STRIDING);
    
    }
}
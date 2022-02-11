using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ApplyImportsAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            System.Collections.Generic.List<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(10);
            Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(getExpression(), cie);
            cie.setStaticInfo(new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.pattern.AnyNodeTest.getInstance(), false, Saxon.Eej.ee.stream.Posture.STRIDING));
            list.Add(new Saxon.Hej.expr.Operand(getExpression(), cie, new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.SINGLE_NODE)));
            foreach (Saxon.Hej.expr.Operand o in getExpression().operands()) {
                list.Add(o);
            }
            return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(getExpression(), list, contextInfo, reasons);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            return (watchManager, @out, context) => {
                Saxon.Hej.expr.Component.M actualMode = context.getCurrentMode();
                Saxon.Eej.ee.stream.watch.ApplyImportsAction actor = new Saxon.Eej.ee.stream.watch.ApplyImportsAction(getExpression(), @out, context);
                actor.setMode(actualMode);
                actor.setWatchManager(watchManager);
                return new Saxon.Eej.ee.stream.watch.Trigger(Saxon.Hej.pattern.AnchorPattern.getInstance(), actor, context);
            } ;
        }
    
    }
}
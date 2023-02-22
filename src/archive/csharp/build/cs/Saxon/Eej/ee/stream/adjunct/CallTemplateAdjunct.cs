using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class CallTemplateAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.PostureAndSweep computeStreamability(Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.CallTemplate inst = (Saxon.Hej.expr.instruct.CallTemplate)(getExpression());
            System.Collections.Generic.List<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(10);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(inst, inst.getActualParams(), list);
            Saxon.Hej.expr.instruct.WithParam.gatherOperands(inst, inst.getTunnelParams(), list);
            Saxon.Hej.expr.instruct.NamedTemplate t = (Saxon.Hej.expr.instruct.NamedTemplate)(inst.getTarget().getActor());
            if ((!(t.isAbsentFocus()) && !(t.getRequiredContextItemType().isPlainType()))) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(inst, cie);
                list.Add(new Saxon.Hej.expr.Operand(inst, cie, Saxon.Hej.expr.OperandRole.NAVIGATE));
            }
            return /*134*/Saxon.Eej.ee.stream.Streamability.generalStreamabilityRules(inst, list, contextInfo, reasons);
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Eval : Saxon.Eej.functions.extfn.Evaluate {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            Saxon.Hej.om.Item head = arguments[0].head();
            Saxon.Hej.value.ObjectValue<Saxon.Eej.functions.extfn.Evaluate.PreparedExpression> oval = (Saxon.Hej.value.ObjectValue<Saxon.Eej.functions.extfn.Evaluate.PreparedExpression>)(head);
            Saxon.Eej.functions.extfn.Evaluate.PreparedExpression pexpr = oval.getObject();
            c2.setOrigin(pexpr);
            c2.openStackFrame(pexpr.stackFrameMap);
            for (int i = 1; (i<arguments.Length); i++) {
                int slot = pexpr.variables[(i-1)].getLocalSlotNumber();
                c2.setLocalVariable(slot, Saxon.Hej.om.SequenceTool.toGroundedValue(arguments[i].iterate()));
            }
            c2.setCurrentIterator(context.getCurrentIterator());
            return Saxon.Hej.om.SequenceTool.toLazySequence(pexpr.expression.iterate(c2));
        }
    
    }
}
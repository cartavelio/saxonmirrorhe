using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public class LocalVariableEvaluator : Saxon.Hej.expr.elab.SequenceEvaluator {
        readonly int slot;
        public LocalVariableEvaluator(int slot)  {
            this.slot = slot;
        }
        public Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context) {
            return context.evaluateLocalVariable(slot).materialize();
        }
    
    }
}
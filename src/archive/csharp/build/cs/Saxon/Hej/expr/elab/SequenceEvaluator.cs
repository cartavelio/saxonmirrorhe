using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.elab {
    public interface SequenceEvaluator {
        Saxon.Hej.om.Sequence evaluate(Saxon.Hej.expr.XPathContext context);
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public interface IAccumulatorData {
        Saxon.Hej.expr.accum.Accumulator getAccumulator();
        Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent);
    
    }
}
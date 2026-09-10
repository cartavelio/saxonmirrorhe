using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class VirtualAccumulatorData : Saxon.Hej.expr.accum.IAccumulatorData {
        private readonly Saxon.Hej.expr.accum.IAccumulatorData realData;
        public VirtualAccumulatorData(Saxon.Hej.expr.accum.IAccumulatorData realData)  {
            this.realData = realData;
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
            return realData.getAccumulator();
        }
        public Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent) {
            Saxon.Hej.om.NodeInfo original = ((Saxon.Hej.tree.wrapper.VirtualCopy)(node)).getOriginalNode();
            return realData.getValue(original, postDescent);
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class FailedAccumulatorData : Saxon.Hej.expr.accum.IAccumulatorData {
        private readonly Saxon.Hej.expr.accum.Accumulator acc;
        private readonly Saxon.Hej.trans.XPathException error;
        public FailedAccumulatorData(Saxon.Hej.expr.accum.Accumulator acc, Saxon.Hej.trans.XPathException error)  {
            this.acc = acc;
            this.error = error;
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
            return acc;
        }
        public Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent) {
            throw error;
        }
    
    }
}
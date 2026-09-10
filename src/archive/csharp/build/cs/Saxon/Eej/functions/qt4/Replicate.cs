using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class Replicate : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.GroundedValue baseValue = arguments[0].materialize();
            Saxon.Hej.value.NumericValue n = (Saxon.Hej.value.NumericValue)(arguments[1].head());
            int count = (int)(n.longValue());
            if ((count<2)) {
                if ((count<0)) {
                    throw new Saxon.Hej.trans.XPathException("Replication count is negative", "FORG0099");
                } else {
                    if ((count == 0)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return baseValue;
                    }
                }
            } else {
                return new Saxon.Hej.om.LazySequence(new Saxon.Eej.functions.qt4.Replicate.RepeatingIterator(baseValue, count, context));
            }
        }
        private class RepeatingIterator : Saxon.Hej.expr.instruct.AbstractBlockIterator {
            private readonly Saxon.Hej.om.GroundedValue baseValue;
            public RepeatingIterator(Saxon.Hej.om.GroundedValue baseValue, int count, Saxon.Hej.expr.XPathContext context) : base (count, context) {
                this.baseValue = baseValue;
            }
            public override Saxon.Hej.om.SequenceIterator getNthChildIterator(int n) {
                return baseValue.iterate();
            }
        
        }
    
    }
}
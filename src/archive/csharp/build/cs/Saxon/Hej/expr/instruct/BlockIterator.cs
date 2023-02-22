using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class BlockIterator : Saxon.Hej.expr.instruct.AbstractBlockIterator {
        private readonly Saxon.Hej.expr.Operand[] operanda;
        public BlockIterator(Saxon.Hej.expr.Operand[] operanda, Saxon.Hej.expr.XPathContext context) : base (operanda.Length, context) {
            this.operanda = operanda;
        }
        public override Saxon.Hej.om.SequenceIterator getNthChildIterator(int n) {
            return operanda[n].getChildExpression().iterate(context);
        }
    
    }
}
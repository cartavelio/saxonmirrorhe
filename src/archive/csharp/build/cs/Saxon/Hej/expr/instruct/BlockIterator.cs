using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class BlockIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.expr.Operand[] operanda;
        private int currentChildExpr = 0;
        private Saxon.Hej.om.SequenceIterator currentIter;
        private readonly Saxon.Hej.expr.XPathContext context;
        private int position = 0;
        public BlockIterator(Saxon.Hej.expr.Operand[] operanda, Saxon.Hej.expr.XPathContext context)  {
            this.operanda = operanda;
            this.currentChildExpr = 0;
            this.context = context;
        }
        public Saxon.Hej.om.Item next() {
            if ((position<0)) {
                return null;
            }
            try {
                while (true) {
                    if ((currentIter == null)) {
                        currentIter = operanda[currentChildExpr++].getChildExpression().iterate(context);
                    }
                    Saxon.Hej.om.Item current = currentIter.next();
                    if ((current != null)) {
                        position++;
                        return current;
                    }
                    currentIter = null;
                    if ((currentChildExpr>=operanda.Length)) {
                        position = -1;
                        return null;
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public void close() {
            if ((currentIter != null)) {
                currentIter.close();
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class AbstractBlockIterator : Saxon.Hej.om.SequenceIterator {
        protected internal int size;
        protected internal int currentOperand;
        protected internal Saxon.Hej.om.SequenceIterator currentIter;
        protected internal Saxon.Hej.expr.XPathContext context;
        public AbstractBlockIterator()  {
        }
        public AbstractBlockIterator(int size, Saxon.Hej.expr.XPathContext context)  {
            this.size = size;
            this.context = context;
            this.currentOperand = 0;
        }
        public void init(int size, Saxon.Hej.expr.XPathContext context) {
            this.size = size;
            this.context = context;
            this.currentOperand = 0;
        }
        public Saxon.Hej.om.Item next() {
            if ((currentOperand<0)) {
                return null;
            }
            while (true) {
                if ((currentIter == null)) {
                    try {
                        currentIter = getNthChildIterator(currentOperand++);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                }
                Saxon.Hej.om.Item current = currentIter.next();
                if ((current != null)) {
                    return current;
                }
                currentIter = null;
                if ((currentOperand>=size)) {
                    currentOperand = -1;
                    return null;
                }
            }
        }
        public abstract Saxon.Hej.om.SequenceIterator getNthChildIterator(int n);
        public void close() {
            if ((currentIter != null)) {
                currentIter.close();
            }
            currentOperand = -1;
        }
    
    }
}
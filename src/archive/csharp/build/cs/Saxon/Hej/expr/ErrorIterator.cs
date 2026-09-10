using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ErrorIterator : Saxon.Hej.om.SequenceIterator {
        private readonly Saxon.Hej.trans.XPathException exception;
        public ErrorIterator(Saxon.Hej.trans.XPathException exception)  {
            this.exception = exception;
        }
        public Saxon.Hej.om.Item next() {
            throw new Saxon.Hej.trans.UncheckedXPathException(exception);
        }
    
        /* Expanding default method close from interface SequenceIterator */
        public virtual void close() {
        }

        /* Expanding default method discharge from interface SequenceIterator */
        public virtual void discharge() {
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class UncheckedXPathException : Saxon.Ejava.lang.RuntimeException {
        public UncheckedXPathException(Saxon.Hej.trans.XPathException cause) : base (cause) {
        }
        public UncheckedXPathException(string message) : base (new Saxon.Hej.trans.XPathException(message)) {
        }
        public UncheckedXPathException(string message, string errorCode) : base (new Saxon.Hej.trans.XPathException(message, errorCode)) {
        }
        public UncheckedXPathException(System.Exception cause) : base (new Saxon.Hej.trans.XPathException(cause)) {
        }
        public Saxon.Hej.trans.XPathException getXPathException() {
            return (Saxon.Hej.trans.XPathException)(InnerException);
        }
        public override  /* REPLACING public */ string getMessage() {
            return InnerException.Message;
        }
    
    }
}
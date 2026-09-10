using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class ComparisonException : System.InvalidCastException {
        internal Saxon.Hej.trans.XPathException reason;
        public ComparisonException(Saxon.Hej.trans.XPathException reason)  {
            this.reason = reason;
        }
        public string getMessage() {
            return reason.Message;
        }
        public Saxon.Hej.trans.XPathException getReason() {
            return reason;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class TerminationException : Saxon.Hej.trans.XPathException {
        public TerminationException(string message) : base (message) {
            setErrorCode("XTMM9000");
        }
    
    }
}
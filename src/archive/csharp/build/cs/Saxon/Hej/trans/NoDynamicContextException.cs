using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class NoDynamicContextException : Saxon.Hej.trans.XPathException {
        public NoDynamicContextException(string message) : base (("Dynamic context missing: "+message)) {
        }
    
    }
}
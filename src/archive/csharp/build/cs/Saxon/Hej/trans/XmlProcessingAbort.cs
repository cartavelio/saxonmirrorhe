using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class XmlProcessingAbort : Saxon.Ejava.lang.RuntimeException {
        public XmlProcessingAbort(string message) : base (message) {
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class QNameException : Saxon.Ejava.lang.Exception {
        internal string message;
        public QNameException(string message)  {
            this.message = message;
        }
        public override  /* REPLACING public */ string getMessage() {
            return message;
        }
    
    }
}
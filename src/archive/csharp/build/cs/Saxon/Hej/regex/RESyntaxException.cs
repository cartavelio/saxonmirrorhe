using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class RESyntaxException : Saxon.Ejava.lang.RuntimeException {
        public RESyntaxException(string s) : base (s) {
        }
        public RESyntaxException(string s, int offset) : base (((("Syntax error at char "+offset)+" in regular expression: ")+s)) {
        }
        public override  /* REPLACING public */ string getMessage() {
            return base.Message;
        }
    
    }
}
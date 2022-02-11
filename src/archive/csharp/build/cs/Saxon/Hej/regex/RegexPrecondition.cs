using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class RegexPrecondition {
        public Saxon.Hej.regex.Operation operation;
        public int fixedPosition;
        public int minPosition;
        public RegexPrecondition(Saxon.Hej.regex.Operation op, int fixedPos, int minPos)  {
            this.operation = op;
            this.fixedPosition = fixedPos;
            this.minPosition = minPos;
        }
    
    }
}
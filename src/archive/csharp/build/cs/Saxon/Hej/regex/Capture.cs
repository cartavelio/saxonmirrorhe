using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class Capture {
        public int groupNr;
        public int start;
        public int end;
        public Capture(int groupNr, int start, int end)  {
            this.groupNr = groupNr;
            this.start = start;
            this.end = end;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamableUserFunction : Saxon.Hej.expr.instruct.UserFunction {
        private Saxon.Eej.ee.stream.ComponentInversion inversion;
        public void makeInversion() {
            if ((inversion == null)) {
                Saxon.Eej.ee.stream.ComponentInversion inv = new Saxon.Eej.ee.stream.ComponentInversion(getBody());
                inv.setStackFrame(getStackFrameMap());
                inv.invert(true);
                inversion = inv;
            }
        }
        public Saxon.Eej.ee.stream.ComponentInversion getInversion() {
            return inversion;
        }
        public override void prepareForStreaming() {
            if (getDeclaredStreamability().isStreaming()) {
                /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(getBody(), Saxon.Eej.ee.trans.ContextItemStaticInfoEE.ABSENT, new System.Collections.Generic.List<string>());
                makeInversion();
            }
        }
    
    }
}
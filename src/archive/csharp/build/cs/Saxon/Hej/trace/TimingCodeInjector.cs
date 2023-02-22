using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class TimingCodeInjector : Saxon.Hej.trace.TraceCodeInjector {
        public override void process(Saxon.Hej.trace.TraceableComponent component) {
            Saxon.Hej.expr.instruct.ComponentTracer trace = new Saxon.Hej.expr.instruct.ComponentTracer(component);
            component.setBody(trace);
        }
    
    }
}
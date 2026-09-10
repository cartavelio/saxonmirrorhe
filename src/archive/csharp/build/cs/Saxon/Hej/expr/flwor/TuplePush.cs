using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public abstract class TuplePush {
        private readonly Saxon.Hej.@event.Outputter outputter;
        protected internal TuplePush(Saxon.Hej.@event.Outputter outputter)  {
            this.outputter = outputter;
        }
        protected internal Saxon.Hej.@event.Outputter getOutputter() {
            return outputter;
        }
        public abstract void processTuple(Saxon.Hej.expr.XPathContext context);
        public virtual void close() {
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public abstract class ExtensionFunctionCall : Saxon.Hej.expr.Callable {
        internal Saxon.Hej.lib.ExtensionFunctionDefinition definition;
        public void setDefinition(Saxon.Hej.lib.ExtensionFunctionDefinition definition) {
            this.definition = definition;
        }
        public Saxon.Hej.lib.ExtensionFunctionDefinition getDefinition() {
            return definition;
        }
        public virtual  /* REPLACING public */ void supplyStaticContext(Saxon.Hej.expr.StaticContext context, int locationId, Saxon.Hej.expr.Expression[] arguments) {
        }
        public virtual  /* REPLACING public */ Saxon.Hej.expr.Expression rewrite(Saxon.Hej.expr.StaticContext context, Saxon.Hej.expr.Expression[] arguments) {
            return null;
        }
        public virtual  /* REPLACING public */ void copyLocalData(Saxon.Hej.lib.ExtensionFunctionCall destination) {
        }
        public abstract Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments);
        public virtual  /* REPLACING public virtual */ bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(call(context, arguments).iterate());
        }
        public virtual  /* REPLACING public */ System.Object getStreamingImplementation() {
            return null;
        }
    
    }
}
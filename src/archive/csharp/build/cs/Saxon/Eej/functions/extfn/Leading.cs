using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Leading : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContext c2 = context.newCleanContext();
            Saxon.Hej.om.Function function = ((arguments.Length == 2) ? (Saxon.Hej.om.Function)(arguments[1].head()) : context.getConfiguration().makeSystemFunction("data", 1));
            return Saxon.Hej.om.SequenceTool.toLazySequence(new Saxon.Eej.functions.extfn.Leading.LeadingIterator(arguments[0].iterate(), function, c2));
        }
        private class LeadingIterator : Saxon.Hej.om.SequenceIterator {
            private readonly Saxon.Hej.om.SequenceIterator @base;
            private readonly Saxon.Hej.om.Function function;
            private readonly Saxon.Hej.expr.XPathContext context;
            private bool exhausted = false;
            private readonly Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[1];
            public LeadingIterator(Saxon.Hej.om.SequenceIterator @base, Saxon.Hej.om.Function function, Saxon.Hej.expr.XPathContext context)  {
                this.@base = @base;
                this.function = function;
                this.context = context;
            }
            public Saxon.Hej.om.Item next() {
                if (exhausted) {
                    return null;
                }
                Saxon.Hej.om.Item next = @base.next();
                if ((next == null)) {
                    return null;
                }
                args[0] = next;
                try {
                    bool matches = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(dynamicCall(function, context, args).iterate());
                    if (matches) {
                        return next;
                    } else {
                        exhausted = true;
                        @base.close();
                        return null;
                    }
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            }
            public void close() {
                @base.close();
            }
        
        }
    
    }
}
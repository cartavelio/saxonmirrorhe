using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class CallableFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        internal Saxon.Hej.expr.Callable function;
        internal Saxon.Hej.expr.Expression[] arguments;
        internal bool done = false;
        public CallableFeed(Saxon.Hej.expr.Callable function, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, Saxon.Hej.expr.Expression[] arguments) : base (result, context) {
            this.function = function;
            this.arguments = arguments;
        }
        public override void append(Saxon.Hej.om.Item item) {
            if (!(hasFailed())) {
                if (done) {
                    string msg = "A sequence of more than one item must not be supplied as the streaming argument";
                    if ((function is Saxon.Hej.om.FunctionItem)) {
                        Saxon.Hej.om.StructuredQName fName = ((Saxon.Hej.om.FunctionItem)(function)).getFunctionName();
                        if ((fName != null)) {
                            msg += (" of function "+fName.getDisplayName());
                        }
                    }
                    Saxon.Hej.s9api.Location loc = ((getExpression() == null) ? Saxon.Hej.expr.parser.Loc.NONE : getExpression().getLocation());
                    throw new Saxon.Hej.trans.XPathException(msg, "XPTY0004", loc);
                }
                doCall(item);
                done = true;
            }
        }
        public override void close() {
            if (!(hasFailed())) {
                if (!(done)) {
                    doCall(null);
                }
                base.close();
            }
        }
        private void doCall(Saxon.Hej.om.Item item) {
            Saxon.Hej.om.Sequence[] args = Saxon.Hej.om.SequenceTool.makeSequenceArray(arguments.Length);
            int i = 0;
            foreach (Saxon.Hej.expr.Expression exp in arguments) {
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(exp) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    args[i++] = ((item == null) ? Saxon.Hej.value.EmptySequence.getInstance() : item);
                } else {
                    args[i++] = new Saxon.Hej.om.LazySequence(exp.iterate(getContext()));
                }
            }
            Saxon.Hej.om.Sequence result;
            try {
                result = function.call(getContext(), args);
            } catch (Saxon.Hej.trans.XPathException e) {
                dynamicError(e);
                return ;
            }
            processItems(result.iterate(), getNextOutputter());
        }
    
    }
}
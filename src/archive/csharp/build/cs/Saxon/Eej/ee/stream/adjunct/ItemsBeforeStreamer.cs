using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ItemsBeforeStreamer : Saxon.Eej.ee.stream.adjunct.FilterFnStreamer {
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.om.FunctionItem function = (Saxon.Hej.om.FunctionItem)(((Saxon.Hej.expr.SystemFunctionCall)(getExpression())).getArg(1).evaluateItem(context));
            return new Inner_Filter_4(context, function);
        }
    
        private class Inner_Filter_4 : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            public Inner_Filter_4(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.FunctionItem function) {
                this.context = context;
                this.function = function;
                found = false;
            }
            private Saxon.Hej.expr.XPathContext context;
            private Saxon.Hej.om.FunctionItem function;
            internal bool found;
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                if (/*field*/found) {
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE;
                }
                bool match = ((Saxon.Hej.value.BooleanValue)(/*extraParam*/function.call(/*extraParam*/context, new Saxon.Hej.om.Sequence[]{/*localparam*/item}).head())).getBooleanValue();
                if (/*localvar*/match) {
                    /*field*/found = true;
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE;
                }
                return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
            }
        }

    }
}
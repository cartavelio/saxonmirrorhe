using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class ItemsAfterStreamer : Saxon.Eej.ee.stream.adjunct.FilterFnStreamer {
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.om.Function function = (Saxon.Hej.om.Function)(((Saxon.Hej.expr.SystemFunctionCall)(getExpression())).getArg(1).evaluateItem(context));
            return new Inner_Filter_d252e128(context, function);
        }
    
        private class Inner_Filter_d252e128 : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            public Inner_Filter_d252e128(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Function function) {
                this.context = context;
                this.function = function;
                started = false;
            }
            private Saxon.Hej.expr.XPathContext context;
            private Saxon.Hej.om.Function function;
            internal bool started;
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                if (/*field*/started) {
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
                } else {
                    bool match = ((Saxon.Hej.value.BooleanValue)(/*extraParam*/function.call(/*extraParam*/context, new Saxon.Hej.om.Sequence[]{/*localparam*/item}).head())).getBooleanValue();
                    if (/*localvar*/match) {
                        /*field*/started = true;
                    }
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP;
                }
            }
        }

    }
}
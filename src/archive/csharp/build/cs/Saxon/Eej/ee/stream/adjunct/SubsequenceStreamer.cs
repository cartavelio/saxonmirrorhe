using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SubsequenceStreamer : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Hej.functions.SystemFunction fn = fnc.getTargetFunction();
            int first, last;
            Saxon.Hej.value.NumericValue startD = (Saxon.Hej.value.NumericValue)(fnc.getArg(1).evaluateItem(context));
            first = (int)(startD.round(0).longValue());
            if ((fn.getArity() == 3)) {
                Saxon.Hej.value.NumericValue lenD = (Saxon.Hej.value.NumericValue)(fnc.getArg(2).evaluateItem(context));
                int len = (int)(lenD.round(0).longValue());
                last = ((first+len)-1);
            } else {
                last = System.Int32.MaxValue;
            }
            return new Saxon.Eej.ee.stream.adjunct.SubsequenceStreamer.SubsequenceFilter(first, last);
        }
        public class SubsequenceFilter : Saxon.Eej.ee.stream.feed.FilteringFeed.Filter {
            private readonly int first;
            private readonly int last;
            public SubsequenceFilter(int first, int last)  {
                this.first = first;
                this.last = last;
            }
            public Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult matches(Saxon.Hej.om.Item item, int position) {
                if ((position<first)) {
                    return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP;
                } else {
                    if ((position<=last)) {
                        return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
                    } else {
                        return /*explicit enum*/ Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.DONE;
                    }
                }
            }
        
        }
    
    }
}
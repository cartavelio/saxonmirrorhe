using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class DistinctValuesStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.DistinctValuesStreamer.DistinctValuesFeed(getExpression(), @out, context);
        }
        private class DistinctValuesFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.lib.StringCollator collator;
            private System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey> lookup;
            private readonly int implicitTimezone;
            internal DistinctValuesFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
                implicitTimezone = context.getImplicitTimezone();
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(expr);
                Saxon.Hej.functions.SystemFunction fn = call.getTargetFunction();
                if ((fn is Saxon.Hej.functions.DistinctValues)) {
                    collator = ((Saxon.Hej.functions.DistinctValues)(fn)).getStringCollator();
                }
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                lookup = new System.Collections.Generic.HashSet<Saxon.Hej.expr.sort.AtomicMatchKey>(40);
                if ((collator == null)) {
                    Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                    Saxon.Hej.expr.Expression collationArg = call.getArg((call.getArity()-1));
                    string collationName = collationArg.evaluateAsString(getContext()).ToString();
                    collator = getContext().getConfiguration().getCollation(collationName);
                    if ((collator == null)) {
                        dynamicError(new Saxon.Hej.trans.XPathException(("Unknown collation "+collationName), "FOCH0002"));
                    }
                }
                getResultFeed().open(terminator);
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(hasFailed())) {
                    Saxon.Hej.value.AtomicValue next = (Saxon.Hej.value.AtomicValue)(item);
                    Saxon.Hej.expr.sort.AtomicMatchKey key = (next.isNaN() ? Saxon.Hej.functions.DistinctValues.NaN_MATCH_KEY : next.getXPathMatchKey(collator, implicitTimezone));
                    if (lookup.Add(key)) {
                        getNextOutputter().append(item);
                    }
                }
            }
        
        }
    
    }
}
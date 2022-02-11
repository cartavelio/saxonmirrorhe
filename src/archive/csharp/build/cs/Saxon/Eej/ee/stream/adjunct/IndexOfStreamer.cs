using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class IndexOfStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.IndexOfStreamer.IndexOfFeed(getExpression(), @out, context);
        }
        private class IndexOfFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            private Saxon.Hej.value.AtomicValue search;
            private Saxon.Hej.expr.sort.AtomicComparer comparer;
            private int count;
            internal IndexOfFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            }
            public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
                base.open(terminator);
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.SystemFunction fn = call.getTargetFunction();
                if ((fn is Saxon.Hej.functions.IndexOf)) {
                    comparer = ((Saxon.Hej.functions.IndexOf)(fn)).getAtomicComparer(getContext());
                } else {
                    Saxon.Hej.expr.Expression collationArg = call.getArg((call.getArity()-1));
                    string collationName = collationArg.evaluateAsString(getContext()).ToString();
                    Saxon.Hej.lib.StringCollator collator = getContext().getConfiguration().getCollation(collationName);
                    if ((collator == null)) {
                        dynamicError(new Saxon.Hej.trans.XPathException(("Unknown collation "+collationName), "FOCH0002"));
                    }
                    comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(collator, getContext());
                }
                Saxon.Hej.expr.Expression searchArg = call.getArg(1);
                search = (Saxon.Hej.value.AtomicValue)(searchArg.evaluateItem(getContext()));
                count = 0;
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(hasFailed())) {
                    count++;
                    if (comparer.comparesEqual((Saxon.Hej.value.AtomicValue)(item), search)) {
                        getNextOutputter().append(new Saxon.Hej.value.Int64Value(count));
                    }
                }
            }
        
        }
    
    }
}
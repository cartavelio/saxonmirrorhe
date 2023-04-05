using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class DeepEqualStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        private int consumingArg = -1;
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            return base.getWatchMaker(forGrouping);
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            consumingArg = findConsumingArg(getExpression(), consumingArg);
            if ((consumingArg>=2)) {
                return getDefaultFeedMaker(consumingArg).makeItemFeed(watchManager, @out, context);
            }
            Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            Saxon.Eej.ee.stream.Posture posture = /*134*/Saxon.Eej.ee.stream.Streamability.getPosture(call.getArg(consumingArg));
            if (((posture == Saxon.Eej.ee.stream.Posture.GROUNDED) || (posture == Saxon.Eej.ee.stream.Posture.CLIMBING))) {
                return new Saxon.Eej.ee.stream.adjunct.DeepEqualStreamer.GroundedDeepEqualFeed(watchManager, call, @out, context, consumingArg);
            } else {
                Saxon.Eej.ee.stream.feed.ItemFeed comparisonFeed = new Saxon.Eej.ee.stream.adjunct.DeepEqualStreamer.GroundedDeepEqualFeed(watchManager, call, @out, context, consumingArg);
                return new Saxon.Eej.ee.stream.feed.CopyOfFeed(watchManager, comparisonFeed, context);
            }
        }
        private class GroundedDeepEqualFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
            internal Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            internal int consumingArg;
            internal Saxon.Hej.om.SequenceIterator other;
            internal Saxon.Hej.expr.sort.AtomicComparer comparer;
            internal bool done;
            internal bool closed;
            internal Saxon.Hej.functions.DeepEqual40.DeepEqualOptions options;
            public GroundedDeepEqualFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.SystemFunctionCall exp, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, int consumingArg) : base (exp, result, context) {
                this.watchManager = watchManager;
                other = exp.getArg((1-consumingArg)).iterate(context);
                this.consumingArg = consumingArg;
                Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression collationArg = exp.getArg(2);
                Saxon.Hej.om.Item collationVal = collationArg.evaluateItem(context);
                string collationName = ((collationVal == null) ? call.getRetainedStaticContext().getDefaultCollationName() : collationVal.getStringValue());
                Saxon.Hej.expr.Expression optionsArg = exp.getArg(3);
                Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(optionsArg.evaluateItem(context));
                try {
                    options = new Saxon.Hej.functions.DeepEqual40.DeepEqualOptions(suppliedOptions, collationName, context);
                } catch (Saxon.Hej.trans.XPathException e) {
                    dynamicError(e);
                }
            }
            public override void append(Saxon.Hej.om.Item item) {
                if ((!(done) && !(hasFailed()))) {
                    Saxon.Hej.om.Item otherItem = other.next();
                    try {
                        if (((otherItem == null) || !(Saxon.Hej.functions.DeepEqual40.deepEqual(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item), Saxon.Hej.tree.iter.SingletonIterator.makeIterator(otherItem), getContext(), options)))) {
                            getNextOutputter().append(Saxon.Hej.value.BooleanValue.FALSE);
                            done = true;
                            if (watchManager.allowsEarlyExit()) {
                                closed = true;
                                getNextOutputter().close();
                                getTerminator().terminate();
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        dynamicError(e);
                    }
                }
            }
            public override void close() {
                if (!(done)) {
                    bool more = (other.next() != null);
                    getNextOutputter().append(Saxon.Hej.value.BooleanValue.get(!(more)));
                }
                if (!(closed)) {
                    getNextOutputter().close();
                }
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public abstract class InspectionAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Operand consumer = null;
            foreach (Saxon.Hej.expr.Operand o in getExpression().operands()) {
                if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(o.getChildExpression()) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                    consumer = o;
                    break ;
                }
            }
            if ((consumer == null)) {
                throw new Saxon.Hej.trans.XPathException("No consuming operand found");
            }
            Saxon.Hej.expr.Expression select = consumer.getChildExpression();
            if (/*134*/Saxon.Eej.ee.stream.Streamability.isIncrementalPosture(/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select))) {
                return getInspectionWatchMaker(getExpression(), select, forGrouping);
            } else {
                return null;
            }
        }
        protected internal Saxon.Eej.ee.stream.watch.WatchMaker getInspectionWatchMaker(Saxon.Hej.expr.Expression parent, Saxon.Hej.expr.Expression select, bool forGrouping) {
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.Streamability.toStreamingPattern(select, getConfiguration());
            Saxon.Eej.ee.stream.feed.FeedMaker feedMaker = getFeedMaker(0);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.feed.ItemFeed feed = feedMaker.makeItemFeed(watchManager, @out, context);
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, feed, context);
                } ;
            } else {
                Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunct = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
                Saxon.Eej.ee.stream.watch.WatchMaker selectWM = adjunct.getWatchMaker(forGrouping);
                if ((selectWM == null)) {
                    throw new Saxon.Hej.trans.XPathException(("No watch found for "+select.toShortString()));
                }
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.feed.ItemFeed feed = feedMaker.makeItemFeed(watchManager, @out, context);
                    return /*Delegate.makeWatch()*/selectWM(watchManager, feed, context);
                } ;
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class EmptyTextNodeRemoverAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.EmptyTextNodeRemover scc = (Saxon.Hej.expr.EmptyTextNodeRemover)(getExpression());
            if ((/*134*/Saxon.Eej.ee.stream.Streamability.getSweep(scc) == Saxon.Eej.ee.stream.Sweep.CONSUMING)) {
                Saxon.Hej.expr.Expression select = scc.getBaseExpression();
                Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
                if ((selection != null)) {
                    return (watchManager, @out, context) => {
                        Saxon.Eej.ee.stream.feed.SimpleContentFeed action = new Saxon.Eej.ee.stream.feed.SimpleContentFeed(@out);
                        Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                        watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                        return watch;
                    } ;
                }
                return null;
            } else {
                return null;
            }
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.FilteringFeed(@out, context, new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => (((!(Saxon.Hej.expr.AdjacentTextNodeMerger.isTextNode(item)) || !(item.getUnicodeStringValue().isEmpty()))) ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP)));
        }
    
    }
}
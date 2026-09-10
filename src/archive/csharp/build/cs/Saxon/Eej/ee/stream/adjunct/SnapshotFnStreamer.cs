using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SnapshotFnStreamer : Saxon.Eej.ee.stream.adjunct.ComposingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeComposingAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.SnapshotFeed(watchManager, @out, context);
        }
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.SystemFunctionCall)(getExpression())).getArg(0);
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.feed.SnapshotFeed actor = new Saxon.Eej.ee.stream.feed.SnapshotFeed(watchManager, @out, context);
                    return new Saxon.Eej.ee.stream.watch.Trigger(selection, actor, context);
                } ;
            }
            return null;
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.SnapshotFeed(watchManager, @out, context);
        }
    
    }
}
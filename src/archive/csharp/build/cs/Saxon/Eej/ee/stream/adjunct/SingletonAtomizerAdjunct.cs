using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class SingletonAtomizerAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Expression select = ((Saxon.Hej.expr.SingletonAtomizer)(getExpression())).getBaseExpression();
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Eej.ee.stream.feed.TypedValueFeed action = new Saxon.Eej.ee.stream.feed.TypedValueFeed(@out, context);
                    Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                    watch.setPipelineConfiguration(watchManager.getPipelineConfiguration());
                    return watch;
                } ;
            }
            return null;
        }
    
    }
}
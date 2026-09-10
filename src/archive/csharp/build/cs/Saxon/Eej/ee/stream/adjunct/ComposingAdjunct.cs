using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public abstract class ComposingAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct {
        public abstract Saxon.Eej.ee.stream.feed.ItemFeed makeComposingAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context);
        public override Saxon.Eej.ee.stream.watch.WatchMaker getWatchMaker(bool forGrouping) {
            Saxon.Hej.expr.Operand consumingChild = /*134*/Saxon.Eej.ee.stream.Streamability.getConsumingOperand(getExpression());
            Saxon.Hej.expr.Expression select = ((consumingChild == null) ? null : consumingChild.getChildExpression());
            if ((select == null)) {
                return null;
            }
            Saxon.Hej.pattern.Pattern selection = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(select, getConfiguration(), null);
            if ((selection != null)) {
                return (watchManager, @out, context) => {
                    Saxon.Hej.@event.PipelineConfiguration pipe = watchManager.getPipelineConfiguration();
                    Saxon.Eej.ee.stream.feed.ItemFeed action = makeComposingAction(watchManager, @out, context);
                    Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
                    watch.setPipelineConfiguration(pipe);
                    return watch;
                } ;
            }
            if (/*134*/Saxon.Eej.ee.stream.Streamability.isIncrementalPosture(/*134*/Saxon.Eej.ee.stream.Streamability.getPosture(select))) {
                Saxon.Eej.ee.stream.adjunct.StreamingAdjunct adjunct = /*134*/Saxon.Eej.ee.stream.adjunct.StreamingAdjunct.makeStreamingAdjunct(getConfiguration(), select);
                if ((adjunct is Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)) {
                    return (watchManager, @out, context) => {
                        Saxon.Eej.ee.stream.feed.ItemFeed action = makeComposingAction(watchManager, @out, context);
                        return ((Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct)(adjunct)).makeTransmissionFlow(watchManager, select, action, context);
                    } ;
                }
            }
            return null;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class FoldStreamer : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public override Saxon.Eej.ee.stream.feed.FeedMaker getFeedMaker(int arg) {
            return ((arg == 0) ? this : getDefaultFeedMaker(arg));
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.SystemFunctionCall call = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
            return new Saxon.Eej.ee.stream.adjunct.FoldStreamer.FoldFeed(call, @out, context);
        }
        private class FoldFeed : Saxon.Eej.ee.stream.feed.AtomicItemFeed {
            private readonly Saxon.Hej.functions.Fold fold;
            public FoldFeed(Saxon.Hej.expr.SystemFunctionCall expression, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expression, result, context) {
                Saxon.Hej.om.Sequence[] additionalArgs = Saxon.Hej.om.SequenceTool.makeSequenceArray((expression.getArity()-1));
                for (int i = 1; (i<expression.getArity()); i++) {
                    additionalArgs[(i-1)] = Saxon.Hej.om.SequenceTool.toLazySequence(expression.getArg(i).iterate(context));
                }
                Saxon.Hej.functions.FoldingFunction ff = (Saxon.Hej.functions.FoldingFunction)(expression.getTargetFunction());
                fold = ff.getFold(context, additionalArgs);
            }
            public override void append(Saxon.Hej.om.Item item) {
                if (!(hasFailed())) {
                    try {
                        fold.processItem(item);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        dynamicError(err);
                    }
                    if (fold.isFinished()) {
                        close();
                        getTerminator().terminate();
                    }
                }
            }
            public override void close() {
                if (!(hasFailed())) {
                    Saxon.Hej.om.Sequence value = fold.result();
                    Saxon.Hej.@event.Outputter @out = getNextOutputter();
                    processItems(value.iterate(), @out);
                    @out.close();
                }
            }
        
        }
    
    }
}
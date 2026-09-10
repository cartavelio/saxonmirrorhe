using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class NextIterationFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        private readonly int streamedArgument;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
        internal Saxon.Hej.om.Sequence[] newValues;
        public NextIterationFeed(Saxon.Hej.expr.Expression inst, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context, int arg) : base (inst, result, context) {
            Saxon.Hej.expr.instruct.WithParam[] actualParams = ((Saxon.Hej.expr.instruct.NextIteration)(getExpression())).getParameters();
            newValues = Saxon.Hej.om.SequenceTool.makeSequenceArray(actualParams.Length);
            streamedArgument = arg;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            getResultFeed().open(terminator);
            Saxon.Hej.expr.instruct.WithParam[] actualParams = ((Saxon.Hej.expr.instruct.NextIteration)(getExpression())).getParameters();
            Saxon.Hej.expr.XPathContext context = getContext();
            if ((actualParams.Length>1)) {
                for (int i = 0; (i<actualParams.Length); i++) {
                    if ((i != streamedArgument)) {
                        Saxon.Hej.expr.instruct.WithParam wp = actualParams[i];
                        newValues[i] = wp.getSelectValue(context);
                    }
                }
            }
        }
        public override void append(Saxon.Hej.om.Item item) {
            inputValue.Add(item);
        }
        public override void close() {
            Saxon.Hej.expr.instruct.WithParam[] actualParams = ((Saxon.Hej.expr.instruct.NextIteration)(getExpression())).getParameters();
            Saxon.Hej.expr.instruct.WithParam wp = actualParams[streamedArgument];
            Saxon.Hej.expr.XPathContext context = getContext();
            context.setLocalVariable(wp.getSlotNumber(), Saxon.Hej.value.SequenceExtent.makeSequenceExtent(inputValue));
            for (int i = 0; (i<actualParams.Length); i++) {
                if ((i != streamedArgument)) {
                    wp = actualParams[i];
                    context.setLocalVariable(wp.getSlotNumber(), newValues[i]);
                }
            }
            getNextOutputter().close();
        }
    
    }
}
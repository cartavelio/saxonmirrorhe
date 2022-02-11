using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class BufferingFilterExpressionFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        internal Saxon.Hej.expr.Expression filter;
        internal Saxon.Hej.expr.XPathContext localContext;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
        public BufferingFilterExpressionFeed(Saxon.Hej.expr.Expression expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            filter = ((Saxon.Hej.expr.FilterExpression)(expr)).getActionExpression();
            localContext = context;
        }
        public override void append(Saxon.Hej.om.Item item) {
            inputValue.Add(item);
        }
        public override void close() {
            Saxon.Hej.om.Sequence extent = Saxon.Hej.value.SequenceExtent.makeSequenceExtent(inputValue);
            Saxon.Hej.expr.XPathContext c2 = localContext.newMinorContext();
            Saxon.Hej.om.FocusIterator iter = c2.trackFocus(extent.iterate());
            Saxon.Hej.om.SequenceIterator filterIter = new Saxon.Hej.expr.FilterIterator(iter, filter, c2);
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            processItems(filterIter, @out);
            @out.close();
        }
    
    }
}
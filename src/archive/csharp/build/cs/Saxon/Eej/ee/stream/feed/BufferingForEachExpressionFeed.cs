using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class BufferingForEachExpressionFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        internal Saxon.Hej.expr.Expression action;
        internal Saxon.Hej.expr.XPathContext localContext;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Item> inputValue = new System.Collections.Generic.List<Saxon.Hej.om.Item>(4);
        public BufferingForEachExpressionFeed(Saxon.Hej.expr.instruct.ForEach expr, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (expr, result, context) {
            action = expr.getActionExpression();
            localContext = context;
        }
        public override void append(Saxon.Hej.om.Item item) {
            inputValue.Add(item);
        }
        public override void close() {
            Saxon.Hej.expr.XPathContext c2 = localContext.newMinorContext();
            c2.trackFocus(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(inputValue));
            Saxon.Hej.om.SequenceIterator mappingIter = new Saxon.Hej.expr.ContextMappingIterator(c1 => action.iterate(c1), c2);
            Saxon.Hej.@event.Outputter @out = getNextOutputter();
            processItems(mappingIter, @out);
            @out.close();
        }
    
    }
}
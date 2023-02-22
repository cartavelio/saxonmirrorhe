using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class TailExpressionAdjunct : Saxon.Eej.ee.stream.adjunct.FilteringAdjunct {
        public override Saxon.Eej.ee.stream.feed.FilteringFeed.Filter makeFilter(Saxon.Hej.expr.XPathContext context, Saxon.Eej.ee.stream.feed.ItemFeed @out) {
            int start = ((Saxon.Hej.expr.TailExpression)(getExpression())).getStart();
            return new Saxon.Eej.ee.stream.feed.FilteringFeed.FilterLambda((item, position) => ((position>=start) ? Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.MATCHES : Saxon.Eej.ee.stream.feed.FilteringFeed.FilterResult.SKIP));
        }
    
    }
}
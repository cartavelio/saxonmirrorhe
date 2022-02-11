using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class AtomicSequenceConverterAdjunct : Saxon.Eej.ee.stream.adjunct.StreamingAdjunct, Saxon.Eej.ee.stream.feed.FeedMaker {
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.AtomicSequenceConverter.AtomicSequenceMappingFunction mapper = new Saxon.Hej.expr.AtomicSequenceConverter.AtomicSequenceMappingFunction();
            mapper.setConverter(((Saxon.Hej.expr.AtomicSequenceConverter)(getExpression())).getConverter());
            return new Saxon.Eej.ee.stream.feed.ItemMappingFeed(getExpression(), @out, context, mapper);
        }
    
    }
}
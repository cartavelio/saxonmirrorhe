using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class WherePopulatedAdjunct : Saxon.Eej.ee.stream.adjunct.TransmissionAdjunct {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.adjunct.WherePopulatedAdjunct.WherePopulatedFeed(@out, context);
        }
        private class WherePopulatedFeed : Saxon.Eej.ee.stream.feed.DelegatingFeed {
            internal WherePopulatedFeed(Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) : base (new Saxon.Hej.@event.WherePopulatedOutputter(@out), context) {
            }
        
        }
    
    }
}
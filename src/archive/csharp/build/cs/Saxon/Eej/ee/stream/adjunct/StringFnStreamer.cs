using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.adjunct {
    public class StringFnStreamer : Saxon.Eej.ee.stream.adjunct.ComposingAdjunct {
        public override Saxon.Eej.ee.stream.feed.ItemFeed makeComposingAction(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.StringValueFeed(@out, context);
        }
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Eej.ee.stream.feed.StringValueFeed(@out, context);
        }
    
    }
}
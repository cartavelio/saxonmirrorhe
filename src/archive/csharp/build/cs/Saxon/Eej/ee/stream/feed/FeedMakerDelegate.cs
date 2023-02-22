using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class FeedMakerDelegate : Saxon.Eej.ee.stream.feed.FeedMaker {
        private Saxon.Eej.ee.stream.feed.FeedMakerDelegate.Lambda @delegate;
        public FeedMakerDelegate(Saxon.Eej.ee.stream.feed.FeedMakerDelegate.Lambda @delegate)  {
            this.@delegate = @delegate;
        }
        public delegate Saxon.Eej.ee.stream.feed.ItemFeed Lambda (Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context);
        public Saxon.Eej.ee.stream.feed.ItemFeed makeItemFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context) {
            return /*Delegate.makeItemFeed()*/@delegate(watchManager, @out, context);
        }
    
    }
}
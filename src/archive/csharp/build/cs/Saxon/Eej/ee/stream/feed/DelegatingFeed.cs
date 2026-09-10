using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public abstract class DelegatingFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        public DelegatingFeed(Saxon.Hej.@event.ProxyOutputter proxy, Saxon.Hej.expr.XPathContext context) : base (proxy, context, true) {
        }
        public override Saxon.Eej.ee.stream.feed.ItemFeed getResultFeed() {
            Saxon.Hej.@event.ProxyOutputter proxy = (Saxon.Hej.@event.ProxyOutputter)(getNextOutputter());
            return (Saxon.Eej.ee.stream.feed.ItemFeed)(proxy.getNextOutputter());
        }
        public override void append(Saxon.Hej.om.Item item) {
            Saxon.Hej.@event.ProxyOutputter proxy = (Saxon.Hej.@event.ProxyOutputter)(getNextOutputter());
            proxy.append(item);
        }
    
    }
}
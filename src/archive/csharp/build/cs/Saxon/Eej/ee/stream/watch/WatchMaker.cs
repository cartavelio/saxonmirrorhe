using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public delegate Saxon.Eej.ee.stream.watch.Trigger WatchMaker (Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed @out, Saxon.Hej.expr.XPathContext context);
}
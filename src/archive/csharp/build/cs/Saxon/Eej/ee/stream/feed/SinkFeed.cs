using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class SinkFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        public SinkFeed(Saxon.Hej.expr.XPathContext context) : base (new Saxon.Hej.@event.SinkOutputter(), context, true) {
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
        }
        public override void append(Saxon.Hej.om.Item item) {
        }
        public override bool handlesAppend() {
            return true;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class NoOpenOrCloseFeed : Saxon.Eej.ee.stream.feed.ItemFeed {
        public NoOpenOrCloseFeed(Saxon.Eej.ee.stream.feed.ItemFeed parentFeed, Saxon.Hej.expr.XPathContext context) : base (parentFeed, context) {
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            return getResultFeed().startSelectedParentNode(node, locationId);
        }
        public override void append(Saxon.Hej.om.Item item) {
            getNextOutputter().append(item);
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            getResultFeed().endSelectedParentNode(locationId);
        }
        public override void close() {
        }
    
    }
}
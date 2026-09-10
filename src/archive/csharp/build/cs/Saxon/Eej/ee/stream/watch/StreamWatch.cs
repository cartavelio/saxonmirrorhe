using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class StreamWatch : Saxon.Eej.ee.stream.watch.AbstractWatch {
        private readonly Saxon.Eej.ee.stream.Inversion action;
        private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private readonly Saxon.Hej.expr.XPathContext context;
        public StreamWatch(Saxon.Eej.ee.stream.Inversion action, Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.expr.XPathContext context)  {
            this.action = action;
            this.watchManager = watchManager;
            this.context = context;
        }
        public override bool matchesNode(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.expr.XPathContext context) {
            return (node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT);
        }
        public override bool matchesNodesOfKind(Saxon.Hej.type.UType kind) {
            return kind.subsumes(Saxon.Hej.type.UType.DOCUMENT);
        }
        public override void setAnchorNode(Saxon.Eej.ee.stream.om.FleetingParentNode anchor) {
        }
        public override Saxon.Eej.ee.stream.om.FleetingParentNode getAnchorNode() {
            return null;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            Saxon.Eej.ee.stream.Inversion inversion = action;
            Saxon.Hej.tree.iter.ManualIterator iter = new Saxon.Hej.tree.iter.ManualIterator(node);
            context.setCurrentIterator(iter);
            Saxon.Eej.ee.stream.feed.ItemFeed parentFeed = new Saxon.Eej.ee.stream.feed.DecomposingFeed(watchManager.getOutputter(), context);
            Saxon.Eej.ee.stream.watch.Watch watch = inversion.getWatch(watchManager, parentFeed, context);
            watch.setAnchorNode(node);
            watchManager.addWatch(watch, true);
            return null;
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
        }
    
    }
}
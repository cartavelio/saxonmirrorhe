using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class SnapshotFeed : Saxon.Eej.ee.stream.feed.AbsorptionFeed {
        private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private bool copyAccumulators;
        public static Saxon.Eej.ee.stream.watch.Trigger getWatch(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.pattern.Pattern selection, Saxon.Eej.ee.stream.watch.WatchManager wm, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.feed.SnapshotFeed action = new Saxon.Eej.ee.stream.feed.SnapshotFeed(wm, result, context);
            action.copyAccumulators = false;
            Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
            watch.setPipelineConfiguration(wm.getPipelineConfiguration());
            return watch;
        }
        public SnapshotFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
            this.watchManager = watchManager;
            this.copyAccumulators = true;
        }
        protected internal override Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
            return new Saxon.Eej.ee.stream.feed.SnapshotFeed.PendingSnapshotAction(this, sequenceNr, node);
        }
        protected internal override Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item) {
            return Saxon.Hej.functions.SnapshotFn.snapshotSingle(item);
        }
        private class PendingSnapshotAction : Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem {
            public int sequence;
            public Saxon.Hej.@event.BuilderMonitor monitor;
            public System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> ancestorList;
            private readonly Saxon.Eej.ee.stream.feed.SnapshotFeed feed;
            public PendingSnapshotAction(Saxon.Eej.ee.stream.feed.SnapshotFeed feed, int sequence, Saxon.Eej.ee.stream.om.FleetingParentNode node) : base (sequence) {
                this.feed = feed;
                this.sequence = sequence;
                ancestorList = Saxon.Hej.functions.SnapshotFn.makeAncestorList(node);
                monitor = Saxon.Hej.functions.SnapshotFn.openAncestors(node, ancestorList, feed.getContext());
                monitor.markNextNode(node.getNodeKind());
            }
            public override Saxon.Hej.@event.Receiver getGatherer() {
                Saxon.Hej.tree.tiny.TinyBuilder builder = (Saxon.Hej.tree.tiny.TinyBuilder)(monitor.getNextReceiver());
                if (feed.copyAccumulators) {
                    return new Saxon.Eej.ee.stream.feed.CopyOfFeed.AccumulatorCopier(feed.watchManager, monitor, builder);
                } else {
                    return monitor;
                }
            }
            public override Saxon.Hej.om.Sequence deliver() {
                Saxon.Hej.functions.SnapshotFn.closeAncestors(ancestorList, monitor);
                return monitor.getMarkedNode();
            }
            public override void rollback() {
            }
        
        }
    
    }
}
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.functions.SnapshotFn;
import net.sf.saxon.event.BuilderMonitor;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;

import java.util.List;

/**
 * This class provides streamed implementation of the snapshot() function
 */


public class SnapshotFeed extends AbsorptionFeed {

    private final WatchManager watchManager;
    private boolean copyAccumulators;

    /**
     * Create a Watch whose effect is to create snapshots of the selected (streamed) nodes and pass
     * them to the supplied feed
     * @param result The feed to which the snapshot nodes should be supplied
     * @param selection defines the nodes to be selected
     * @param wm The WatchManager that will notify the selected nodes
     * @param context the XPath evaluation context
     * @return the new Watch
     */

    public static Trigger getWatch(ItemFeed result, Pattern selection, WatchManager wm, XPathContext context) {
        SnapshotFeed action = new SnapshotFeed(wm, result, context);
        action.copyAccumulators = false;
        Trigger watch = new Trigger(selection, action, context);
        watch.setPipelineConfiguration(wm.getPipelineConfiguration());
        return watch;
    }

    public SnapshotFeed(WatchManager watchManager, ItemFeed result, XPathContext context) {
        super(result, context);
        this.watchManager = watchManager;
        this.copyAccumulators = true;
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) throws XPathException {
        return new PendingSnapshotAction(this, sequenceNr, node);
    }

    @Override
    protected Sequence processGroundedItem(Item item) {
        return SnapshotFn.snapshotSingle(item);
    }

    /**
     * A pending item represents either a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private static class PendingSnapshotAction extends PendingItem {
        public int sequence;
        public BuilderMonitor monitor;
        public List<NodeInfo> ancestorList;
        private final SnapshotFeed feed;

        public PendingSnapshotAction(SnapshotFeed feed, int sequence, FleetingParentNode node) throws XPathException {
            super(sequence);
            this.feed = feed;
            this.sequence = sequence;
            ancestorList = SnapshotFn.makeAncestorList(node);
            monitor = SnapshotFn.openAncestors(node, ancestorList, feed.getContext());
            monitor.markNextNode(node.getNodeKind());
        }

        @Override
        public Receiver getGatherer() throws XPathException {
            TinyBuilder builder = (TinyBuilder)monitor.getNextReceiver();
            if (feed.copyAccumulators) {
                // Ensure that accumulator values are copied
                return new CopyOfFeed.AccumulatorCopier(feed.watchManager, monitor, builder);
            } else {
                return monitor;
            }
        }

        @Override
        public Sequence deliver() throws XPathException {
            SnapshotFn.closeAncestors(ancestorList, monitor);
            return monitor.getMarkedNode();
        }

        @Override
        public void rollback() {
        }
    }
}

// Copyright (c) 2010-2023 Saxonica Limited

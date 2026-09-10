package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.ManualGroupIterator;
import com.saxonica.ee.stream.adjunct.FeedToEventBuffer;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoCloseFeed;
import com.saxonica.ee.stream.feed.NoOpenOrCloseFeed;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.TeeOutputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.CompositeAtomicKey;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AncestorQualifiedPattern;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.value.AtomicValue;

import java.util.*;

import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.MATCHES;
import static com.saxonica.ee.stream.feed.FilteringFeed.FilterResult.SKIP;


/**
 * This watch implements xsl:for-each-group in streaming mode, in cases where the population is being forked
 * (using group-by), and where the population is not grounded.
 *
 * The general idea here is that each item is processed as it is encountered, by sending it to an
 * agent which is responsible for processing the group that it belongs to. This means that the processing
 * of different groups is interleaved (they are processed in parallel, though not actually in separate
 * threads). On completion, the outputs from each of the per-group agents are concatenated in order.
 *
 * The memory needed for this process is the amount needed to hold the output of the xsl:for-each-group
 * instruction; the strategy is only really effective in cases where this is significantly less than
 * the memory needed to hold the input. One case where this definitely applies is where the body of the
 * xsl:for-each-group is a call to xsl:result-document; another is where the grouping action does aggregation,
 * e.g. count(current-group()) or sum(current-group()).
 *
 * Although this class inherits from ForEachAction, it does not use all the functionality of the superclass.
 * In particular, it is not capable of handling overlapping nodes in the population.
 */


public class ForEachGroupParallelAction extends ForEachAction {

    private final HashMap<CompositeAtomicKey, GroupInfo> groups = new HashMap<>();
    private final List<GroupInfo> groupList = new ArrayList<>(); // retains groups in order of first appearance
    private int groupSequence = 1;
    private final XPathContext populationContext;
    private final StringCollator collator;
    private final int implicitTimezone;
    private ItemFeed currentItemFeed;
    private final boolean compositeKeys;
    private NodeInfo anchorNode;

    // Information held for each group being processed

    private static class GroupInfo {
        public XPathContext bodyContext;    // context used for evaluating the for-each-group action, for a given group
        public ItemFeed feed;                   // feed used for evaluating the for-each-group action, for a given group
        public FeedToEventBuffer outputter; // destination of the result of processing the for-each-group action, for a given group
        public List<Watch> watchList;
        public WatchManager.GroupingScope scope;
    }

    public ForEachGroupParallelAction(WatchManager watchManager, ForEachGroup expression, ItemFeed result, XPathContext context) {
        super(watchManager, expression, result, context);
        populationContext = context.newMinorContext();
        collator = expression.getCollation();
        implicitTimezone = context.getImplicitTimezone();
        compositeKeys = expression.isComposite();
    }


    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        populationContext.setCurrentIterator(new ManualIterator());
    }

    /**
     * Compute the grouping key for an item in the population. In general this is a sequence, which
     * may be interpreted as multiple single keys or as a composite key.
     * @param node the item in the population whose grouping key is to be calculated
     * @param comparisonKey caller supplies an empty list, which is populated with the list of match keys,
     *                      one for each key value
     * @return an AtomicArray holding the actual grouping keys as atomic values
     * @throws XPathException if anything goes wrong
     */

    private AtomicArray getGroupingKey(Item node, List<AtomicMatchKey> comparisonKey) throws XPathException {
        if (currentItemFeed != null) {
            throw new XPathException("xsl:for-each-group can't handle overlapping (crawling) nodes");
        }
        ManualIterator popIter = (ManualIterator) populationContext.getCurrentIterator();
        popIter.setContextItem(node);
        popIter.incrementPosition();
        Expression groupingKey = ((ForEachGroup) getExpression()).getGroupingKey();
        SequenceIterator iter = groupingKey.iterate(populationContext);
        AtomicArray key = new AtomicArray(iter);
        for (AtomicValue val : key) {
            comparisonKey.add(val.getXPathMatchKey(collator, implicitTimezone));
        }
        return key;
    }


    /**
     * Signal that an element (or document node) has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws XPathException if a dynamic error occurs while processing the start-element event
     * @return a Receiver to be notified of subsequent events, or null
     */

    /*@Nullable*/
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        List<AtomicMatchKey> comparisonKeyParts = new ArrayList<>();
        AtomicArray key = getGroupingKey(node, comparisonKeyParts);
        anchorNode = node;
        List<CompositeAtomicKey> separateKeys;
        List<AtomicValue> distinctKeys = null;
        if (compositeKeys) {
            separateKeys = new ArrayList<>(1);
            separateKeys.add(new CompositeAtomicKey(comparisonKeyParts));
        } else {
            separateKeys = new ArrayList<>(comparisonKeyParts.size());
            distinctKeys = new ArrayList<>();
            Set<AtomicMatchKey> duplicates = new HashSet<>();
            int kk = 0;
            for (AtomicMatchKey oneKey : comparisonKeyParts) {
                if (duplicates.add(oneKey)) {
                    List<AtomicMatchKey> singleKey = new ArrayList<>(1);
                    singleKey.add(oneKey);
                    separateKeys.add(new CompositeAtomicKey(singleKey));
                    distinctKeys.add(key.itemAt(kk));
                }
                kk++;
            }
        }

        Receiver result = null;
        List<ItemFeed> groupFeeds = new ArrayList<>();
        int k = 0;

        for (CompositeAtomicKey groupingKey : separateKeys) {
            AtomicSequence actualKey = compositeKeys ? key : distinctKeys.get(k++);
            GroupInfo gp = groups.get(groupingKey);
            boolean newGroup = gp == null;
            //System.err.println("key " + key.getStringValue() + " new group? " + newGroup);
            WatchManager.GroupingScope scope = getWatchManager().startGroupingScope();
            getWatchManager().startCapturingGroupingWatches(scope);
            if (newGroup) {
                gp = makeNewGroup(node, groupingKey, actualKey);
            } else {
                ((ManualGroupIterator)gp.bodyContext.getCurrentGroupIterator()).appendToCurrentGroup(node);
            }
            gp.scope = scope;

            if (isActionConsuming()) {
                if (newGroup) {
                    ItemFeed toOutputter = gp.outputter;
                    ItemFeed thisFeed = new NoOpenOrCloseFeed(toOutputter, gp.bodyContext);
                    Trigger watch = getInversion().getWatch(getWatchManager(), thisFeed, gp.bodyContext);
                    final Pattern pattern = watch.getSelection();
                    ItemFeed actionFeed = watch.getAction();
                    if (pattern instanceof AnchorPattern) {
                        gp.feed = actionFeed;
                        gp.feed.open(getTerminator());
                    } else {
                        FilteringFeed.Filter filter = new FilteringFeed.FilterLambda((item, position) ->
                                pattern.matchesBeneathAnchor((NodeInfo) item, anchorNode, bodyContext) ? MATCHES : SKIP);
                        gp.feed = new FilteringFeed(actionFeed, getContext(), filter);
                        if (pattern instanceof AncestorQualifiedPattern) {
                            // This typically means we need to watch for ./xxx or current-group()/xxx
                            getWatchManager().addWatch(watch, true);
                            // The watch takes care of closing the action feed, so we don't need to do it twice (si-fork-809)
                            gp.feed = new NoCloseFeed(gp.feed, getContext());
                        }
                    }
                } else {
                    if (gp.watchList != null) {
                        for (Watch w : gp.watchList) {
                            w.setAnchorNode(node);
                        }
                    }
                }
                groupFeeds.add(gp.feed);

            } else {
                if (newGroup) {
                    Expression action = ((ForEachGroup) getExpression()).getActionExpression();
                    FeedToEventBuffer out = gp.outputter;
                    action.process(out, gp.bodyContext);
                }
            }
//            if (newGroup) {
//                gp.watchList = getWatchManager().popGroupingScope(gp.scope);
//            } else {
//                getWatchManager().popGroupingScope(gp.scope);
//            }
            if (newGroup) {
                gp.watchList = gp.scope.watches;
            }
            getWatchManager().stopCapturingGroupingWatches(scope);
        }
        if (!groupFeeds.isEmpty()) {
            if (groupFeeds.size() == 1) {
                currentItemFeed = groupFeeds.get(0);
                result = currentItemFeed.startSelectedParentNode(node, locationId);
            } else {
                currentItemFeed = new ForkingFeed(groupFeeds, getResultFeed(), getContext());
                result = currentItemFeed.startSelectedParentNode(node, locationId);
            }
        }
        return result;
    }

    private GroupInfo makeNewGroup(Item node, CompositeAtomicKey comparisonKey, AtomicSequence key) {
        GroupInfo gp = new GroupInfo();
        gp.outputter = new FeedToEventBuffer(getNextOutputter(), this, getContext());
        gp.outputter.setPipelineConfiguration(getPipelineConfiguration());

        // Because execution of the for-each-group body processes multiple groups effectively in parallel,
        // or at least interleaved, each needs its own copy of the local variables. Local variables declared
        // outside the for-each-group should be shared, but those declared inside should have one instance
        // per group.
        XPathContextMajor bodyContext = XPathContextMajor.newThreadContext((XPathContextMinor) getContext());
        ManualGroupIterator mgi = new ManualGroupIterator(node, groupSequence++);
        mgi.setCurrentGroupingKey(key);
        mgi.startNewCurrentGroup(node);
        bodyContext.setCurrentIterator(mgi);
        bodyContext.setCurrentGroupIterator(mgi);
        //bodyContext.setReceiver(gp.outputter);
        gp.bodyContext = bodyContext;

        groups.put(comparisonKey, gp);
        groupList.add(gp);
        return gp;
    }


    /**
     * Signal that an item (other than an element) that matches the selection
     * has been found.
     *
     * @param node       the item that was matched.
     *
     */

    @Override
    public void append(Item node) throws XPathException {
        List<AtomicMatchKey> comparisonKeyParts = new ArrayList<>();
        AtomicArray key = getGroupingKey(node, comparisonKeyParts);

        List<CompositeAtomicKey> separateKeys;
        if (compositeKeys) {
            separateKeys = new ArrayList<>(1);
            separateKeys.add(new CompositeAtomicKey(comparisonKeyParts));
        } else {
            separateKeys = new ArrayList<>(comparisonKeyParts.size());
            Set<AtomicMatchKey> duplicates = new HashSet<>();
            for (AtomicMatchKey oneKey : comparisonKeyParts) {
                if (duplicates.add(oneKey)) {
                    List<AtomicMatchKey> singleKey = new ArrayList<>(1);
                    singleKey.add(oneKey);
                    separateKeys.add(new CompositeAtomicKey(singleKey));
                }
            }
        }

        int k = 0;
        for (CompositeAtomicKey groupingKey : separateKeys) {
            AtomicSequence actualKey = compositeKeys ? key : key.itemAt(k++);
            GroupInfo gp = groups.get(groupingKey);
            boolean newGroup = gp == null;
            if (newGroup) {
                gp = makeNewGroup(node, groupingKey, actualKey);
            }


            if (isActionConsuming()) {
                if (newGroup) {
                    ItemFeed thisFeed = new NoOpenOrCloseFeed(getResultFeed(), getBodyContext());
                    Trigger watch = getInversion().getWatch(getWatchManager(), thisFeed, gp.bodyContext);
                    gp.feed = watch.getAction();
                    gp.feed.open(getTerminator());
                }
                gp.feed.append(node);
            } else {
                if (newGroup) {
                    Expression action = ((ForEachGroup) getExpression()).getActionExpression();
                    SequenceTool.supply(action.iterate(gp.bodyContext), item -> getResultFeed().append(item));
                }
            }
        }
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @throws net.sf.saxon.type.ValidationException
     *          May be raised if the constraint implemented by this
     *          Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (isActionConsuming()) {
            currentItemFeed.endSelectedParentNode(locationId);
            currentItemFeed = null;
        }
    }

    @Override
    public void close() throws XPathException {
        for (GroupInfo gp : groupList) {
            if (gp.watchList != null) {
                for (Watch w : gp.watchList) {
                    w.close();
                }
            }
            if (gp.feed != null) {
                gp.feed.close();
            }
            FeedToEventBuffer out = gp.outputter;
            out.replay(getNextOutputter());
//            out.close();
//            for (Item item : out.getList()) {
//                getResult().processItem(item);
//            };
        }
        super.close();
    }

    private static class ForkingFeed extends ItemFeed {

        private final List<ItemFeed> prongs;

        public ForkingFeed(List<ItemFeed> prongs, ItemFeed result, XPathContext context) {
            super(result, context);
            this.prongs = prongs;
        }

        @Override
        public void close() throws XPathException {
            for (ItemFeed prong : prongs) {
                prong.close();
            }
        }

        @Override
        public PipelineConfiguration getPipelineConfiguration() {
            return null;
        }

        @Override
        public void open(Terminator terminator) throws XPathException {
            for (ItemFeed prong : prongs) {
                prong.open(terminator);
            }
        }

        @Override
        public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
            Receiver result = null;
            for (ItemFeed prong : prongs) {
                Receiver r = prong.startSelectedParentNode(node, locationId);
                if (r != null) {
                    result = result == null ? r : new TeeOutputter(result, r);
                }
            }
            return result;
        }

        @Override
        public void endSelectedParentNode(Location locationId) throws XPathException {
            for (ItemFeed prong : prongs) {
                prong.endSelectedParentNode(locationId);
            }
        }

        @Override
        public void append(Item item) throws XPathException {
            for (ItemFeed prong : prongs) {
                prong.append(item);
            }
        }

//        @Override
//        public Outputter getOutputter() {
//            return getResult().getOutputter();
//        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.ManualGroupIterator;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.adjunct.ForEachGroupAdjunct;
import com.saxonica.ee.stream.adjunct.GroupAdjacentTester;
import com.saxonica.ee.stream.adjunct.GroupBoundaryTester;
import com.saxonica.ee.stream.feed.FilteringFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoOpenOrCloseFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.ForEachGroup;
import net.sf.saxon.functions.CurrentGroupCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.AncestorQualifiedPattern;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

import java.util.List;


/**
 * This watch implements xsl:for-each-group in streaming mode, in cases where the population is being partitioned
 * (that is, group-adjacent, group-starting-with, and group-ending-with), and where the population is not grounded.
 * It is informed of the nodes or other items selected by the
 * xsl:for-each instruction; when such an item is encountered, it locates the relevant (inverted)
 * action representing the body of the for-each, and activates it.
 * <p>The feed is capable of handling a select expression that selects non-peer nodes (that is, one selected node
 * that is a descendant of another). A top-level selected node is processed directly to the output receiver;
 * a nested node is processed to create a temporary result tree, which is then appended to the output sequence
 * when the processing of the outer selected node is complete.</p>
 */


public class ForEachGroupPartitionAction extends ItemFeed {

    // bodyContext contains the iterator over the nodes selected by the for-each instruction,
    // and is the context in which the inner inverted expression is evaluated
    protected XPathContext bodyContext;
    private final WatchManager watchManager;
    private GroupBoundaryTester boundaryTester;
    private ItemFeed currentGroupFeed;
    private final XPathContext populationContext;
    private final Expression action;
    private Inversion inversion;
    private final boolean actionIsConsuming;
    private boolean closed = true;
    private List<Watch> currentGroupWatches;
    public boolean feedAppliesToWholeGroup = false;
    private boolean closeAtEndElement;
    private NodeInfo anchorNode;
    private Watch thisGroupWatch = null;
    private WatchManager.GroupingScope currentGroupingScope = null;

    public ForEachGroupPartitionAction(WatchManager watchManager, ForEachGroup expression, ItemFeed result, XPathContext context) {
        super(expression, result, context);
        this.watchManager = watchManager;
        action = expression.getActionExpression();
        actionIsConsuming = Streamability.getSweep(action) == Sweep.CONSUMING;
        populationContext = context.newMinorContext();
    }

    public void setInversion(Inversion inversion) {
        this.inversion = inversion;
    }

    protected Inversion getInversion() {
        return inversion;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        makeBodyContext();
        populationContext.setCurrentIterator(new ManualIterator());
        boundaryTester = ForEachGroupAdjunct.makeBoundaryTester((ForEachGroup) getExpression(), populationContext);
        closed = false;
    }

    protected void makeBodyContext() {
        bodyContext = getContext().newContext();
        ManualGroupIterator mgi = new ManualGroupIterator(null, 0);
        bodyContext.setCurrentIterator(mgi);
        ((XPathContextMajor) bodyContext).setCurrentGroupIterator(mgi);
    }


    /**
     * Signal that an element (or document node) has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws XPathException if a dynamic error occurs while processing the start-element event
     * @return a receiver that collects the contents of the selected node, or null
     */

    /*@Nullable*/
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        ManualIterator popIter = (ManualIterator) populationContext.getCurrentIterator();
        popIter.setContextItem(node);
        popIter.incrementPosition();
        anchorNode = node;
        boolean needToStartNewGroup = boundaryTester.notifyItem();
        //System.err.println("FEG start new group = " + startNewGroup);
        if (actionIsConsuming) {
            if (needToStartNewGroup) {
                startNewGroup(node);
                closeAtEndElement = true;
                return currentGroupFeed.startSelectedParentNode(node, locationId);
            } else {
                ((ManualGroupIterator) bodyContext.getCurrentGroupIterator()).appendToCurrentGroup(node);
                for (Watch w : currentGroupWatches) {
                    if (w instanceof Trigger && ((Trigger)w).isMatchCurrentGroup()) {
                        w.setAnchorNode(node);
                    }
                    // TODO: for non-grouping watches, we can delete the Watch
                }
                closeAtEndElement = feedAppliesToWholeGroup;
                if (feedAppliesToWholeGroup) {
                    return currentGroupFeed.startSelectedParentNode(node, locationId);
                }
            }
            return currentGroupFeed.startSelectedParentNode(node, locationId);
        } else {
            maybeStartNewGroup(node, needToStartNewGroup);
            return null;
        }
    }

    


    private void maybeStartNewGroup(Item node, boolean startNewGroup) throws XPathException {
        if (startNewGroup) {
            ManualIterator iter = (ManualIterator) bodyContext.getCurrentIterator();
            iter.setContextItem(node);
            iter.incrementPosition();

            //localContext.setCurrentTemplateRule(rule);     // TODO: clear current template rule
            SequenceIterator stepIter = action.iterate(bodyContext);
            processItems(stepIter, getNextOutputter());
        }
    }

    /**
     * Signal that an item (other than an element) that matches the selection
     * has been found.
     *
     * @param node the item that was matched.
     */

    @Override
    public void append(Item node) throws XPathException {
        ManualIterator popIter = (ManualIterator) populationContext.getCurrentIterator();
        popIter.setContextItem(node);
        popIter.incrementPosition();
        boolean needToStartNewGroup = boundaryTester.notifyItem();
        if (actionIsConsuming) {
            if (needToStartNewGroup) {
                startNewGroup(node);
            }
            currentGroupFeed.append(node);
        } else {
            maybeStartNewGroup(node, needToStartNewGroup);
        }
    }

    private void startNewGroup(final Item node) throws XPathException {
        if (currentGroupFeed != null) {
            watchManager.endGroupingScope(currentGroupingScope);
            watchManager.stopCapturingGroupingWatches(currentGroupingScope);
            currentGroupFeed.close();
        }
        if (thisGroupWatch != null) {
            watchManager.removeWatch(thisGroupWatch);
        }
        currentGroupingScope = watchManager.startGroupingScope();
        watchManager.startCapturingGroupingWatches(currentGroupingScope);
        ManualGroupIterator firstItemsIter = (ManualGroupIterator) bodyContext.getCurrentIterator();
        firstItemsIter.setContextItem(node);
        firstItemsIter.incrementPosition();
        firstItemsIter.startNewCurrentGroup(node);
        if (boundaryTester instanceof GroupAdjacentTester) {
            firstItemsIter.setCurrentGroupingKey(((GroupAdjacentTester) boundaryTester).getCurrentGroupingKey());
        }
        if (node instanceof FleetingNode) {
            ItemFeed thisFeed = new NoOpenOrCloseFeed(getResultFeed(), bodyContext);
            Trigger watch = getInversion().getWatch(watchManager, thisFeed, bodyContext);
            final Pattern pattern = watch.getSelection();
            ItemFeed actionFeed = watch.getAction();
            if (pattern.matchesCurrentGroup()) {
                feedAppliesToWholeGroup = true;
            }
            boolean opened = false;
            if (pattern instanceof AnchorPattern) {
                currentGroupFeed = actionFeed;
            } else {
                FilteringFeed.Filter filter = new FilteringFeed.FilterLambda(
                        (item, position) -> pattern.matchesBeneathAnchor((NodeInfo)item, anchorNode, bodyContext)
                                ? FilteringFeed.FilterResult.MATCHES
                                : FilteringFeed.FilterResult.SKIP);
                currentGroupFeed = new FilteringFeed(watchManager, actionFeed, getContext(), filter);
                if (pattern instanceof AncestorQualifiedPattern) {
                    // This typically means we need to watch for ./xxx or current-group()/xxx
                    watchManager.addWatch(watch, true);
                    opened = true;
                    thisGroupWatch = watch;
                }
            }
            if (!opened) {
                currentGroupFeed.open(getTerminator());
            }
        } else {
            currentGroupFeed = new NoOpenOrCloseFeed(findCurrentGroupFeed(), bodyContext);
            currentGroupFeed.open(getTerminator());
        }
        //currentGroupWatches = watchManager.peekGroupingScope(currentGroupingScope);
        currentGroupWatches = currentGroupingScope.watches;
        //watchManager.freezeCurrentGroup(currentGroupingScope);
        currentGroupingScope.isActive = false;
    }

    private ItemFeed findCurrentGroupFeed() throws XPathException {
        ItemFeed thisFeed = new NoOpenOrCloseFeed(getResultFeed(), bodyContext);
        Trigger watch = getInversion().getWatch(watchManager, thisFeed, bodyContext);
        ItemFeed feed = watch.getAction();
        while (!(feed.getExpression() instanceof CurrentGroupCall)) {
            feed = feed.getResultFeed();
        }
        return feed;

    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
     *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (currentGroupFeed != null && closeAtEndElement) {
            currentGroupFeed.endSelectedParentNode(locationId);
        }
    }

    @Override
    public void close() throws XPathException {
        if (currentGroupFeed != null && !closed) {
            closed = true;
            watchManager.endGroupingScope(currentGroupingScope);
            watchManager.stopCapturingGroupingWatches(currentGroupingScope);
            currentGroupFeed.close();
        }
        super.close();
    }
}

// Copyright (c) 2009-2022 Saxonica Limited
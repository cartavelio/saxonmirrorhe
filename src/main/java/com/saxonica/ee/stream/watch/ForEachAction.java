package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoOpenOrCloseFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.ContextSwitchingExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;


/**
 * This watch implements xsl:for-each in streaming mode. It is informed of the nodes or other items selected by the
 * xsl:for-each instruction; when such an item is encountered, it locates the relevant (inverted)
 * action representing the body of the for-each, and activates it.
 */


public class ForEachAction extends ItemFeed {

    // bodyContext contains the iterator over the nodes selected by the for-each instruction,
    // and is the context in which the inner inverted expression is evaluated
    protected XPathContext bodyContext;
    protected final WatchManager watchManager;

    private final Expression action;
    private Inversion inversion;
    protected PullEvaluator actionPuller;
    private final boolean actionIsConsuming;

    public ForEachAction(WatchManager watchManager, Expression expression, ItemFeed result, XPathContext context) {
        super(expression, result, context);
        //System.err.println("new FEA " + this);
        this.watchManager = watchManager;
        action = ((ContextSwitchingExpression)expression).getActionExpression();
        actionIsConsuming = Streamability.getSweep(action) == Sweep.CONSUMING;
        if (actionPuller == null && !actionIsConsuming) {
            actionPuller = action.makeElaborator().elaborateForPull();
        }
    }

    /**
     * Set the inversion of the for-each action (the body of the loop)
     * @param inversion the inverted code implementing the body of the for-each
     */

    public void setInversion(Inversion inversion) {
        this.inversion = inversion;
    }

    protected Inversion getInversion() {
        return inversion;
    }

    protected WatchManager getWatchManager() {
        return watchManager;
    }

    protected Expression getAction() {
        return action;
    }

    protected boolean isActionConsuming() {
        return actionIsConsuming;
    }

    protected XPathContext getBodyContext() {
        return bodyContext;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        makeBodyContext();
    }

    protected void makeBodyContext() {
        bodyContext = getContext().newMinorContext();
        ManualIterator iter = new ManualIterator();
        bodyContext.setCurrentIterator(iter);
    }


    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws net.sf.saxon.type.ValidationException
     *          May be raised if the constraint implemented by this
     *          Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @return always null for this method, but may be non-null in subclasses
     */

    /*@Nullable*/
    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (actionIsConsuming) {
            startSelectedNode(node);
            ItemFeed thisFeed = new NoOpenOrCloseFeed(getResultFeed(), bodyContext);
            Watch watch = inversion.getWatch(watchManager, thisFeed, bodyContext);
            watch.setAnchorNode(node);
            watchManager.addWatch(watch, true);
            return null;
        } else {
            ManualIterator iter = (ManualIterator) bodyContext.getCurrentIterator();
            iter.setContextItem(node);
            iter.incrementPosition();

            //localContext.setCurrentTemplateRule(rule);     // TODO: clear current template rule
            SequenceIterator stepIter = actionPuller.iterate(bodyContext);
            processItems(stepIter, getResultFeed());
            return null;
        }
    }


    private void startSelectedNode(FleetingNode node) throws XPathException {
        //System.err.println("FEA " + this + " .startSelectedNode( " + node.getDisplayName() + ")");
        ManualIterator iter = (ManualIterator) bodyContext.getCurrentIterator();
        iter.setContextItem(node);
        iter.incrementPosition();

        //localContext.setCurrentTemplateRule(rule);     // TODO: clear current template rule

    }

    /**
     * Method called by the feed for a subexpression to supply a grounded item from the streamed input.
     * @param it the grounded item.
     *
     */

    @Override
    public void append(Item it) throws XPathException {
        if (actionIsConsuming && it instanceof FleetingParentNode) {
            // consuming, typically striding or crawling nodes
            XPathContext cxt = bodyContext;
            ItemFeed thisFeed = new NoOpenOrCloseFeed(getResultFeed(), cxt);
            Watch watch = inversion.getWatch(watchManager, thisFeed, cxt);
            if (watch.matchesNode((FleetingParentNode)it, cxt)) {
                // not sure about this path, I don't think it is exercised by the test suite (MHK Aug 2017)
                startSelectedNode((FleetingNode) it);
                watch.open(Terminator.getInstance());
                watch.processItem(it);
                watch.close();
                endSelectedParentNode(Loc.NONE);
            } else {
                // used in function-5006
                watch.open(Terminator.getInstance());
                getWatchManager().addWatch(watch, true);
            }
            
        } else {
            if (actionPuller == null) {
                actionPuller = action.makeElaborator().elaborateForPull();
            }
            // either the selected nodes are grounded or the action is non-consuming
            XPathContext context = bodyContext;
            ManualIterator iterator = (ManualIterator)context.getCurrentIterator();
            iterator.setContextItem(it);
            iterator.incrementPosition();
            Outputter out = getNextOutputter();
            try {
                SequenceTool.supply(actionPuller.iterate(context), (ItemConsumer<? super Item>) out::append);
            } catch (UncheckedXPathException e) {
                throw e.getXPathException();
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
        // no action
    }

    @Override
    public void close() throws XPathException {
        super.close();
    }
}

// Copyright (c) 2009-2022 Saxonica Limited
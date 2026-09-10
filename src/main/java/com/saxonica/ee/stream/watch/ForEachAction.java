package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.Inversion;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.StreamingSequenceWriter;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.stream.feed.Feed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.NoOpenOrCloseFeed;
import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceOutputter;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.event.SequenceWriter;
import net.sf.saxon.expr.ContextSwitchingExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;
import java.util.Stack;


/**
 * This watch implements xsl:for-each in streaming mode. It is informed of the nodes or other items selected by the
 * xsl:for-each instruction; when such an item is encountered, it locates the relevant (inverted)
 * action representing the body of the for-each, and activates it.
 * <p>The feed is capable of handling a select expression that selects non-peer nodes (that is, one selected node
 * that is a descendant of another). A top-level selected node is processed directly to the output receiver;
 * a nested node is processed to create a temporary result tree, which is then appended to the output sequence
 * when the processing of the outer selected node is complete.</p>
 */


public class ForEachAction extends ItemFeed {

    // bodyContext contains the iterator over the nodes selected by the for-each instruction,
    // and is the context in which the inner inverted expression is evaluated
    protected XPathContext bodyContext;
    private WatchManager watchManager;
    private Stack<Activation> activationStack = new Stack<>();

    private List<Sequence<?>> valuesAwaitingOutput = new ArrayList<>(5);

    private Expression action;
    private Inversion inversion;
    private boolean actionIsConsuming;
    private SequenceWriter sequenceWriter;

    public ForEachAction(WatchManager watchManager, Expression expression, Feed result, XPathContext context) {
        super(expression, result, context);
        //System.err.println("new FEA " + this);
        this.watchManager = watchManager;
        action = ((ContextSwitchingExpression)expression).getActionExpression();
        actionIsConsuming = Streamability.getSweep(action) == Sweep.CONSUMING;
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

    protected Stack<Activation> getActivationStack() {
        return activationStack;
    }

    protected List<Sequence<?>> getValuesAwaitingOutput() {
        return valuesAwaitingOutput;
    }

    @Override
    public SequenceReceiver getReceiver() {
        if (sequenceWriter == null) {
            sequenceWriter = new StreamingSequenceWriter(getPipelineConfiguration(), this);
        }
        return sequenceWriter;
    }

    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        makeBodyContext();
    }

    protected void makeBodyContext() {
        bodyContext = getContext().newMinorContext();
        ManualIterator<?> iter = new ManualIterator<>();
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
     */

    /*@Nullable*/
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (actionIsConsuming) {
            Activation activation = startSelectedNode(node);
            Feed thisFeed = new NoOpenOrCloseFeed(getResult(), activation.context);
            Watch watch = inversion.getWatch(watchManager, thisFeed, activation.context);
            watch.setAnchorNode(node);
            watchManager.addWatch(watch, true);
            return null;
        } else {
            ManualIterator iter = (ManualIterator) bodyContext.getCurrentIterator();
            iter.setContextItem(node);
            iter.incrementPosition();

            //localContext.setCurrentTemplateRule(rule);     // TODO: clear current template rule
            SequenceIterator<?> stepIter = action.iterate(bodyContext);
            processItems(stepIter, getResult());
            return null;
        }
    }


    private Activation startSelectedNode(FleetingNode node) throws XPathException {
        //System.err.println("FEA " + this + " .startSelectedNode( " + node.getDisplayName() + ")");
        ManualIterator iter = (ManualIterator) bodyContext.getCurrentIterator();
        iter.setContextItem(node);
        iter.incrementPosition();

        //localContext.setCurrentTemplateRule(rule);     // TODO: clear current template rule

        XPathContext context;
        if (activationStack.isEmpty()) {
            // we are processing an outermost selected element node
            context = bodyContext;
        } else {
            // xsl:for-each no longer allows crawling select: TODO: remove supporting code
            //throw new XPathException("xsl:for-each (etc) with a crawling select expression is no longer supported");
            // we are processing a node that is a descendant of one that is already active. We write the
            // results of processing this node to a buffered destination.
            // For nested elements we always use a clean outputter.
            // And we also need a copy of the context object.
            context = bodyContext.newMinorContext();
            Receiver out = new SequenceOutputter(getPipelineConfiguration());
            context.setReceiver(out);
            valuesAwaitingOutput.add(net.sf.saxon.value.StringValue.EMPTY_STRING); // to be replaced later
        }

        Activation activation = new Activation(valuesAwaitingOutput.size()-1);
        activation.context = context;
        activation.isDocumentNode = (node instanceof FleetingDocumentNode);
        activationStack.push(activation);
        return activation;
    }

    /**
     * Method called by the feed for a subexpression to supply a grounded item from the streamed input.
     * @param it the grounded item.
     *
     */

    public void processItem(Item<?> it) throws XPathException {
        if (actionIsConsuming && it instanceof FleetingParentNode) {
            // consuming, typically striding or crawling nodes
            XPathContext cxt = activationStack.isEmpty() ? bodyContext : activationStack.peek().context;
            Feed thisFeed = new NoOpenOrCloseFeed(getResult(), cxt);
            Watch watch = inversion.getWatch(watchManager, thisFeed, cxt);
            if (watch.matchesNode((FleetingParentNode)it, cxt)) {
                // not sure about this path, I don't think it is exercised by the test suite (MHK Aug 2017)
                startSelectedNode((FleetingNode) it);
                watch.open(Terminator.getInstance());
                watch.processItem(it);
                watch.close();
                endSelectedParentNode(ExplicitLocation.UNKNOWN_LOCATION);
            } else {
                // used in function-5006
                watch.open(Terminator.getInstance());
                getWatchManager().addWatch(watch, true);
            }
            
        } else {
            // either the selected nodes are grounded or the action is non-consuming
            XPathContext context = bodyContext;
            ManualIterator iterator = (ManualIterator)context.getCurrentIterator();
            iterator.setContextItem(it);
            iterator.incrementPosition();
            Feed parentFeed = getResult();
            action.iterate(context).forEachOrFail(parentFeed::processItem);
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

    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (actionIsConsuming) {
            Activation activation = activationStack.pop();
            XPathContext context = activation.context;

            if (activationStack.isEmpty()) {
                for (Sequence<?> aValuesAwaitingOutput : valuesAwaitingOutput) {
                    SequenceIterator<?> iter = aValuesAwaitingOutput.iterate();
                    processItems(iter, getResult());
                }
                valuesAwaitingOutput.clear();

            } else {
                SequenceOutputter temp = (SequenceOutputter) context.getReceiver();
                int seq = activation.sequence;
                Sequence<Item<?>> root = SequenceExtent.makeSequenceExtent(temp.getList());
                temp.close();
                valuesAwaitingOutput.set(seq, root);
            }
        }
    }

    public void close() throws XPathException {
        super.close();
    }

    /**
     * An activation represents an occurrence of a node that matches the selection, whose start tag
     * has been encountered and whose end tag has not yet been encountered. This is a stack because
     * there can be several nodes in this state, if the selection selects nodes that are not peers of
     * each other.
     */

    protected static class Activation {
        public int sequence;
        public boolean isDocumentNode;
        public XPathContext context;
        public boolean matches;

        public Activation(int sequence) {
            this.sequence = sequence;
        }
    }


}

// Copyright (c) 2009-2013 Saxonica Limited.
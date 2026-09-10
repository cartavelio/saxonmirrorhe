package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.DecomposingFeed;
import com.saxonica.ee.stream.feed.ItemFeed;
import com.saxonica.ee.stream.feed.SnapshotFeed;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorRule;
import net.sf.saxon.expr.instruct.SlotManager;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.functions.AccumulatorFn;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnchorPattern;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeTestPattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SimpleMode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.rules.Rule;
import net.sf.saxon.tree.iter.ManualIterator;
import net.sf.saxon.tree.util.IndexedStack;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AnyExternalObject;
import net.sf.saxon.value.ObjectValue;

import java.util.Stack;

/**
 * This watch implements a single accumulator in streaming mode. Unlike other watches,
 * it is called to process every node in the stream; it then checks whether the accumulator
 * has any rule that matches this node, and if so applies the next-value expression associated
 * with that accumulator rule.
 *
 * <p>The class also provides methods to return the "before" and "after" values of the
 * accumulator for a given node. These have to make provision (a) for handling error situations
 * (where evaluation of the accumulator failed), and (b) for handling the case where the value
 * is requested (by another accumulator) before it is available.</p>
 */


public class AccumulatorWatch extends PatternWatch {

    private final Accumulator accumulator;
    private final SimpleMode preDescentMode;
    private final SimpleMode postDescentMode;

    private final IndexedStack<Sequence> preDescentStack = new IndexedStack<>();
    private final Stack<ComplexContentOutputter> captureStack = new Stack<>();

    private Sequence currentValue;

    private FleetingNode lastPredescentNode = null;
    private FleetingNode lastPostdescentNode = null;

    private AccumulatorFn.Phase beingComputed = AccumulatorFn.Phase.UNSPECIFIED;


    // localContext contains the iterator over the nodes selected by the apply-templates instruction,
    // and is the context in which the matched template rules are evaluated
    private XPathContextMajor context;
    private WatchManager watchManager;
    private Stack<FleetingNode> nodeStack;


    public AccumulatorWatch(Accumulator accumulator) {
        this.accumulator = accumulator;
        this.preDescentMode = accumulator.getPreDescentRules();
        this.postDescentMode = accumulator.getPostDescentRules();
        setSelection(new NodeTestPattern(AnyNodeTest.getInstance()));
    }

    public Accumulator getAccumulator() {
        return accumulator;
    }

    public XPathContext getContext() {
        return context;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        context = watchManager.getXPathContext().newCleanContext();
        context.setCurrentComponent(accumulator.getDeclaringComponent());
        SlotManager slotManager = accumulator.getSlotManagerForInitialValueExpression();
        context.setStackFrame(slotManager, SequenceTool.makeSequenceArray(slotManager.getNumberOfVariables()));
        context.setCurrentIterator(new ManualIterator());
        try {
            currentValue = ExpressionTool.eagerEvaluate(accumulator.getInitialValueExpression(), getContext());
        } catch (XPathException err) {
            // Save the error until the accumulator value is read
            currentValue = new ObjectValue<>(err);
        }
    }

    public void setNodeStack(Stack<FleetingNode> stack) {
        this.nodeStack = stack;
    }


    public void setWatchManager(WatchManager watchManager) {
        this.watchManager = watchManager;
    }

    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the WatchManager while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
     *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @return always null
     */

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
//        System.err.println("AccumulatorWatch.startSelectedParentNode(" +
//                                   accumulator.getAccumulatorName() + "," + node.getDisplayName() + ")");
        int nodeNr = node.getNodeNumber();
        if (lastPredescentNode != null && nodeNr == lastPredescentNode.getNodeNumber()) {
            // We have already processed this event
            return null;
        }
        lastPredescentNode = node;

        ManualIterator iter = (ManualIterator) context.getCurrentIterator();
        iter.setContextItem(node);
        iter.incrementPosition();

        Rule rule = preDescentMode.getRule(node, context);
        Sequence value = currentValue;
        if (rule != null) {
            beingComputed = AccumulatorFn.Phase.BEFORE;
            value = applyRule(node, rule);
            beingComputed = AccumulatorFn.Phase.UNSPECIFIED;
            currentValue = value;
            if (accumulator.isTracing()) {
                context.getConfiguration().getLogger().info(accumulator.getAccumulatorName().getDisplayName() +
                                                                    " BEFORE " + Navigator.getPath(node) + ": " + Err.depictSequence(value));
            }
        }

        // If there is a capturing post-descent rule, then fire it off now
        Rule capture = postDescentMode.getRule(node, context);
        if (capture != null && ((AccumulatorRule)capture.getAction()).isCapturing()) {
            SequenceCollector so = new SequenceCollector(getPipelineConfiguration());
            ComplexContentOutputter cco = new ComplexContentOutputter(so);
            ItemFeed feed = new DecomposingFeed(cco, context);
            Watch snapshotWatch = SnapshotFeed.getWatch(feed, AnchorPattern.getInstance(), watchManager, context);
            watchManager.addWatch(snapshotWatch, true);
            captureStack.push(cco);
        }
        int depth = nodeStack.size();
        setAtDepth(preDescentStack, depth-1, value);

        return null;
    }

    private static boolean valueIsException(Sequence value) {
        return value instanceof AnyExternalObject
                && ((AnyExternalObject) value).getWrappedObject() instanceof XPathException;
    }

    private static void detectErrorValue(Sequence value) throws XPathException {
        if (valueIsException(value)) {
            throw (XPathException) ((AnyExternalObject) value).getWrappedObject();
        }
    }


    private Sequence applyRule(FleetingNode node, Rule rule) throws XPathException {
        if (valueIsException(currentValue)) {
            return currentValue;
        }
        AccumulatorRule target = (AccumulatorRule) rule.getAction();
        Expression delta = target.getNewValueExpression();
        XPathContextMajor c2 = context.newCleanContext();
        final Controller controller = c2.getController();
        assert controller != null;

        NodeInfo targetNode = node;
        if (target.isPostDescent() && target.isCapturing()) {
            ComplexContentOutputter cco = captureStack.pop();
            SequenceCollector so = (SequenceCollector)cco.getReceiver();
            targetNode = (NodeInfo)so.getFirstItem();
        }

        ManualIterator initialNode = new ManualIterator(targetNode);
        c2.setCurrentIterator(initialNode);
        c2.openStackFrame(target.getStackFrameMap());
        c2.setLocalVariable(0, currentValue);
        c2.setCurrentComponent(accumulator.getDeclaringComponent());
        c2.setTemporaryOutputState(StandardNames.XSL_ACCUMULATOR_RULE);
        try {
            return ExpressionTool.eagerEvaluate(delta, c2);
        } catch (XPathException e) {
            // Save the error until the accumulator value is read
            return new ObjectValue<>(e);
        }
    }

    private static void setAtDepth(IndexedStack<Sequence> stack, int depth, Sequence value) {
        if (depth >= stack.size()) {
            stack.push(value);
        } else {
            stack.set(depth, value);
        }
    }

    public boolean isBeingComputed(AccumulatorFn.Phase phase) {
        return beingComputed == phase;
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item the node that was matched.
     */

    @Override
    public void processItem(Item item) throws XPathException {
        //System.err.println("AccumulatorWatch.processItem()");
        FleetingNode node = (FleetingNode) item;
        if (node.getNodeKind() == Type.ATTRIBUTE) {
            return;
        }
        ManualIterator iter = (ManualIterator) context.getCurrentIterator();
        iter.setContextItem(item);
        iter.incrementPosition();

        int nodeNr = node.getNodeNumber();
        if (lastPredescentNode != null && nodeNr == lastPredescentNode.getNodeNumber()) {
            // We have already processed this event
            return;
        }
        lastPredescentNode = node;
        Rule rule = preDescentMode.getRule(item, context);
        if (rule != null) {
            beingComputed = AccumulatorFn.Phase.BEFORE;
            currentValue = applyRule(node, rule);
            beingComputed = AccumulatorFn.Phase.UNSPECIFIED;
            if (accumulator.isTracing()) {
                context.getConfiguration().getLogger().info(accumulator.getAccumulatorName().getDisplayName() +
                                                                    " BEFORE " + Navigator.getPath(node) + ": " + Err.depictSequence(currentValue));
            }
        }
        int depth = nodeStack.size();
        setAtDepth(preDescentStack, depth, currentValue);
        rule = postDescentMode.getRule(item, context);
        if (rule != null) {
            beingComputed = AccumulatorFn.Phase.AFTER;
            currentValue = applyRule(node, rule);
            beingComputed = AccumulatorFn.Phase.UNSPECIFIED;
            if (accumulator.isTracing()) {
                context.getConfiguration().getLogger().info(accumulator.getAccumulatorName().getDisplayName() +
                                                                    " AFTER " + Navigator.getPath(node) + ": " + Err.depictSequence(currentValue));
            }
        }
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the parsing event
     * @throws net.sf.saxon.type.ValidationException May be raised if the constraint implemented by this
     *                                               Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        FleetingNode node = nodeStack.peek();
//        System.err.println("AccumulatorWatch.endSelectedParentNode(" +
//                                   accumulator.getAccumulatorName() + "," + node.getDisplayName());
        int nodeNr = node.getNodeNumber();
        if (lastPostdescentNode != null && nodeNr == lastPostdescentNode.getNodeNumber()) {
            // We have already processed this event
            return;
        }
        lastPostdescentNode = node;
        Rule rule = postDescentMode.getRule(node, context);
        if (rule != null) {
            beingComputed = AccumulatorFn.Phase.AFTER;
            currentValue = applyRule(node, rule);
            beingComputed = AccumulatorFn.Phase.UNSPECIFIED;
            if (accumulator.isTracing()) {
                context.getConfiguration().getLogger().info(accumulator.getAccumulatorName().getDisplayName() +
                                                                    " AFTER " + Navigator.getPath(node) + ": " + Err.depictSequence(currentValue));
            }
        }
    }

    @Override
    public void close() throws XPathException {
        //
    }

    public Sequence getPreDescentValue(FleetingNode node) throws XPathException {
        if (lastPredescentNode == null || node.getNodeNumber() > lastPredescentNode.getNodeNumber()) {
            // we haven't yet processed this node (accumulators are allowed to reference each other)
            if (node instanceof FleetingParentNode) {
                startSelectedParentNode((FleetingParentNode) node, Loc.NONE);
            } else {
                processItem(node);
            }
        }
        detectErrorValue(currentValue);
        Sequence value = preDescentStack.get(node.getDepth());
        detectErrorValue(value);
        return value;
    }

    public Sequence getPreDescentValueAtDepth(int depth) throws XPathException {
        Sequence value = preDescentStack.get(depth);
        detectErrorValue(value);
        return value;
    }

    public Sequence getPostDescentValue(FleetingNode node) throws XPathException {
        if (node == null) {
            // happens while copying accumulators
            node = nodeStack.peek();
        }
        if (lastPostdescentNode == null || node.getNodeNumber() > lastPostdescentNode.getNodeNumber()) {
            // we haven't yet processed this node (accumulators are allowed to reference each other)
            if (node instanceof FleetingParentNode) {
                endSelectedParentNode(Loc.NONE);
            } else {
                processItem(node);
            }
        }
        Sequence value = currentValue;
        detectErrorValue(value);
        return value;
    }


}

// Copyright (c) 2012-2022 Saxonica Limited
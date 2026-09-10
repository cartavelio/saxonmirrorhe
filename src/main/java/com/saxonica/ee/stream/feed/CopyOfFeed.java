package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.StreamingSequenceWriter;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.AccumulatorWatch;
import com.saxonica.ee.stream.watch.MultiAccumulatorWatch;
import com.saxonica.ee.stream.watch.Trigger;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.validate.CatchingReceiver;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.expr.accum.AccumulatorManager;
import net.sf.saxon.expr.accum.IAccumulatorData;
import net.sf.saxon.expr.instruct.CopyOf;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.UnfailingErrorListener;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XsltController;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.z.IntHashMap;

import javax.xml.transform.TransformerException;
import java.util.HashMap;
import java.util.Map;

/**
 * This class is used to construct a grounded copy of a streamed node, by capturing the events representing the
 * streamed node as they are notified, and constructing the grounded copy from these events.
 */


public class CopyOfFeed extends AbsorptionFeed {

    private WatchManager watchManager;
    private boolean copyNamespaces = true;
    private boolean copyAccumulators = true;
    private int validation = Validation.PRESERVE;
    private SchemaType type = null;
    private XPathException validationError;
    private SequenceReceiver proxy;

    /**
     * Create a Watch whose effect is to create copies of the selected (streamed) nodes and pass
     * them to the supplied feed
     *
     * @param result    The feed to which the copied nodes should be supplied
     * @param selection defines the nodes to be selected
     * @param wm        The WatchManager that will notify the selected nodes
     * @param context   the XPath evaluation context
     * @return the new Watch
     */

    public static Trigger getWatch(Feed result, Pattern selection, WatchManager wm, XPathContext context) {
        CopyOfFeed action = new CopyOfFeed(wm, result, context);
        Trigger watch = new Trigger(selection, action, context);
        watch.setPipelineConfiguration(wm.getPipelineConfiguration());
        return watch;
    }

    public CopyOfFeed(WatchManager watchManager, Feed result, XPathContext context) {
        super(result, context);
        this.watchManager = watchManager;

        // proxy is a Receiver that accepts decomposed output from subexpressions (for example,
        // another copy-of() expression used as an operand - see test case si-copy-of-026). In principle
        // we could avoid composing the output of such a subexpression into the form of an Item, but it's
        // an edge case and proved tricky to achieve in practice (8/2014). Note that the output of the subexpression
        // can't bypass this copy operation, because this copy operation might be stripping namespaces or performing
        // validation.
        proxy = new StreamingSequenceWriter(getPipelineConfiguration(), result);
    }

    @Override
    public void close() throws XPathException {
        //System.err.println("CopyOfFeed.close()");
        if (hasFailed()) {
            if (validationError != null) {
                //if (!validationError.hasBeenReported()) {
                dynamicError(validationError);
                //}
            } else {
                dynamicError(new XPathException("Streamed copy-of has failed. A validation error, perhaps?)"));
            }
        }
        super.close();
    }

    /**
     * Say whether unused namespaces are to be copied
     *
     * @param copy true if unused namespace declarations should be copied to the result tree
     *             (this is the default
     */

    public void setCopyNamespaces(boolean copy) {
        this.copyNamespaces = copy;
    }

    /**
     * Say whether accumulators are to be copied
     *
     * @param copy true if unused namespace declarations should be copied to the result tree
     *             (this is the default
     */

    public void setCopyAccumulators(boolean copy) {
        this.copyAccumulators = copy;
    }

    /**
     * Set the options for schema validation
     *
     * @param validation the validation mode, e.g. strict, lax, preserve, strip
     * @param type       the type to validate against
     */

    public void setValidationOptions(int validation, SchemaType type) {
        this.validation = validation;
        this.type = type;
    }

    /**
     * Get a receiver to which start and end events for documents and elements can be
     * written. By default this will be the same as the receiver for the result feed,
     * because very few XSLT constructs (specifically, variables and functions) allow
     * the results of document and element constructors to be captured for further
     * processing. A construct that does need to process these events will intercept
     * them by providing its own receiver, typically a SequenceOutputter.
     */
    @Override
    public SequenceReceiver getReceiver() {
        return proxy;
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) {
        return new PendingCopyAction(sequenceNr, node);
    }

    @Override
    protected Sequence<?> processGroundedItem(Item item) throws XPathException {
        final Location loc = ExplicitLocation.UNKNOWN_LOCATION;
        if (item instanceof NodeInfo) {
            int kind = ((NodeInfo) item).getNodeKind();
            if (kind == Type.ATTRIBUTE) {
                NodeInfo node = (NodeInfo) item;
                Configuration config = getContext().getConfiguration();
                Orphan orphan = new Orphan(config);
                orphan.setNodeKind((short) node.getNodeKind());
                orphan.setNodeName(NameOfNode.makeName(node));
                orphan.setStringValue(item.getStringValue());
                orphan.setTypeAnnotation(node.getSchemaType());
                if (validation != Validation.PRESERVE || type != null) {
                    if (type != null && !type.isSimpleType()) {
                        throw new XPathException("When copying an attribute with schema validation, the requested type must not be a complex type", "XTTE1535", getExpression().getLocation());
                    }
                    SchemaType annotation = CopyOf.validateAttribute(node, (SimpleType) type, validation, getContext());
                    orphan.setTypeAnnotation(annotation);
                }
                return orphan;
            } else if (kind == Type.ELEMENT || kind == Type.DOCUMENT) {
                if (!copyNamespaces) {
                    SequenceOutputter writer = new SequenceOutputter(getPipelineConfiguration());
                    ((NodeInfo) item).copy(writer, 0, loc);
                    return writer.getFirstItem();
                }
                if (validation != Validation.PRESERVE || type != null) {
                    NodeInfo node = (NodeInfo) item;
                    ParseOptions validationOptions = new ParseOptions();
                    validationOptions.setSchemaValidationMode(validation);
                    validationOptions.setTopLevelType(type);
                    Configuration config = getPipelineConfiguration().getConfiguration();
                    SequenceOutputter writer = new SequenceOutputter(getPipelineConfiguration());
                    Receiver copier;
                    if (kind == Type.DOCUMENT) {
                        copier = config.getDocumentValidator(writer, node.getBaseURI(), validationOptions, loc);
                    } else {
                        validationOptions.setTopLevelElement(NameOfNode.makeName(node).getStructuredQName());
                        copier = config.getElementValidator(writer, validationOptions, loc);
                    }
                    node.copy(copier, CopyOptions.ALL_NAMESPACES, loc);
                    return writer.getFirstItem();
                }
            }
        }
        return item;
    }

    /**
     * Report a dynamic error. This requires searching up the "feed" pipeline to see
     * if there is a try/catch expression interested in being notified of this error;
     * if there is none, the error is thrown as an exception
     *
     * @param error the exception representing the dynamic error
     * @throws net.sf.saxon.trans.XPathException if there is no try/catch expression wanting to catch this error
     */
    @Override
    public void dynamicError(XPathException error) throws XPathException {
        super.dynamicError(error);
        proxy = new Sink(getPipelineConfiguration());
    }


    /**
     * A pending item represents a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private class PendingCopyAction extends PendingItem {
        private TinyBuilder builder;
        private Receiver destination;
        private FleetingParentNode rootOfCopy;

        public PendingCopyAction(int sequence, FleetingParentNode node) {
            super(sequence);
            this.rootOfCopy = node;
            PipelineConfiguration pipe = getPipelineConfiguration();
            if (valuesReadyToBeOutput.isEmpty() &&
                (getResult() instanceof DecomposingFeed || getResult() instanceof ComplexNodeEventFeed)) {
                Receiver r = getResult().getReceiver();
                destination = new ProxyReceiver(r) {
                    @Override
                    public void open() {
                        // no action
                    }

                    @Override
                    public void close() {
                        //no action
                    }
                };

            } else {
                // In theory the above technique ought to work whatever kind of result feed we have.
                // But in practice most ItemFeeds aren't good at accepting decomposed events as input,
                // and in any case expressions such as deep-equal() need to compose the input into items anyway,
                // so in this case copy-of() composes the events into items and sends the completed items
                // to the next feed in the pipeline. See for example test case sf-deep-equal-007.

                // For nested elements we always build the subtree. In principle for
                // grandchild nested elements we could use a virtual subtree of the tree
                // built for the child, but it's a rare case so not worth optimizing.
                builder = new TinyBuilder(pipe);
                builder.setStatistics(pipe.getConfiguration().getTreeStatistics().TEMPORARY_TREE_STATISTICS);
                destination = builder;
                if (copyAccumulators) {
                    destination = new AccumulatorCopier(watchManager, builder, builder);
                }
            }
        }

        /**
         * Get a receiver which will be notified of all events in the streamed input between the start event
         * for this node and the corresponding end event. This Receiver gathers data into the pending item
         * so that the final result of processing the streamed node can be computed by the deliver() method
         *
         * @return a receiver to be notified of all events relating to this streamed input node
         * @throws XPathException if a dynamic error is detected
         */

        @Override
        public Receiver getGatherer() throws XPathException {
            Receiver copier = destination;
            if (!copyNamespaces) {
                copier = new NamespaceReducer(copier);
                copier = new ProxyReceiver(copier) {
                    @Override
                    public void namespace(NamespaceBindingSet namespaceBindings, int properties) {
                        // do nothing
                    }
                };
            }
            if (validation != Validation.PRESERVE || type != null) {
                ParseOptions validationOptions = new ParseOptions();
                validationOptions.setSchemaValidationMode(validation);
                validationOptions.setTopLevelType(type);
                Configuration config = getPipelineConfiguration().getConfiguration();
                Location loc = getExpression() == null ? ExplicitLocation.UNKNOWN_LOCATION : getExpression().getLocation();
                if (rootOfCopy.getNodeKind() == Type.DOCUMENT) {
                    copier = config.getDocumentValidator(copier, rootOfCopy.getBaseURI(), validationOptions, loc);
                } else {
                    if (!(copier instanceof SequenceReceiver)) {
                        copier = new TreeReceiver(copier);
                    }
                    validationOptions.setTopLevelElement(NameOfNode.makeName(rootOfCopy).getStructuredQName());
                    try {
                        copier = config.getElementValidator(copier, validationOptions, loc);
                    } catch (XPathException e) {
                        dynamicError(e);
                        return null;
                    }
                }
                PipelineConfiguration newPipe = new PipelineConfiguration(getPipelineConfiguration());
                newPipe.setErrorListener(new UnfailingErrorListener() {

                    private int errorCount = 0;

                    public void warning(TransformerException exception) {
                        //
                    }

                    public void error(TransformerException exception) {
                        if (errorCount++ == 0) {
                            try {
                                //noinspection ThrowableResultOfMethodCallIgnored
                                final XPathException xe = XPathException.makeXPathException(exception);
                                validationError = xe;
                                dynamicError(xe);
                            } catch (XPathException e) {
                                //
                            }
                        }
                    }

                    public void fatalError(TransformerException exception) {
                        if (errorCount++ == 0) {
                            try {
                                //noinspection ThrowableResultOfMethodCallIgnored
                                final XPathException xe = XPathException.makeXPathException(exception);
                                validationError = xe;
                                dynamicError(xe);
                            } catch (XPathException e) {
                                //
                            }
                        }
                    }
                });
                copier.setPipelineConfiguration(newPipe);
                copier = new CatchingReceiver(copier, newPipe.getErrorListener());
            }
            return copier;
        }


        @Override
        public Sequence<?> deliver() throws XPathException {
            if (hasFailed()) {
                if (validationError != null) {
                    throw validationError;
                } else {
                    throw new XPathException("Streamed copy-of has failed. Perhaps a validation error?");
                }
            }
            if (builder != null) {
                return builder.getCurrentRoot();
            } else {
                return EmptySequence.getInstance();
            }
        }

        @Override
        public void rollback() {
        }
    }

    public static class AccumulatorCopier extends ProxyReceiver {

        private WatchManager watchManager;
        private TinyBuilder builder;
        private Map<Accumulator, CopiedAccumulatorData> dataMap = new HashMap<>();

        public AccumulatorCopier(WatchManager watchManager, Receiver next, TinyBuilder builder) {
            super(next);
            this.builder = builder;
            this.watchManager = watchManager;
            final MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
            if (multiAccumulatorWatch != null) {
                Map<Accumulator, AccumulatorWatch> accumulators = multiAccumulatorWatch.getAccumulatorMap();
                for (AccumulatorWatch watch : accumulators.values()) {
                    dataMap.put(watch.getAccumulator(), new CopiedAccumulatorData(watch.getAccumulator()));
                }
            }
        }

        int depth = 0;

        @Override
        public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
            //System.err.println("CopyOfFeed.startElement(" + elemName.getDisplayName() + ")");
            super.startElement(elemName, typeCode, location, properties);
            depth = watchManager.getDepth();
        }

        @Override
        public void endElement() throws XPathException {
            //System.err.println("CopyOfFeed.endElement(" + stack.pop().getDisplayName() + ")");
            super.endElement();
            depth--;
            TinyNodeImpl newNode = builder.getLastCompletedElement();
            final MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
            if (multiAccumulatorWatch != null) {
                Map<Accumulator, AccumulatorWatch> accumulators = multiAccumulatorWatch.getAccumulatorMap();
                for (AccumulatorWatch watch : accumulators.values()) {
                    Accumulator acc = watch.getAccumulator();
                    CopiedAccumulatorData data = dataMap.get(acc);
//                    System.err.println("Copying acc data for " + acc.getAccumulatorName().getDisplayName() +
//                        " for " + newNode.getDisplayName() + " = " + watch.getPreDescentValue(watch.getLastPredescentNode()));
                    data.addValues(newNode, watch.getPreDescentValueAtDepth(depth), watch.getPostDescentValue(null));
                }
            }
        }

        @Override
        public void close() throws XPathException {
            Controller controller = watchManager.getPipelineConfiguration().getController();
            if (controller instanceof XsltController) {
                AccumulatorManager manager = ((XsltController)controller).getAccumulatorManager();
                TreeInfo tree = builder.getTree();
                final MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
                if (multiAccumulatorWatch != null) {
                    Map<Accumulator, AccumulatorWatch> accumulatorWatchMap = multiAccumulatorWatch.getAccumulatorMap();
                    for (Accumulator acc : accumulatorWatchMap.keySet()) {
                        manager.addAccumulatorData(tree, acc, dataMap.get(acc));
                    }
                }
                for (CopiedAccumulatorData data : dataMap.values()) {
                    manager.addAccumulatorData(tree, data.getAccumulator(), data);
                }
            }
        }
    }

    private static class CopiedAccumulatorData implements IAccumulatorData {

        private Accumulator accumulator;
        private IntHashMap<Sequence<?>> preDescentValues = new IntHashMap<>();
        private IntHashMap<Sequence<?>> postDescentValues = new IntHashMap<>();

        public CopiedAccumulatorData(Accumulator acc) {
            this.accumulator = acc;
        }

        public void addValues(TinyNodeImpl node, Sequence<?> preDescent, Sequence<?> postDescent) {
            preDescentValues.put(node.getNodeNumber(), preDescent);
            postDescentValues.put(node.getNodeNumber(), postDescent);
        }

        public Accumulator getAccumulator() {
            return accumulator;
        }

        public Sequence<?> getValue(NodeInfo node, boolean postDescent) {
            int nodeNr = ((TinyNodeImpl)node).getNodeNumber();
            return postDescent ? postDescentValues.get(nodeNr) : preDescentValues.get(nodeNr);
        }
    }
}

// Copyright (c) 2005-2014 Saxonica Limited

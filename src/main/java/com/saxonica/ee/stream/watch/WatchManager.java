package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.SnapshotFeed;
import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.*;

import java.util.*;


/**
 * This class sits on a push pipeline and evaluates the passing element and attribute
 * nodes. One or more Watch objects can be registered with the pipeline at any time; element
 * and attribute events are notified to the active Watch objects. A watch that is added immediately
 * after a startElement call is automatically dropped at the corresponding endElement call. If
 * a Watch matches a startElement call, then it becomes an active Watch; it is automatically deactivated
 * (but not dropped) at the corresponding endElement call.
 * <p>A Watch is not itself a receiver, but it can nominate a Receiver to receive all events while it is
 * active. It will stop receiving events when it is deactivated.</p>
 */

public class WatchManager extends SequenceReceiver implements NamespaceResolver {

    private final static boolean TRACING = false;

    private boolean startDocumentPending;
    private int startDocumentProperties;
    private final FleetingDocumentNode documentNode;

    //private StartTagBuffer startTagBuffer;

    private FleetingParentNode[] elementStack = new FleetingParentNode[20];
    // stack holding element nodes. If the document node is watched, then the zeroth entry is the document node

    private int[] receiverCountStack = new int[20];
    // stack holding, for each nested element, the number of Receivers
    // on the activeReceiverStack that were allocated at that level of nesting

    protected int elementStackTop = 0;
    // stacktop for the preceding stacks. During processing of startElement and endElement events
    // for the outermost element that is actually processed, this has the value zero; during
    // processing of the contents of that element (including attributes) it has the value 1, and
    // so on.

    private Stack<NamespaceMap> namespaceStack = new Stack<>();

    protected List<Watch> watchList = new ArrayList<>(10);
    // list of Watch objects interested in seeing what comes along...

    protected Stack<List<ClosedownAction>> activeWatchStack = new Stack<>();
    // stack of Watch objects that have been activated. One entry for each level of element nesting;
    // the entry holds a list of Watch objects that were activated at that level of nesting

    private final Stack<Receiver> activeReceiverStack = new Stack<>();
    // stack of Receiver objects that are listening to events. The number of recievers added at each
    // level of element nesting is recorded in receiverCountStack

    private Location lastLocationId = Loc.NONE;

    private XPathContext context;
    private Outputter outputter;

    //private Map<Accumulator, AccumulatorWatch> accumulatorWatchMap;
    private MultiAccumulatorWatch multiAccumulatorWatch;

    public static class GroupingScope {
        public boolean isActive = true;
        public List<Watch> watches = new ArrayList<>();

        public void addWatch(Watch watch) {
            if (isActive) {
                watches.add(watch);
            }
        }
    }

    private final List<GroupingScope> capturingScopes = new ArrayList<>();

    // sequence number to identify the current event
    private long eventNumber = 0;

    @CSharpReplaceBody(code="")
    private void trace(String message) {
        if (TRACING) {
            System.err.println("WM " + Integer.toHexString(hashCode()) + " " + eventNumber + " " + message);
        }
    }



    /**
     * Create a WatchManager
     * @param pipe the pipeline configuration
     */

    public WatchManager(PipelineConfiguration pipe) {
        super(pipe);
        Configuration config = pipe.getConfiguration();
        context = config.getConversionContext();
        long docNr = config.getDocumentNumberAllocator().allocateStreamedDocumentNumber();
        documentNode = new FleetingDocumentNode(docNr);
        documentNode.setConfiguration(config);
        pipe.setComponent("com.saxonica.ee.stream.watch.WatchManager", this);

    }

    public void setXPathContext(XPathContext context) {
        this.context = context;
    }

    public XPathContext getXPathContext() {
        return context;
    }

    public void setOutputter(Outputter outputter) {
        outputter.setPipelineConfiguration(getPipelineConfiguration());
        this.outputter = outputter;

    }

    public Outputter getOutputter() {
        return outputter;
    }

    /**
     * Append an arbitrary item (node or atomic value) to the output
     * @param item           the item to be appended
     * @param locationId     the location of the calling instruction, for diagnostics
     * @param copyNamespaces if the item is an element node, this indicates whether its namespaces
*                       need to be copied. Values are {@link ReceiverOption#ALL_NAMESPACES};
     *                            the default (0) means
     */
    @Override
    public void append(Item item, Location locationId, int copyNamespaces) throws XPathException {
        throw new UnsupportedOperationException("WatchManager does not implement append()");
    }

    /**
     * Notify the end of the event stream
     *
     * @throws net.sf.saxon.trans.XPathException
     *          if an error occurs
     */
    @Override
    public void close() throws XPathException {
        //No action
    }

    /**
     * Start of a document node.
     * @param properties properties of the document node
     */

    @Override
    public void startDocument(int properties) throws XPathException {
        // Don't start the document immediately, because we want unparsed entities to be available while
        // processing the document node
        //("startDocument");
        if (elementStackTop == 0) {
            startDocumentPending = true;
            startDocumentProperties = properties;
            namespaceStack = new Stack<>();
        }
        eventNumber++;
    }

    private void deferredStartDocument() throws XPathException {

        documentNode.setSystemId(getSystemId());
        //documentNode.setAccumulatorWatchMap(accumulatorWatchMap);
        documentNode.setMultiAccumulatorWatch(multiAccumulatorWatch);
        elementStack[elementStackTop] = documentNode;
        receiverCountStack[elementStackTop] = 0;
        elementStackTop++;

        List<ClosedownAction> activeWatchList = new ArrayList<>(4);
        activeWatchStack.push(activeWatchList);

        //noinspection ForLoopReplaceableByForEach
        for (int i = 0; i < watchList.size(); i++) {
            // NB: watchList is modified within the loop
            Watch watch = watchList.get(i);
            boolean matches = watch.matchesNode(documentNode, context);
            if (matches) {
                ClosedownAction action = new ClosedownAction(watch, ClosedownAction.DEACTIVATE);
                activeWatchList.add(action);
                Receiver out = watch.startSelectedParentNode(documentNode, Loc.NONE);
                if (out != null) {
                    out.open();
                    activeReceiverStack.push(out);
                    receiverCountStack[elementStackTop - 1]++;
                }
            }
        }


        for (Receiver out : activeReceiverStack) {
            copyUnparsedEntities(out);
            out.startDocument(startDocumentProperties);
        }

        documentNode.copyInitialCommentsAndProcessingInstructions(this);


    }

    /**
     * Copy the unparsed entities notified on this stream to a registered output receiver
     * @param out the output receiver
     * @throws XPathException if something goes wrong
     */

    private void copyUnparsedEntities(Receiver out) throws XPathException {
        if (documentNode != null) {
            Iterator<String> names = documentNode.getUnparsedEntityNames();
            while (names.hasNext()) {
                String name = names.next();
                String[] entity = documentNode.getUnparsedEntity(name);
                out.setUnparsedEntity(name, entity[0], entity[1]);
            }
        }
    }

    /**
     * Output element start tag
     */

    @Override
    public void startElement(NodeName elementName, SchemaType typeCode,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        trace("startElement " + elementName);
        try {
            if (startDocumentPending) {
                startDocumentPending = false;
                deferredStartDocument();
            }
            ValidationException ve = null;
            ensureStackSpace();

            FleetingElementNode element = new FleetingElementNode(elementName, typeCode);
            FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
            element.setParent(parent);
            element.setNamespaceMap(namespaces);
            element.setAttributes(attributes);
            element.setHasChildren(ReceiverOption.contains(properties, ReceiverOption.HAS_CHILDREN));

            elementStack[elementStackTop] = element;
            receiverCountStack[elementStackTop] = 0;
            namespaceStack.push(namespaces);
            elementStackTop++;

            List<ClosedownAction> activeWatchList = new ArrayList<>(4);
            activeWatchStack.push(activeWatchList);

            addPrearrangedWatches();

            //noinspection ForLoopReplaceableByForEach
            for (int i = 0; i < watchList.size(); i++) {
                // NB: watchList is modified within the loop
                Watch watch = watchList.get(i);
                boolean matches = watch.matchesNode(element, context);
                if (matches) {
                    //System.err.println("WatchManager matches");
                    // The order here is important. We add the closedown action to the list now, because
                    // activating the watch may have the side-effect of adding further closedown actions to
                    // the list, and we want those to come after the closedown action for this watch.
                    ClosedownAction action = new ClosedownAction(watch, ClosedownAction.DEACTIVATE);
                    activeWatchList.add(action);
                    Receiver out = watch.startSelectedParentNode(element, location);
                    if (out != null) {
                        out.open();
                        activeReceiverStack.push(out);
                        receiverCountStack[elementStackTop - 1]++;
                    }
                }
            }

            for (Receiver out : activeReceiverStack) {
                try {
                    out.startElement(elementName, typeCode, attributes, namespaces, location, properties);
                } catch (ValidationException err) {
                    ve = err;
                }
            }

            if (ve != null) {
                throw ve;
            }

            lastLocationId = location.saveLocation();
        } catch (QuitParsingException quit) {
            if (multiAccumulatorWatch == null) {
                while (!namespaceStack.isEmpty()) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw quit;
            }
        }

        processAttributes(attributes);
        eventNumber++;
    }

    /**
     * During schema validation, the startElement() event is emitted before we can
     * examine all the streamed assertions that might apply to that element. So this
     * method can be used to process a Watch against the most recent start element event,
     * on the basis that we might have missed it the first time round.
     */

    public void restartElement(Watch watch)
            throws XPathException {
        //trace("startElement " + elementName);
        try {
            if (startDocumentPending) {
                startDocumentPending = false;
                deferredStartDocument();
            }
            ValidationException ve = null;
            ensureStackSpace();

            FleetingElementNode element = (FleetingElementNode)elementStack[elementStackTop - 1];
            List<ClosedownAction> activeWatchList = activeWatchStack.peek();

            boolean matches = watch.matchesNode(element, context);
            if (matches) {
                //System.err.println("WatchManager matches");
                // The order here is important. We add the closedown action to the list now, because
                // activating the watch may have the side-effect of adding further closedown actions to
                // the list, and we want those to come after the closedown action for this watch.
                ClosedownAction action = new ClosedownAction(watch, ClosedownAction.DEACTIVATE);
                activeWatchList.add(action);
                Receiver out = watch.startSelectedParentNode(element, element);
                if (out != null) {
                    out.open();
                    try {
                        out.startElement(element.getNodeName(), element.getSchemaType(),
                                         element.getAttributes(), element.getAllNamespaces(), lastLocationId, 0);
                    } catch (ValidationException err) {
                        ve = err;
                    }
                    activeReceiverStack.push(out);
                    receiverCountStack[elementStackTop - 1]++;
                }
            }

            if (ve != null) {
                throw ve;
            }

        } catch (QuitParsingException quit) {
            while (!namespaceStack.isEmpty()) {
                endElement();
            }
            endDocument();
            warning(quit);
            throw quit;
        }

    }


    private void processAttributes(AttributeMap attributes) throws XPathException {
        for (AttributeInfo attribute : attributes) {
            try {
                FleetingNode att = null;
                NodeName nameCode = attribute.getNodeName();
                //trace("attribute " + nameCode.getDisplayName());
                for (Watch watch : watchList) {
                    if (watch.matchesNodesOfKind(UType.ATTRIBUTE)) {
                        if (att == null) {
                            att = new FleetingNode();
                            att.setNodeKind(Type.ATTRIBUTE);
                            att.setNodeName(nameCode);
                            att.setTypeAnnotation(attribute.getType());
                            att.setParent(elementStack[elementStackTop - 1]);
                            att.setStringValue(StringView.tidy(attribute.getValue()));
                        }
                        boolean matches = watch.matchesNode(att, context);
                        if (matches) {
                            watch.processItem(att);
                        }
                    }
                }

                lastLocationId = attribute.getLocation();
            } catch (QuitParsingException quit) {
                while (!namespaceStack.isEmpty()) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw quit;
            }
        }
    }

    public int getDepth() {
        return elementStackTop;
    }

    private void ensureStackSpace() {
        if (elementStackTop + 1 > elementStack.length) {
            elementStack = Arrays.copyOf(elementStack, elementStackTop*2);
            receiverCountStack = Arrays.copyOf(receiverCountStack, elementStackTop*2);
        }
    }

    public FleetingParentNode getCurrentNode() {
        return elementStack[elementStackTop - 1];
    }

    public long getCurrentEventNumber() {
        return eventNumber;
    }

    protected void addPrearrangedWatches() throws XPathException {
        // No action: exists for the benefit of subclasses
    }

    /**
     * Text node
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {

        try {
            //trace("text: " + Err.wrap(chars));
            ensureStackSpace();

            FleetingNode node = null;
            FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;

            for (Watch watch : watchList) {
                if (watch.matchesNodesOfKind(UType.TEXT)) {  // optimization: most patterns match element nodes only
                    if (node == null) {
                        node = new FleetingNode();
                        node.setNodeKind(Type.TEXT);
                        node.setParent(parent);
                        node.setStringValue(chars.tidy());
                        node.setTypeAnnotation(BuiltInAtomicType.UNTYPED_ATOMIC);
                    }
                    boolean matches = watch.matchesNode(node, context);
                    if (matches) {
                        watch.processItem(node);
                    }
                }
            }

            for (Receiver out : activeReceiverStack) {
                out.characters(chars, locationId, properties);
            }

            //nextReceiver.characters(chars, locationId, properties);
            lastLocationId = locationId;
        } catch (QuitParsingException quit) {
            while (!namespaceStack.isEmpty()) {
                endElement();
            }
            endDocument();
            warning(quit);
            throw quit;
        }
        eventNumber++;
    }

    private void warning(QuitParsingException exception) {
        XmlProcessingException err = new XmlProcessingException(exception);
        err.setWarning(true);
        getPipelineConfiguration().getErrorReporter().report(err);
    }

    /**
     * Notify end of element
     */

    @Override
    public void endElement() throws XPathException {
        //trace("endElement " + elementStack[elementStackTop - 1].getDisplayName());
        for (Receiver out : activeReceiverStack) {
            out.endElement();
        }

        elementStackTop--;

        // undeclare namespaces declared at this level
        if (!namespaceStack.isEmpty()) { // can happen during recovery from early exit
            namespaceStack.pop();
        }

        // drop any receivers that were registered at this level

        int drop = receiverCountStack[elementStackTop];
        receiverCountStack[elementStackTop] = 0;
        for (int i = 0; i < drop; i++) {
            Receiver out = activeReceiverStack.pop();
            out.close();
        }

        // deactivate/close any watches that need to be actioned at this level, in reverse order

        List<ClosedownAction> activeWatchList = activeWatchStack.pop();
        QuitParsingException quit = null;
        
        // Process the closedown actions in three phases: first any snapshot feeds established using saxon:capture,
        // then the accumulator (phase=end) rules, then anything else
        int[] doItWhen = new int[activeWatchList.size()];
        for (int i=0; i< activeWatchList.size(); i++) {
            ClosedownAction action = activeWatchList.get(i);
            if (action.watch instanceof Trigger && ((Trigger) action.watch).getAction() instanceof SnapshotFeed) {
                doItWhen[i] = 0;
            } else if (action.watch instanceof MultiAccumulatorWatch) {
                doItWhen[i] = 1;
            } else {
                doItWhen[i] = 2;
            }
        }
        for (int phase = 0; phase <= 2; phase++) {
            for (int i = activeWatchList.size() - 1; i >= 0; i--) {
                ClosedownAction action = activeWatchList.get(i);
                if (doItWhen[i] == phase) {
                    if (action.action == ClosedownAction.DEACTIVATE) {
                        try {
                            action.watch.endSelectedParentNode(lastLocationId);
                        } catch (QuitParsingException q) {
                            quit = q;
                        }
                    } else {
                        try {
                            action.watch.close();
                        } catch (QuitParsingException q) {
                            quit = q;
                        }
                        watchList.remove(action.watch);
                    }
                }
            }
        }

        if (quit != null) {
            while (!namespaceStack.isEmpty()) {     // but see bug #4493
                endElement();
            }
            endDocument();
            warning(quit);
            throw quit;
        }
        eventNumber++;

    }

    /**
     * Notify the end of a document node
     */

    @Override
    public void endDocument() throws XPathException {
        //trace("endDocument");
        elementStackTop--;

        if (elementStackTop == 0) {
            for (Receiver out : activeReceiverStack) {
                out.endDocument();
            }

            // drop any receivers that were registered at this level

            int drop = receiverCountStack[elementStackTop];
            receiverCountStack[elementStackTop] = 0;
            for (int i = 0; i < drop; i++) {
                Receiver out = activeReceiverStack.pop();
                out.close();
            }

            // deactivate/close any watches that need to be actioned at this level, in reverse order

            List<ClosedownAction> activeWatchList = activeWatchStack.pop();
            for (int i = activeWatchList.size() - 1; i >= 0; i--) {
                ClosedownAction action = activeWatchList.get(i);
                if (action.action == ClosedownAction.DEACTIVATE) {
                    action.watch.endSelectedParentNode(lastLocationId);
                } else {
                    action.watch.close();
                    watchList.remove(action.watch);
                }
            }
        }
        eventNumber++;
        //nextReceiver.endDocument();
    }

    /**
     * Output a comment
     */

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {

        ensureStackSpace();

        FleetingNode node = new FleetingNode();
        FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
        node.setNodeKind(Type.COMMENT);
        node.setParent(parent);
        node.setStringValue(chars.tidy());
        node.setTypeAnnotation(BuiltInAtomicType.UNTYPED_ATOMIC);

        if (startDocumentPending) {

            documentNode.captureInitialCommentOrProcessingInstruction(node);

        } else {

            for (Watch watch : watchList) {
                boolean matches = watch.matchesNode(node, context);
                if (matches) {
                    watch.processItem(node);
                }
            }

            for (Receiver out : activeReceiverStack) {
                out.comment(chars, locationId, properties);
            }

            lastLocationId = locationId;
        }
        eventNumber++;
    }

    /**
     * Processing Instruction
     */

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        ensureStackSpace();

        FleetingNode node = new FleetingNode();
        FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
        node.setNodeKind(Type.PROCESSING_INSTRUCTION);
        node.setParent(parent);
        NoNamespaceName nodeName = new NoNamespaceName(target);
        node.setNodeName(nodeName);
        node.setStringValue(data.tidy());
        node.setTypeAnnotation(BuiltInAtomicType.UNTYPED_ATOMIC);

        if (startDocumentPending) {

            documentNode.captureInitialCommentOrProcessingInstruction(node);

        } else {

            for (Watch watch : watchList) {
                boolean matches = watch.matchesNode(node, context);
                if (matches) {
                    watch.processItem(node);
                }
            }

            for (Receiver out : activeReceiverStack) {
                out.processingInstruction(target, data, locationId, properties);
            }

            lastLocationId = locationId;
        }
        eventNumber++;
    }

    /**
     * Notify an unparsed entity URI.
     *
     * @param name     The name of the unparsed entity
     * @param systemID The system identifier of the unparsed entity
     * @param publicID The public identifier of the unparsed entity
     */

    @Override
    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        if (documentNode != null) {
            documentNode.setUnparsedEntity(name, systemID, publicID);
        }
    }

    /**
     * Add a new Watch. This is associated with the current level in the element stack, and
     * is automatically closed and dropped when the current element goes out of scope.
     *
     * @param watch the watch to be added
     * @param autoClose set to true if the watch is to be automatically opened as soon as it is registered,
     *                  and is to be automatically closed when it goes out of scope.
     * @throws XPathException if any error occurs
     */

    public void addWatch(Watch watch, boolean autoClose) throws XPathException {
        watchList.add(watch);
        watch.setPipelineConfiguration(getPipelineConfiguration());
        watch.setNamespaceResolver(this);
//        if (!groupingScopeStack.isEmpty() && groupingScopeStack.peek().isActive) {
//            groupingScopeStack.peek().addWatch(watch);
//            watch.setAnchorNode(elementStack[elementStackTop - 1]);
//        } else
        boolean foundGroupCapture = false;
        if (!capturingScopes.isEmpty()) {
            for (GroupingScope scope : capturingScopes) {
                if (scope.isActive) {
                    scope.addWatch(watch);
                    watch.setAnchorNode(elementStack[elementStackTop - 1]);
                    foundGroupCapture = true;
                }
            }
        }
        if (!foundGroupCapture && elementStackTop > 0) {
            watch.setAnchorNode(elementStack[elementStackTop - 1]);
            if (autoClose) {
                activeWatchStack.peek().add(new ClosedownAction(watch, ClosedownAction.CLOSE));
            }
        }
        if (autoClose) {
            watch.open(Terminator.getInstance());
        }
    }

    /**
     * Remove a watch
     * @param watch the watch to be removed. No error occurs if the watch is unknown or inactive.
     */

    public void removeWatch(Watch watch) {
        watchList.remove(watch);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Implement the NamespaceResolver interface
    ///////////////////////////////////////////////////////////////////////////////

    /**
     * Get the namespace URI corresponding to a given prefix. Return null
     * if the prefix is not in scope.
     *
     * @param prefix     the namespace prefix. May be the zero-length string, indicating
     *                   that there is no prefix. This indicates either the default namespace or the
     *                   null namespace, depending on the value of useDefault.
     * @param useDefault true if the default namespace is to be used when the
     *                   prefix is "". If false, the method returns "" when the prefix is "".
     * @return the uri for the namespace, or null if the prefix is not in scope.
     *         The "null namespace" is represented by the pseudo-URI "".
     */

    /*@Nullable*/
    @Override
    public NamespaceUri getURIForPrefix(String prefix, boolean useDefault) {
        if (!useDefault && prefix.isEmpty()) {
            return NamespaceUri.NULL;
        }
        return namespaceStack.peek().getURIForPrefix(prefix, useDefault);
    }

    @Override
    public Iterator<String> iteratePrefixes() {
        return namespaceStack.peek().iteratePrefixes();
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    @Override
    public boolean usesTypeAnnotations() {
        return true;
    }

    public boolean allowsEarlyExit() {
        return multiAccumulatorWatch == null;
    }

    public FleetingDocumentNode getDocumentNode() {
        return documentNode;
    }

    public MultiAccumulatorWatch getMultiAccumulatorWatch() {
        return multiAccumulatorWatch;
    }

    public void watchAccumulators(MultiAccumulatorWatch multiWatch) throws XPathException {
        this.multiAccumulatorWatch = multiWatch;
        addWatch(multiWatch, true);
    }

    protected static class ClosedownAction {
        public static final int DEACTIVATE = 0;
        public static final int CLOSE = 1;
        public Watch watch;
        public int action;

        public ClosedownAction(Watch watch, int action) {
            this.watch = watch;
            this.action = action;
        }
    }

    /**
     * Called during a partitioning for-each-group whenever a new group is started
     */

    public GroupingScope startGroupingScope() {
        return new GroupingScope();
    }

    /**
     * Called during a partitioning for-each-group whenever a group is closed
     * @throws XPathException if a failure occurs
     */

    public void endGroupingScope(GroupingScope current) throws XPathException {
        for (Watch watch : current.watches) {
            watch.close();
            removeWatch(watch);
        }
    }

    public void startCapturingGroupingWatches(GroupingScope scope) {
        capturingScopes.add(scope);
    }

    public void stopCapturingGroupingWatches(GroupingScope scope) {
        capturingScopes.remove(scope);
    }

}

// Copyright (c) 2004-2022 Saxonica Limited

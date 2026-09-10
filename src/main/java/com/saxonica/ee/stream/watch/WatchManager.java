package com.saxonica.ee.stream.watch;

import com.saxonica.ee.stream.feed.SnapshotFeed;
import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.AttributeCollectionImpl;
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
    private FleetingDocumentNode documentNode;

    private StartTagBuffer startTagBuffer;

    private FleetingParentNode[] elementStack = new FleetingParentNode[20];
    // stack holding element nodes. If the document node is watched, then the zeroth entry is the document node

    private int[] receiverCountStack = new int[20];
    // stack holding, for each nested element, the number of Receivers
    // on the activeReceiverStack that were allocated at that level of nesting

    private int[] namespaceCountStack = new int[20];
    // stack holding, for each nested element, the number of namespaces declared or undeclared
    // on that element start tag

    protected int elementStackTop = 0;
    // stacktop for the preceding stacks. During processing of startElement and endElement events
    // for the outermost element that is actually processed, this has the value zero; during
    // processing of the contents of that element (including attributes) it has the value 1, and
    // so on.

    private NamespaceBinding[] namespaceStack = new NamespaceBinding[20];
    // stack holding the current namespace declarations and undeclarations, as namespace codes

    private int namespaceStackTop = 0;
    // stacktop for the namespace stack

    protected List<Watch> watchList = new ArrayList<>(10);
    // list of Watch objects interested in seeing what comes along...

    protected Stack<List<ClosedownAction>> activeWatchStack = new Stack<>();
    // stack of Watch objects that have been activated. One entry for each level of element nesting;
    // the entry holds a list of Watch objects that were activated at that level of nesting

    private Stack<Receiver> activeReceiverStack = new Stack<>();
    // stack of Receiver objects that are listening to events. The number of recievers added at each
    // level of element nesting is recorded in receiverCountStack

    private Location lastLocationId = ExplicitLocation.UNKNOWN_LOCATION;

    private XPathContext context;

    //private Map<Accumulator, AccumulatorWatch> accumulatorWatchMap;
    private MultiAccumulatorWatch multiAccumulatorWatch;

    public static class GroupingScope {
        public boolean isActive = true;
        public List<Watch> watches = new ArrayList<>();

        private void addWatch(Watch watch) {
            if (isActive) {
                watches.add(watch);
            }
        }
    }

    private List<GroupingScope> capturingScopes = new ArrayList<>();

    private void trace(String message) {
        if (TRACING) {
            System.err.println("WM " + Integer.toHexString(hashCode()) + " " + message);
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

    }

    public void setStartTagBuffer(StartTagBuffer stb) {
        this.startTagBuffer = stb;
    }

    public StartTagBuffer getStartTagBuffer() {
        return startTagBuffer;
    }

    public void setXPathContext(XPathContext context) {
        this.context = context;
    }

    public XPathContext getXPathContext() {
        return context;
    }

    /**
     * Append an arbitrary item (node or atomic value) to the output
     *
     * @param item           the item to be appended
     * @param locationId     the location of the calling instruction, for diagnostics
     * @param copyNamespaces if the item is an element node, this indicates whether its namespaces
     *                       need to be copied. Values are {@link ReceiverOptions#ALL_NAMESPACES},
     *                       {@link ReceiverOptions#LOCAL_NAMESPACES}; the default (0) means
     *                       no namespaces
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
    public void close() throws XPathException {
        //No action
    }

    /**
     * Start of a document node.
     */

    public void startDocument(int properties) throws XPathException {
        // Don't start the document immediately, because we want unparsed entities to be available while
        // processing the document node
        startDocumentPending = true;
        startDocumentProperties = properties;
    }

    private void deferredStartDocument() throws XPathException {

        documentNode.setSystemId(getSystemId());
        //documentNode.setAccumulatorWatchMap(accumulatorWatchMap);
        documentNode.setMultiAccumulatorWatch(multiAccumulatorWatch);
        elementStack[elementStackTop] = documentNode;
        receiverCountStack[elementStackTop] = 0;
        namespaceCountStack[elementStackTop++] = 0;

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
                Receiver out = watch.startSelectedParentNode(documentNode, ExplicitLocation.UNKNOWN_LOCATION);
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

    public void startElement(NodeName elementName, SchemaType typeCode, Location location, int properties)
            throws XPathException {
        //trace("startElement " + elementName);
        if (startDocumentPending) {
            startDocumentPending = false;
            deferredStartDocument();
        }

        try {
            ValidationException ve = null;
            ensureStackSpace();

            FleetingElementNode element = new FleetingElementNode(elementName, typeCode);
            FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
            element.setParent(parent);
            element.setLocalNamespaces(startTagBuffer.getLocalNamespaces());
            element.setAttributes(AttributeCollectionImpl.copy(
                    (AttributeCollectionImpl) startTagBuffer.getAllAttributes()));
            element.setHasChildren((properties & ReceiverOptions.HAS_CHILDREN) != 0);

            elementStack[elementStackTop] = element;
            receiverCountStack[elementStackTop] = 0;
            namespaceCountStack[elementStackTop++] = 0;

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

            final int totalReceivers = activeReceiverStack.size();
            for (Receiver out : activeReceiverStack) {
                try {
                    out.startElement(elementName, typeCode, location, properties);
                } catch (ValidationException err) {
                    ve = err;
                }
            }

            // Newly-added receivers need to catch up on namespaces declared at outer levels
            final int newReceivers = receiverCountStack[elementStackTop - 1];
            for (int i = totalReceivers - newReceivers; i < totalReceivers; i++) {
                Receiver out = activeReceiverStack.get(i);
                for (int n = 0; n < namespaceStackTop; n++) {
                    out.namespace(namespaceStack[n], 0);
                }
            }

            // Now pass the event to the next thing in the pipeline
            //nextReceiver.startElement(elementName, typeCode, locationId, properties);
            if (ve != null) {
                throw ve;
            }

            lastLocationId = location.saveLocation();
        } catch (QuitParsingException quit) {
            while (!activeWatchStack.isEmpty()) {
                endElement();
            }
            getPipelineConfiguration().getErrorListener().warning(quit);
            throw quit;
        }
    }

    public int getDepth() {
        return elementStackTop;
    }

    private void ensureStackSpace() {
        if (elementStackTop + 1 > elementStack.length) {
            elementStack = Arrays.copyOf(elementStack, elementStackTop*2);
            receiverCountStack = Arrays.copyOf(receiverCountStack, elementStackTop*2);
            namespaceCountStack = Arrays.copyOf(namespaceCountStack, elementStackTop*2);
        }
    }

    protected void addPrearrangedWatches() throws XPathException {
        // No action: exists for the benefit of subclasses
    }

    /**
     * Notify an attribute
     *
     * @param nameCode   integer identifying the name of the attribute
     * @param typeCode   integer identifying the type annotation (ignored)
     * @param value      the value of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     *@param properties additional properties of the attribute  @throws net.sf.saxon.trans.XPathException
     *          On any failure to write the attribute
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties)
            throws XPathException {

        try {
            FleetingNode att = null;
            //trace("attribute " + nameCode.getDisplayName());
            for (Watch watch : watchList) {
                if (watch.matchesNodesOfKind(UType.ATTRIBUTE)) {
                    if (att == null) {
                        att = new FleetingNode();
                        att.setNodeKind(Type.ATTRIBUTE);
                        att.setNodeName(nameCode);
                        att.setTypeAnnotation(typeCode);
                        att.setParent(elementStack[elementStackTop - 1]);
                        att.setStringValue(value);
                    }
                    boolean matches = watch.matchesNode(att, context);
                    if (matches) {
                        watch.processItem(att);
                    }
                }
            }

            for (Receiver out : activeReceiverStack) {
                out.attribute(nameCode, typeCode, value, locationId, properties);
            }

            //nextReceiver.attribute(nameCode, typeCode, value, locationId, properties);
            lastLocationId = locationId;
        } catch (QuitParsingException quit) {
            while (!activeWatchStack.isEmpty()) {
                endElement();
            }
            getPipelineConfiguration().getErrorListener().warning(quit);
            throw quit;
        }
    }

    /**
     * Notify a namespace
     *
     * @param namespaceBindings the prefix/uri pair being notified
     * @param properties       properties of this event
     * @throws XPathException if any error occurs
     */

    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {

        for (NamespaceBinding ns : namespaceBindings) {
            namespaceCountStack[elementStackTop - 1]++;
            if (namespaceStackTop >= namespaceStack.length) {
                namespaceStack = Arrays.copyOf(namespaceStack, namespaceStackTop*2);
            }
            namespaceStack[namespaceStackTop++] = ns;
        }

        for (Receiver out : activeReceiverStack) {
            out.namespace(namespaceBindings, properties);
        }
    }

    /**
     * Text node
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {

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
                        node.setStringValue(chars.toString());
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
            while (!activeWatchStack.isEmpty()) {
                endElement();
            }
            getPipelineConfiguration().getErrorListener().warning(quit);
            throw quit;
        }
    }

    /**
     * Notify end of element
     */

    public void endElement() throws XPathException {
        //trace("endElement");
        for (Receiver out : activeReceiverStack) {
            out.endElement();
        }

        elementStackTop--;

        // undeclare namespaces declared at this level
        namespaceStackTop -= namespaceCountStack[elementStackTop];

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
            while (!activeWatchStack.isEmpty()) {
                endElement();
            }
            //getPipelineConfiguration().getErrorListener().warning(quit);
            throw quit;
        }

    }

    /**
     * Notify the end of a document node
     */

    public void endDocument() throws XPathException {
        //trace("endDocument");
        for (Receiver out : activeReceiverStack) {
            out.endDocument();
        }

        elementStackTop--;

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

        //nextReceiver.endDocument();
    }

    /**
     * Output a comment
     */

    public void comment(CharSequence chars, Location locationId, int properties) throws XPathException {

        ensureStackSpace();

        FleetingNode node = new FleetingNode();
        FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
        node.setNodeKind(Type.COMMENT);
        node.setParent(parent);
        node.setStringValue(chars.toString());
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

    }

    /**
     * Processing Instruction
     */

    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) throws XPathException {
        ensureStackSpace();

        FleetingNode node = new FleetingNode();
        FleetingParentNode parent = elementStackTop > 0 ? elementStack[elementStackTop - 1] : documentNode;
        node.setNodeKind(Type.PROCESSING_INSTRUCTION);
        node.setParent(parent);
        NoNamespaceName nodeName = new NoNamespaceName(target);
        node.setNodeName(nodeName);
        node.setStringValue(data.toString());
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
    }

    /**
     * Start element content (called after all the attributes and namespaces have been processed)
     *
     * @throws XPathException if a failure occurs
     */

    public void startContent() throws XPathException {
        for (Receiver out : activeReceiverStack) {
            out.startContent();
        }
        //nextReceiver.startContent();
    }

    /**
     * Notify an unparsed entity URI.
     *
     * @param name     The name of the unparsed entity
     * @param systemID The system identifier of the unparsed entity
     * @param publicID The public identifier of the unparsed entity
     */

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
    public String getURIForPrefix(String prefix, boolean useDefault) {
        if (!useDefault && prefix.isEmpty()) {
            return NamespaceConstant.NULL;
        }
        for (int i = namespaceStackTop - 1; i >= 0; i--) {
            if (namespaceStack[i].getPrefix().equals(prefix)) {
                //int uriCode = namespaceStack[i] & 0xffff;
                String uri = namespaceStack[i].getURI();
                if (uri.isEmpty()) {
                    if (prefix.isEmpty()) {
                        // default namespace has been undeclared
                        return "";
                    } else {
                        // specific prefix has been undeclared
                        return null;
                    }
                } else {
                    return uri;
                }
            }
        }
        return prefix.isEmpty() ? NamespaceConstant.NULL : null;
    }

    public Iterator<String> iteratePrefixes() {
        Set<String> declaredPrefixes = new HashSet<>(5);
        HashSet<String> undeclaredPrefixes = new HashSet<>(5);
        for (int i = namespaceStackTop - 1; i >= 0; i--) {
            String prefixCode = namespaceStack[i].getPrefix();
            String uriCode = namespaceStack[i].getURI();
            if (uriCode.isEmpty()) {
                undeclaredPrefixes.add(prefixCode);
            } else {
                if (!undeclaredPrefixes.contains(prefixCode)) {
                    declaredPrefixes.add(namespaceStack[i].getPrefix());
                }
            }
        }
        return declaredPrefixes.iterator();
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     *         may supply untyped nodes instead of supplying the type annotation
     */

    public boolean usesTypeAnnotations() {
        return true;
    }

    public FleetingDocumentNode getDocumentNode() {
        return documentNode;
    }

    /**
     * Get a map from accumulators to AccumulatorWatch instances
     * @return the map for all accumulators that apply to this stream. Return null if there are no
     * accumulators
     */

    //    /**
//     * Get a map from accumulators to AccumulatorWatch instances
//     * @return the map for all accumulators that apply to this stream. Return null if there are no
//     * accumulators
//     */
//
//    public Map<Accumulator, AccumulatorWatch> getAccumulatorWatchMap() {
//        return accumulatorWatchMap;
//    }
//
//    public void setAccumulatorWatchMap(Map<Accumulator, AccumulatorWatch> accumulatorWatchMap) {
//        this.accumulatorWatchMap = accumulatorWatchMap;
//    }
    public MultiAccumulatorWatch getMultiAccumulatorWatch() {
        return multiAccumulatorWatch;
    }

    public void watchAccumulators(MultiAccumulatorWatch multiWatch) throws XPathException {
        this.multiAccumulatorWatch = multiWatch;
        addWatch(multiWatch, true);
    }

    private static class ClosedownAction {
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

// Copyright (c) 2004-2018 Saxonica Limited.

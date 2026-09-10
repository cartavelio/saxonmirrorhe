package com.saxonica.ee.domino;

import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMSender;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import org.w3c.dom.Document;

import java.util.Arrays;

/**
 * This class is instantiated to build a DOMINO tree, taking a DOM Document as input; its task
 * is to add the indexing information.
 */
public class Erector implements Receiver {

    private Configuration config;
    private Document domDocument;
    private DOMSender sender;
    private DominoNode currentRoot;
    private NamePool namePool;
    private String systemId;

    public static final int PARENT_POINTER_INTERVAL = 10;
    // a lower value allocates more parent pointers which takes more space but reduces
    // the length of parent searches

    /*@Nullable*/ private DominoTree tree;

    private int currentDepth = 0;
    private int nodeNr = 0;             // this is the local sequence within this document
    private boolean ended = false;

    private final static int NULL_FP = NamePool.FP_MASK;


    /*@NotNull*/ private int[] prevAtDepth = new int[100];
    // this array is scaffolding used while constructing the tree, it is
    // not present in the final tree. For each level of the tree, it records the
    // node number of the most recent node at that level.

    /*@NotNull*/ private int[] siblingsAtDepth = new int[100];
    // more scaffolding. For each level of the tree, this array records the
    // number of siblings processed at that level. When this exceeds a threshold value,
    // a dummy node is inserted into the arrays to contain a parent pointer: this it to
    // prevent excessively long searches for a parent node, which is normally found by
    // scanning the siblings. The value is then reset to zero.


    public Erector(Document doc, String systemId, Configuration config) {
        sender = new DOMSender(doc, this);
        sender.setSystemId(systemId);
        this.setSystemId(systemId);
        this.domDocument = doc;
        this.config = config;
        this.namePool = config.getNamePool();
    }

    public DominoTree erect() throws XPathException {
        sender.send();
        tree.setSystemId(systemId);
        return tree;
    }

    /**
     * Get the tree being built by this builder
     *
     * @return the TinyTree
     */

    /*@Nullable*/
    public DominoTree getTree() {
        return tree;
    }

    /**
     * Get the current depth in the tree
     *
     * @return the current depth
     */

    public int getCurrentDepth() {
        return currentDepth;
    }

    /**
     * Open the event stream
     */

    public void open() {
        if (tree == null) {
            tree = new DominoTree(config);
            tree.setSystemId(systemId);
            currentDepth = 0;
        }
    }

    /**
     * Write a document node to the tree
     */

    public void startDocument(int properties) throws XPathException {

        if (tree == null) {
            open();
        }
        DominoTree tt = tree;
        assert tt != null;
        currentRoot = new DominoNode(tt, 0);
        DominoNode doc = currentRoot;
        doc.setSystemId(getSystemId());

        currentDepth = 0;

        int nodeNr = tt.addNode(Type.DOCUMENT, 0, -1, domDocument);
        prevAtDepth[0] = nodeNr;
        prevAtDepth[1] = -1;
        siblingsAtDepth[0] = 0;
        siblingsAtDepth[1] = 0;
        tt.next[nodeNr] = -1;

        currentDepth++;
    }

    /**
     * Callback interface for SAX: not for application use
     */

    public void endDocument() throws XPathException {
//        System.err.println("TinyBuilder: " + this + " End document");

        // Add a stopper node to ensure no-one walks off the end of the array; but
        // decrement numberOfNodes so the next node will overwrite it
        tree.addNode(Type.STOPPER, 0, -1, null);
        tree.numberOfNodes--;

        if (currentDepth > 1) {
            return;
        }
        // happens when copying a document node as the child of an element

        if (ended) {
            return;  // happens when using an IdentityTransformer
        }
        ended = true;

        prevAtDepth[currentDepth] = -1;
        currentDepth--;

    }

    public void reset() {
        tree = null;
        currentDepth = 0;
        nodeNr = 0;
        ended = false;
    }

    public void close() throws XPathException {
        DominoTree tt = tree;
        if (tt != null) {
            tt.addNode(Type.STOPPER, 0, -1, null);
            tt.condense();
        }
    }

    /**
     * Notify the start tag of an element
     */

    public void startElement(/*@NotNull*/ NodeName elemName, SchemaType type, Location location, int properties) throws XPathException {

        // if the number of siblings exceeds a certain threshold, add a parent pointer, in the form
        // of a pseudo-node

        DominoTree tt = tree;
        assert tt != null;

        boolean addParentPointer = false;
        if (siblingsAtDepth[currentDepth] > PARENT_POINTER_INTERVAL) {
            addParentPointer = true;
            siblingsAtDepth[currentDepth] = 0;
        }

        // now add the element node itself
        int fp = elemName.obtainFingerprint(namePool);
        if (addParentPointer) {
            fp |= DominoTree.HAS_PARENT_POINTER;
        }
        nodeNr = tt.addNode(Type.ELEMENT, currentDepth, fp, sender.getCurrentNode());
        if (addParentPointer) {
            tt.addParentPointer(nodeNr, prevAtDepth[currentDepth - 1]);
        }

        if (currentDepth == 0) {
            prevAtDepth[0] = nodeNr;
            prevAtDepth[1] = -1;
            currentRoot = tt.getNode(nodeNr);
        } else {
            int prev = prevAtDepth[currentDepth];
            if (prev > 0) {
                tt.next[prev] = nodeNr;
            }
            tt.next[nodeNr] = prevAtDepth[currentDepth - 1];   // *O* owner pointer in last sibling
            prevAtDepth[currentDepth] = nodeNr;
            siblingsAtDepth[currentDepth]++;
        }
        currentDepth++;

        if (currentDepth == prevAtDepth.length) {
            prevAtDepth = Arrays.copyOf(prevAtDepth, currentDepth*2);
            siblingsAtDepth = Arrays.copyOf(siblingsAtDepth, currentDepth*2);
        }
        prevAtDepth[currentDepth] = -1;
        siblingsAtDepth[currentDepth] = 0;

    }

    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
        assert tree != null;
        for (NamespaceBinding ns : namespaceBindings) {
            tree.addNamespace(nodeNr, ns);
            tree.nameCode[nodeNr] |= DominoTree.HAS_NAMESPACES;
        }
    }

    public void attribute(/*@NotNull*/ NodeName attName, SimpleType type, CharSequence value, Location locationId, int properties)
            throws XPathException {
        tree.nameCode[nodeNr] |= DominoTree.HAS_ATTRIBUTES;
        if ((properties & ReceiverOptions.IS_ID) != 0 || (attName.getLocalPart().equals("id") && attName.hasURI(NamespaceConstant.XML))) {
            tree.registerID(nodeNr, value.toString());
        }
    }

    public void startContent() {
        nodeNr++;
    }

    /**
     * Notify the end of an element node
     */

    public void endElement() throws XPathException {
        DominoTree tt = tree;
        assert tt != null;
//        System.err.println("End element");
        prevAtDepth[currentDepth] = -1;
        siblingsAtDepth[currentDepth] = 0;
        currentDepth--;
    }

    /**
     * Get the last completed element node. This is used during checking of schema assertions,
     * which happens while the tree is still under construction. It is also used when copying
     * accumulator values to the new tree from a streamed input. This method is called immediately after
     * a call on endElement(), and it returns the element that has just ended.
     *
     * @return the last completed element node, that is, the element whose endElement event is the most recent
     * endElement event to be reported, or null if there is no such element
     */

    public DominoNode getLastCompletedElement() {
        if (tree == null) {
            return null;
        }
        return tree.getNode(currentDepth >= 0 ? prevAtDepth[currentDepth] : 0);
        // Note: reading an incomplete tree needs care if it constructs a prior index, etc.
    }


    /**
     * Notify a text node
     */

    public void characters(/*@NotNull*/ CharSequence chars, Location locationId, int properties) throws XPathException {
//        if (chars instanceof CompressedWhitespace &&
//                (properties & ReceiverOptions.WHOLE_TEXT_NODE) != 0) {
//            DomIndex tt = tree;
//            assert tt != null;
//            long lvalue = ((CompressedWhitespace) chars).getCompressedValue();
//            nodeNr = tt.addNode(Type.WHITESPACE_TEXT, currentDepth, (int) (lvalue >> 32), (int) lvalue, -1);
//
//            int prev = prevAtDepth[currentDepth];
//            if (prev > 0) {
//                tt.next[prev] = nodeNr;
//            }
//            tt.next[nodeNr] = prevAtDepth[currentDepth - 1];   // *O* owner pointer in last sibling
//            prevAtDepth[currentDepth] = nodeNr;
//            siblingsAtDepth[currentDepth]++;
//            return;
//        }

        final int len = chars.length();
        if (len > 0) {
            nodeNr = makeTextNode(chars, len);
        }
    }

    /**
     * Create a text node. Separate method so it can be overridden. If the current node
     * on the tree is already a text node, the new text will be appended to it.
     *
     * @param chars the contents of the text node
     * @param len   the length of the text node
     * @return the node number of the created text node, or the text node to which
     * this text has been appended.
     */

    protected int makeTextNode(CharSequence chars, int len) {
        DominoTree tt = tree;

        int n = tt.numberOfNodes - 1;
        if (n >= 0 && tt.nodeKind[n] == Type.TEXT && tt.depth[n] == currentDepth) {
            // we have two adjacent text nodes. Don't add a new node; mark the existing one
            // as having a continuation
            tt.nameCode[n] |= DominoTree.HAS_CONTINUATION;
            return n;
        }
        nodeNr = tt.addNode(Type.TEXT, currentDepth, NULL_FP, sender.getCurrentNode());

        int prev = prevAtDepth[currentDepth];
        if (prev > 0) {
            tt.next[prev] = nodeNr;
        }
        tt.next[nodeNr] = prevAtDepth[currentDepth - 1];
        prevAtDepth[currentDepth] = nodeNr;
        siblingsAtDepth[currentDepth]++;

        return nodeNr;
    }

    /**
     * Callback interface for SAX: not for application use<BR>
     */

    public void processingInstruction(String piname, /*@NotNull*/ CharSequence remainder, Location locationId, int properties) throws XPathException {
        DominoTree tt = tree;
        int fp = namePool.allocateFingerprint("", piname);

        nodeNr = tt.addNode(Type.PROCESSING_INSTRUCTION, currentDepth, fp, sender.getCurrentNode());

        int prev = prevAtDepth[currentDepth];
        if (prev > 0) {
            tt.next[prev] = nodeNr;
        }
        tt.next[nodeNr] = prevAtDepth[currentDepth - 1];   // *O* owner pointer in last sibling
        prevAtDepth[currentDepth] = nodeNr;
        siblingsAtDepth[currentDepth]++;

    }

    /**
     * Callback interface for SAX: not for application use
     */

    public void comment(/*@NotNull*/ CharSequence chars, Location locationId, int properties) throws XPathException {

        DominoTree tt = tree;
        nodeNr = tt.addNode(Type.COMMENT, currentDepth, NULL_FP, sender.getCurrentNode());

        int prev = prevAtDepth[currentDepth];
        if (prev > 0) {
            tt.next[prev] = nodeNr;
        }
        tt.next[nodeNr] = prevAtDepth[currentDepth - 1];   // *O* owner pointer in last sibling
        prevAtDepth[currentDepth] = nodeNr;
        siblingsAtDepth[currentDepth]++;

    }

    /**
     * Set an unparsed entity in the document
     */

    public void setUnparsedEntity(String name, String uri, String publicId) {
        tree.addUnparsedEntity(name, uri, publicId);
    }


    /**
     * Get the pipeline configuration
     *
     * @return the pipeline configuration
     */
    @Override
    public PipelineConfiguration getPipelineConfiguration() {
        return null;
    }

    /**
     * Set the pipeline configuration
     *
     * @param pipe the pipeline configuration
     */
    @Override
    public void setPipelineConfiguration(PipelineConfiguration pipe) {

    }

    /**
     * Set the System ID of the tree represented by this event stream
     *
     * @param systemId the system ID (which is used as the base URI of the nodes
     *                 if there is no xml:base attribute)
     */
    @Override
    public void setSystemId(String systemId) {
        this.systemId = systemId;
        if (tree != null) {
            tree.setSystemId(systemId);
        }
    }

    /**
     * Ask whether this Receiver (or the downstream pipeline) makes any use of the type annotations
     * supplied on element and attribute events
     *
     * @return true if the Receiver makes any use of this information. If false, the caller
     * may supply untyped nodes instead of supplying the type annotation (or conversely, it may
     * avoid stripping unwanted type annotations)
     */
    @Override
    public boolean usesTypeAnnotations() {
        return false;
    }

    /**
     * Get the system identifier that was set with setSystemId.
     *
     * @return The system identifier that was set with setSystemId,
     * or null if setSystemId was not called.
     */
    @Override
    public String getSystemId() {
        return null;
    }
}

// Copyright (c) 2018 Saxonica Limited


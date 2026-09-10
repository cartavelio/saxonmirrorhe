package com.saxonica.ee.domino;

import net.sf.saxon.Configuration;
import net.sf.saxon.om.GenericTreeInfo;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.NodeListIterator;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntToIntHashMap;
import org.w3c.dom.Document;
import org.w3c.dom.Node;

import java.util.*;


/**
 * An adaptation of the TinyTree structure, used to index a DOM.
 * The data structure for a DOMINO tree consists of a set of arrays,
 * each containing one entry for every node other than attributes and
 * namespaces. These arrays contain for each node: the node kind, the
 * depth of the node in the tree, an integer fingerprint of the node
 * name, various bit-significant flags, and a reference to the node
 * in the DOM tree being indexed. All navigation is done using the
 * DOMINO index structures, but the DOM nodes are referenced to get
 * the actual data values, and a few other things like prefixes.
 * Attributes are accessed entirely from the DOM structure.
 *
 * @since 9.8
 */

public final class DominoTree extends GenericTreeInfo implements NodeVectorTree {

    /*@NotNull*/
    public static final int HAS_PARENT_POINTER = 1 << 24;
    public static final int HAS_ATTRIBUTES = 1 << 25;
    public static final int HAS_NAMESPACES = 1 << 26;
    public static final int HAS_CONTINUATION = 1 << 27;

    protected int numberOfNodes = 0;    // excluding attributes and namespaces

    // The following arrays contain one entry for each node other than attribute
    // and namespace nodes, arranged in document order.

    // nodeKind indicates the kind of node, e.g. element, text, or comment
    public byte[] nodeKind;

    // depth is the depth of the node in the hierarchy, i.e. the number of ancestors
    protected short[] depth;

    // next is the node number of the next sibling
    // - unless it points backwards, in which case it is the node number of the parent
    protected int[] next;

    // nameCode holds the name of the node, as an identifier resolved using the name pool
    protected int[] nameCode;

    // the prior array indexes preceding-siblings; it is constructed only when required
    /*@Nullable*/
    protected int[] prior = null;

    protected Node[] domNodes;

    // the namespaces array holds, for element nodes only, a reference to the in-scope namespaces of the element
    private NamespaceMap[] namespaces;

    // when a parent has a large number of children, we keep occasional links from children to parents to reduce searching
    private final IntToIntHashMap extraParentPointers = new IntToIntHashMap();

    // index of elements by name, populated lazily as the result of a search
    private IntHashMap<List<NodeInfo>> elementsByFingerprint;

    // index of unparsed entities. The key is the entity name, the values are the system Id and public Id
    private HashMap<String, String[]> unparsedEntities;

    // index of ID values
    private HashMap<String, Integer> idTable;


    /**
     * Create a tree (package-private constructor)
     *
     * @param config the Saxon configuration
     */

    DominoTree(Configuration config) {
        super(config);

        int nodes = 100;
        nodeKind = new byte[nodes];
        depth = new short[nodes];
        next = new int[nodes];
        nameCode = new int[nodes];
        namespaces = new NamespaceMap[nodes];
        domNodes = new Node[nodes];

        setConfiguration(config);
    }

    /**
     * Factory method to construct a Domino tree referencing a supplied DOM document
     *
     * @param doc      the DOM document
     * @param config   the Saxon configuration
     * @param systemId the system id (base URI) of the document
     * @return the Domino tree object
     * @throws XPathException in the event of failure (for example, if there is no Saxon-EE license)
     */

    public static DominoTree makeTree(Document doc, Configuration config, String systemId) throws XPathException {
        if (!(config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY) ||
                config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
            throw new XPathException("Use of Domino Tree model requires a Saxon-EE license");
        }
        synchronized(doc) {
            // DOM is not thread-safe
            Erector erector = new Erector(doc, systemId, config);
            return erector.erect();
        }
    }

    private void ensureNodeCapacity(short kind) {
        if (nodeKind.length < numberOfNodes + 1) {
            //System.err.println("Number of nodes = " + numberOfNodes);
            int k = kind == Type.STOPPER ? numberOfNodes + 1 : numberOfNodes * 2;
            nodeKind = Arrays.copyOf(nodeKind, k);
            next = Arrays.copyOf(next, k);
            depth = Arrays.copyOf(depth, k);
            nameCode = Arrays.copyOf(nameCode, k);
            domNodes = Arrays.copyOf(domNodes, k);
            namespaces = Arrays.copyOf(namespaces, k);

        }
    }

    /**
     * Add a node to the tree
     *
     * @param kind     The kind of the node. This must be a document, element, text, comment,
     *                 or processing-instruction node (not an attribute or namespace)
     * @param depth    The depth in the tree
     * @param nameCode The name of the node
     * @return the node number of the node that was added
     */
    int addNode(short kind, int depth, int nameCode, Node domNode) {

        ensureNodeCapacity(kind);
        nodeKind[numberOfNodes] = (byte) kind;
        this.depth[numberOfNodes] = (short) depth;
        this.domNodes[numberOfNodes] = domNode;
        this.nameCode[numberOfNodes] = nameCode;
        next[numberOfNodes] = -1;      // safety precaution

        if (numberOfNodes == 0) {
            setDocumentNumber(getConfiguration().getDocumentNumberAllocator().allocateDocumentNumber());
        }

        return numberOfNodes++;
    }
    
    @Override
    public boolean isTyped() {
        return false;
    }

    public void setCurrentNode(Node node) {
        domNodes[numberOfNodes - 1] = node;
    }

    public Node getDomNode(int nodeNr) {
        return domNodes[nodeNr];
    }

    void addParentPointer(int node, int parent) {
        extraParentPointers.put(node, parent);
    }

    /**
     * Condense the tree: release unused memory. This is done after the full tree has been built.
     * The method makes a pragmatic judgement as to whether it is worth reclaiming space; this is
     * only done when the constructed tree is very small compared with the space allocated.
     */

    void condense() {
        //System.err.println("TinyTree.condense() " + this + " roots " + rootIndexUsed + " nodes " + numberOfNodes + " capacity " + nodeKind.length);

        if (numberOfNodes * 3 < nodeKind.length ||
                (nodeKind.length - numberOfNodes > 20000)) {

            //System.err.println("-- copying node arrays");

            nodeKind = Arrays.copyOf(nodeKind, numberOfNodes);
            next = Arrays.copyOf(next, numberOfNodes);
            depth = Arrays.copyOf(depth, numberOfNodes);
            nameCode = Arrays.copyOf(nameCode, numberOfNodes);
            domNodes = Arrays.copyOf(domNodes, numberOfNodes);

        }
    }

    /**
     * Add an unparsed entity
     *
     * @param name     the name of the entity
     * @param systemId the system identifier of the entity
     * @param publicId the public identifier of the entity
     */

    void addUnparsedEntity(String name, String systemId, String publicId) {
        if (unparsedEntities == null) {
            unparsedEntities = new HashMap<>(4);
        }
        unparsedEntities.put(name, new String[]{systemId, publicId});
    }

    /**
     * Get the list of unparsed entities defined in this document
     *
     * @return an Iterator, whose items are of type String, containing the names of all
     * unparsed entities defined in this document. If there are no unparsed entities or if the
     * information is not available then an empty iterator is returned
     */

    @Override
    public Iterator<String> getUnparsedEntityNames() {
        if (unparsedEntities == null) {
            List<String> e = Collections.emptyList();
            return e.iterator();
        } else {
            return unparsedEntities.keySet().iterator();
        }
    }

    /**
     * Get the unparsed entity with a given name
     *
     * @param name the name of the entity
     * @return if the entity exists, return an array of two Strings, the first
     * holding the system ID of the entity (as an absolute URI if possible),
     * the second holding the public ID if there is one, or null if not.
     * If the entity does not exist, the method returns null.
     * Applications should be written on the assumption that this array may
     * be extended in the future to provide additional information.
     */

    @Override
    public String[] getUnparsedEntity(String name) {
        if (unparsedEntities == null) {
            return null;
        }
        return unparsedEntities.get(name);
    }

    /**
     * Get the type annotation of a node. Applies only to document, element, text,
     * processing instruction, and comment nodes.
     *
     * @param nodeNr the node whose type annotation is required
     * @return the fingerprint of the type annotation for elements and attributes, otherwise undefined.
     */

    public SchemaType getSchemaType(int nodeNr) {
        return Untyped.getInstance();
    }


    /**
     * Get the node kind of a given node, which must be a document, element,
     * text, comment, or processing instruction node
     *
     * @param nodeNr the node number
     * @return the node kind
     */

    @Override
    public int getNodeKind(int nodeNr) {
        return nodeKind[nodeNr];
    }

    /**
     * Get the fingerprint for a given node, which must be a document, element,
     * text, comment, or processing instruction node
     *
     * @param nodeNr the node number
     * @return the fingerprint, identifying the node name in the name pool
     */

    @Override
    public int getFingerprint(int nodeNr) {
        int fp = nameCode[nodeNr] & NamePool.FP_MASK;
        return fp == NamePool.FP_MASK ? -1 : fp;
    }

    /**
     * On demand, make an index for quick access to preceding-sibling nodes
     */

    void ensurePriorIndex() {
        if (prior == null) {
            makePriorIndex();
        }
    }

    private synchronized void makePriorIndex() {
        int[] p = new int[numberOfNodes];
        Arrays.fill(p, 0, numberOfNodes, -1);
        for (int i = 0; i < numberOfNodes; i++) {
            int nextNode = next[i];
            if (nextNode > i) {
                p[nextNode] = i;
            }
        }
        prior = p;
    }

    /**
     * Static method to get the parent of a given node, without instantiating the node as an object.
     * The starting node is any node other than an attribute or namespace node.
     *
     * @param nodeNr the node number of the starting node within the tree
     * @return the node number of the parent node, or -1 if there is no parent.
     */

    int getParentNodeNr(int nodeNr) {

        if (depth[nodeNr] == 0) {
            return -1;
        }

        // follow the next-sibling pointers until we reach a next sibling pointer that
        // points backwards, or one that has the PARENT_POINTER flag set
        int p = next[nodeNr];
        while (true) {
            if (p < nodeNr) {
                return p;
            }
            if ((nameCode[p] & HAS_PARENT_POINTER) != 0) {
                return extraParentPointers.get(p);
            }
            p = next[p];
        }
    }

    void setNamespaceMap(int element, NamespaceMap namespaceMap) {
        namespaces[element] = namespaceMap;
    }

    NamespaceMap getNamespaceMap(int nodeNr) {
        return namespaces[nodeNr];
    }

    /**
     * Get the node at a given position in the tree
     *
     * @param nr the node number
     * @return the node at the given position
     */

    @Override
    public final DominoNode getNode(int nr) {
        return new DominoNode(this, nr);
    }


    /**
     * Get the root node of the tree
     * @return the root node
     */

    @Override
    public DominoNode getRootNode() {
        if (getNodeKind(0) == Type.DOCUMENT) {
            if (root != null) {
                return (DominoNode)root;
            } else {
                root = new DominoNode(this, 0);
                return (DominoNode)root;
            }
        } else {
            return getNode(0);
        }
    }

    /**
     * Register a unique element ID. Fails if there is already an element with that ID.
     *
     * @param nodeNr The node number of the node having a particular unique ID value
     * @param id     The unique ID value. The caller is responsible for checking that this
     *               is a valid NCName.
     */

    void registerID(int nodeNr, String id) {
        if (idTable == null) {
            idTable = new HashMap<>(256);
        }

        // the XPath spec (5.2.1) says ignore the second ID if it's not unique
        idTable.putIfAbsent(id, nodeNr);

    }

    /**
     * Get the element with a given ID.
     *
     * @param id        The unique ID of the required element, previously registered using registerID()
     * @param getParent true if the required element is the parent of the element of type ID
     * @return The NodeInfo (always an Element) for the given ID if one has been registered,
     * otherwise null.
     */

    /*@Nullable*/
    @Override
    public DominoNode selectID(String id, boolean getParent) {
        if (idTable == null) {
            return null;            // no ID values found
        }
        Integer nodeNr = idTable.getOrDefault(id, -1);
        if (nodeNr >= 0) {
            DominoNode node = getNode(nodeNr);
            if (getParent) {
                node = node.getParent();
            }
            return node;
        } else {
            return null;
        }

    }

    /**
     * Get the NamePool used to map element and attribute names to fingerprints
     * @return the name pool
     */

    public NamePool getNamePool() {
        return getConfiguration().getNamePool();
    }

    /**
     * Get the number of nodes in the tree, excluding attributes and namespace nodes
     *
     * @return the number of nodes.
     */

    public int getNumberOfNodes() {
        return numberOfNodes;
    }

    /**
     * Get a list of all elements with a given name. This is implemented
     * as a memo function: the first time it is called for a particular
     * element type, it remembers the result for next time.
     *
     * @param fingerprint the fingerprint identifying the required element name
     * @return an iterator over all elements with this name
     */

    public synchronized AxisIterator getAllElements(int fingerprint) {
        if (elementsByFingerprint == null) {
            elementsByFingerprint = new IntHashMap<>(20);
        }
        List<NodeInfo> list = elementsByFingerprint.get(fingerprint);
        if (list == null) {
            list = makeElementList(fingerprint);
            elementsByFingerprint.put(fingerprint, list);
        }
        return new NodeListIterator(list);
    }

    /**
     * Get a list containing all the elements with a given element name
     *
     * @param fingerprint the fingerprint of the element name
     * @return list a List containing the TinyElementImpl objects
     */

    private List<NodeInfo> makeElementList(int fingerprint) {
        int size = getNumberOfNodes() / 20;
        if (size > 100) {
            size = 100;
        }
        if (size < 20) {
            size = 20;
        }
        ArrayList<NodeInfo> list = new ArrayList<>(size);
        int i = 1;
        try {
            while (depth[i] != 0) {
                if (nodeKind[i] == Type.ELEMENT &&
                        getFingerprint(i) == fingerprint) {
                    list.add(getNode(i));
                }
                i++;
            }
        } catch (ArrayIndexOutOfBoundsException e) {
            // this shouldn't happen. If it does happen, it means the tree wasn't properly closed
            // during construction (there is no stopper node at the end). In this case, we'll recover
            return list;
        }
        list.trimToSize();
        return list;
    }

    @Override
    public byte[] getNodeKindArray() {
        return nodeKind;
    }

    @Override
    public int[] getNameCodeArray() {
        return nameCode;
    }
}

// Copyright (c) 2017-2022 Saxonica Limited


package com.saxonica.ee.stream.om;

import com.saxonica.ee.stream.watch.MultiAccumulatorWatch;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * A Fleeting node (that is a transient node in a streamed document) that represents a document node
 */
public class FleetingDocumentNode extends FleetingParentNode implements TreeInfo {

    private Configuration config;
    private String systemId;
    private final long documentNumber;
    private int lastNodeNumber = 1;
    private HashMap<String, Object> userData;
    //private Map<Accumulator, AccumulatorWatch> accumulatorWatchMap;
    private MultiAccumulatorWatch multiAccumulatorWatch;
    private Map<String, String[]> unparsedEntities;
    private List<NodeInfo> initialCommentsAndProcessingInstructions;
    private SpaceStrippingRule spaceStrippingRule = NoElementsSpaceStrippingRule.getInstance();

    public FleetingDocumentNode(long docNr) {
        setNodeKind(Type.DOCUMENT);
        root = this;
        setSystemId("");
        this.documentNumber = docNr;
        setHasChildren(true); // because we only allow streaming of "real" documents
    }

    /**
     * Get the NodeInfo object representing the document node at the root of the tree
     *
     * @return the document node
     */

    @Override
    public NodeInfo getRootNode() {
        return this;
    }

    @Override
    public TreeInfo getTreeInfo() { return this; }

    protected int allocateNodeNumber() {
        return lastNodeNumber++;
    }

    /**
     * Get the document number, which identifies this tree uniquely within a Configuration
     *
     * @return the document number
     */
    @Override
    public long getDocumentNumber() {
        return documentNumber;
    }

    /**
     * Get the durability: always FLEETING
     * @return {@link Durability#FLEETING}
     */

    @Override
    public Durability getDurability() {
        return Durability.FLEETING;
    }

    /**
     * Set the configuration
     * @param config the configuration to which this document belongs
     */

    public void setConfiguration(Configuration config) {
        this.config = config;
    }

    /**
     * Notify an unparsed entity URI.
     *
     * @param name     The name of the unparsed entity
     * @param systemID The system identifier of the unparsed entity
     * @param publicID The public identifier of the unparsed entity
     */

    public void setUnparsedEntity(String name, String systemID, String publicID) throws XPathException {
        if (unparsedEntities == null) {
            unparsedEntities = new HashMap<String, String[]>();
        }
        unparsedEntities.put(name, new String[]{systemID, publicID});
    }

    /**
     * Buffer a comment or processing instruction appearing before the first element start tag
     * @param node a comment or processing instruction node
     */

    public void captureInitialCommentOrProcessingInstruction(NodeInfo node) {
        if (initialCommentsAndProcessingInstructions == null) {
            initialCommentsAndProcessingInstructions = new ArrayList<>(4);
        }
        initialCommentsAndProcessingInstructions.add(node);
    }

    /**
     * Copy any saved initial comments or processing instructions to a specified Receiver; and
     * remove them to release any memory that they occupy
     */

    public void copyInitialCommentsAndProcessingInstructions(Receiver out) throws XPathException {
        if (initialCommentsAndProcessingInstructions != null) {
            for (NodeInfo node : initialCommentsAndProcessingInstructions) {
                switch (node.getNodeKind()) {
                    case Type.COMMENT:
                        out.comment(node.getUnicodeStringValue(), Loc.NONE, ReceiverOption.NONE);
                        break;
                    case Type.PROCESSING_INSTRUCTION:
                        out.processingInstruction(node.getLocalPart(), node.getUnicodeStringValue(),
                                                  Loc.NONE, ReceiverOption.NONE);
                        break;
                    default:
                        throw new IllegalStateException();
                }
            }
        }
        initialCommentsAndProcessingInstructions = null;
    }

    /**
     * Get the element with a given ID, if any
     *
     * @param id        the required ID value
     * @param getParent true if the element required is the parent of the element whose type is xs:ID
     * @return the element with the given ID, or null if there is no such ID
     *         present (or if the parser has not notified attributes as being of
     *         type ID)
     */

    @Override
    public NodeInfo selectID(String id, boolean getParent) {
        return null;
    }

    /**
     * Get the typed value.
     *
     * @return the typed value. This will either be a single AtomicValue or a Value whose items are
     *         atomic values.
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return StringValue.makeUntypedAtomic(getUnicodeStringValue());
    }

    /**
     * Get the list of unparsed entities defined in this document
     *
     * @return an Iterator, whose items are of type String, containing the names of all
     *         unparsed entities defined in this document. If there are no unparsed entities or if the
     *         information is not available then an empty iterator is returned
     */

    @Override
    public Iterator<String> getUnparsedEntityNames() {
        if (unparsedEntities == null) {
            Collection<String> e = Collections.emptyList();
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
     *         holding the system ID of the entity (as an absolute URI if possible),
     *         the second holding the public ID if there is one, or null if not.
     *         If the entity does not exist, the method returns null.
     *         Applications should be written on the assumption that this array may
     *         be extended in the future to provide additional information.
     * @since 8.4
     */

    @Override
    public String[] getUnparsedEntity(String name) {
        if (unparsedEntities == null) {
            return null;
        } else {
            return unparsedEntities.get(name);
        }
    }

    /**
     * Set the system identifier for this Source.
     * <p>The system identifier is optional if the source does not
     * get its data from a URL, but it may still be useful to provide one.
     * The application can use a system identifier, for example, to resolve
     * relative URIs and to include in error messages and warnings.</p>
     *
     * @param systemId The system identifier as a URL string.
     */
    @Override
    public void setSystemId(String systemId) {
        this.systemId = systemId;
    }

    /**
     * Get the configuration used to build the tree containing this node.
     *
     * @return the Configuration
     * @since 8.4
     */

    @Override
    public Configuration getConfiguration() {
        return config;
    }

    /**
     * Get the NamePool that holds the namecode for this node
     *
     * @return the namepool
     * @since 8.4
     */

    @Override
    public NamePool getNamePool() {
        return config.getNamePool();
    }

    /**
     * Copy this node to a given Receiver.
     * <p>This method is primarily for internal use. It should not be considered a stable
     * part of the Saxon API.</p>
     *  @param out         the Receiver to which the node should be copied. It is the caller's
     *                    responsibility to ensure that this Receiver is open before the method is called
     *                    (or that it is self-opening), and that it is closed after use.
     * @param copyOptions Define how namespaces are handled
     * @param locationId  If non-zero, identifies the location of the instruction
     */

    @Override
    public void copy(Receiver out, int copyOptions, Location locationId) throws XPathException {
        throw new UnsupportedOperationException("copy() is not supported in streaming mode for document nodes");
    }

    /**
     * Get the System ID for the node. Note this is not the
     * same as the base URI: the base URI can be modified by xml:base, but
     * the system ID cannot. The base URI is used primarily for resolving
     * relative URIs within the content of the document. The system ID is
     * used primarily in conjunction with a line number, for identifying the
     * location of elements within the source XML, in particular when errors
     * are found. For a document node, the System ID represents the value of
     * the document-uri property as defined in the XDM data model.
     *
     * @return the System Identifier of the entity in the source document
     *         containing the node, or null if not known or not applicable.
     * @since 8.4
     */

    @Override
    public String getSystemId() {
        return systemId;
    }

    /**
     * Get the Base URI for the node, that is, the URI used for resolving a relative URI contained
     * in the node. This will be the same as the System ID unless xml:base has been used. Where the
     * node does not have a base URI of its own, the base URI of its parent node is returned.
     *
     * @return the base URI of the node. This may be null if the base URI is unknown.
     * @since 8.4
     */

    @Override
    public String getBaseURI() {
        return systemId;
    }

    public void setMultiAccumulatorWatch(MultiAccumulatorWatch multiWatch) {
        this.multiAccumulatorWatch = multiWatch;
    }

    public MultiAccumulatorWatch getMultiAccumulatorWatch() {
        return this.multiAccumulatorWatch;
    }

    /**
     * Set details of space stripping action that was applied to this document during
     * construction. This ensures that space stripping is not applied twice to the same
     * document.
     *
     * @param rule details of the space stripping rules that have been applied to this
     *             document during its construction.
     * @since 9.9
     */
    @Override
    public void setSpaceStrippingRule(SpaceStrippingRule rule) {
        this.spaceStrippingRule = rule;
    }

    /**
     * Get details of space stripping action that was applied to this document during
     * construction. This ensures that space stripping is not applied twice to the same
     * document.
     *
     * @return details of the space stripping rules that have been applied to this
     * document during its construction. By default, returns {@link NoElementsSpaceStrippingRule},
     * indicating that no space stripping has been applied
     * @since 9.9
     */
    @Override
    public SpaceStrippingRule getSpaceStrippingRule() {
        return spaceStrippingRule;
    }

    /**
     * Set user data on the document node. The user data can be retrieved subsequently
     * using {@link #getUserData}
     *
     * @param key   A string giving the name of the property to be set. Clients are responsible
     *              for choosing a key that is likely to be unique. Must not be null. Keys used internally
     *              by Saxon are prefixed "saxon:".
     * @param value The value to be set for the property. May be null, which effectively
     *              removes the existing value for the property.
     */

    @Override
    public void setUserData(String key, Object value) {
        if (userData == null) {
            userData = new HashMap<String, Object>(4);
        }
        if (value == null) {
            userData.remove(key);
        } else {
            userData.put(key, value);
        }
    }

    /**
     * Get user data held in the document node. This retrieves properties previously set using
     * {@link #setUserData}
     *
     * @param key A string giving the name of the property to be retrieved.
     * @return the value of the property, or null if the property has not been defined.
     */

    /*@Nullable*/
    @Override
    public Object getUserData(String key) {
        if (userData == null) {
            return null;
        } else {
            return userData.get(key);
        }
    }

    /**
     * Notify the event to a Receiver
     *
     * @param out the receiver to be notified
     */
    @Override
    public void notify(Receiver out) throws XPathException {
        out.startDocument(ReceiverOption.NONE);
    }
}

// Copyright (c) 2009-2023 Saxonica Limited




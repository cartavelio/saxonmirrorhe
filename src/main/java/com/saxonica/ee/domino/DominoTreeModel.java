package com.saxonica.ee.domino;

import net.sf.saxon.Configuration;
import net.sf.saxon.dom.ActiveDOMSource;
import net.sf.saxon.dom.DOMWriter;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.JPConverter;
import net.sf.saxon.expr.PJConverter;
import net.sf.saxon.lib.ExternalObjectModel;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.lib.ActiveSource;
import net.sf.saxon.trans.XPathException;
import org.w3c.dom.Document;
import org.w3c.dom.Node;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.dom.DOMSource;

/**
 * Tree model implementation for the DOMINO tree
 */
public class DominoTreeModel extends TreeModel implements ExternalObjectModel {

    private static final DominoTreeModel THE_INSTANCE = new DominoTreeModel();

    public static DominoTreeModel getInstance() {
        return THE_INSTANCE;
    }

    public DominoTreeModel() {
    }

    /**
     * Get the name of a characteristic class, which, if it can be loaded, indicates that the supporting libraries
     * for this object model implementation are available on the classpath
     *
     * @return by convention (but not necessarily) the class that implements a document node in the relevant
     * external model
     */
    @Override
    public String getDocumentClassName() {
        return "com.saxonica.ee.domino.DominoNode";
    }

    /**
     * Get the URI of the external object model as used in the JAXP factory interfaces for obtaining
     * an XPath implementation
     */

    @Override
    public String getIdentifyingURI() {
        return NamespaceConstant.OBJECT_MODEL_DOMINO;
    }

    @Override
    public Builder makeBuilder(PipelineConfiguration pipe) {
        return new DominoBuilder(pipe);
    }

    @Override
    public int getSymbolicValue() {
        return Builder.DOMINO_TREE;
    }

    @Override
    public String getName() {
        return "DOMINO";
    }


    /**
     * Give this ExternalObjectModel the opportunity of recognising a Source object and returning
     * an ActiveSource, which will be used to send an instance of this external model to a supplied
     * Receiver. The default implementation returns null.
     *
     * @param supplied a supplied Source
     * @return an ActiveSource object if the source is recognised, or null if not
     */
    @Override
    public ActiveSource getActiveSource(Source supplied) {
        if (supplied instanceof DOMSource) {
            return new ActiveDOMSource((DOMSource) supplied);
        }
        return null;
    }


    /**
     * A Builder class for the Domino tree model
     */

    public static class DominoBuilder extends DOMWriter {

        public DominoBuilder(PipelineConfiguration pipe) {
            super();
            setPipelineConfiguration(pipe);
        }
        /**
         * Get the current root node. This will normally be a document node, but if the root of the tree
         * is an element node, it can be an element.
         *
         * @return the root of the tree that is currently being built, or that has been most recently built
         * using this builder
         */
        @Override
        public NodeInfo getCurrentRoot() {
            try {
                Document root = getDOMDocumentNode();
                DominoTree tree = DominoTree.makeTree(root, getConfiguration(), getSystemId());
                return tree.getRootNode();
            } catch (XPathException e) {
                throw new AssertionError(e);
            }
        }
    }

    /**
     * Test whether this object model recognizes a particular kind of JAXP Result object,
     * and if it does, return a Receiver that builds an instance of this data model from
     * a sequence of events. If the Result is not recognised, return null.
     *
     * <p>This implementation always returns null (the Domino model is never used
     * for transformation or query results)</p>
     *
     * @param result a JAXP result object
     * @return a Receiver that writes to that result, if available; or null otherwise
     * @throws XPathException if any failure occurs
     */
    @Override
    public Receiver getDocumentBuilder(Result result) throws XPathException {
        return null;
    }

    /**
     * Get a converter from XPath values to values in the external object model
     *
     * @param targetClass the required class of the result of the conversion. If this class represents
     *                    a node or list of nodes in the external object model, the method should return a converter that takes
     *                    a native node or sequence of nodes as input and returns a node or sequence of nodes in the
     *                    external object model representation. Otherwise, it should return null.
     * @return a converter, if the targetClass is recognized as belonging to this object model;
     * otherwise null
     */
    @Override
    public PJConverter getPJConverter(Class<?> targetClass) {
        return null;
    }

    /**
     * Get a converter from values in the external object model to XPath values.
     *
     * @param sourceClass the class (static or dynamic) of values to be converted
     * @param config the Saxon Configuration
     * @return a converter, if the sourceClass is recognized as belonging to this object model;
     * otherwise null
     */
    @Override
    public JPConverter getJPConverter(Class sourceClass, Configuration config) {
        return null;
    }

    /**
     * Get a converter that converts a sequence of XPath nodes to this model's representation
     * of a node list.
     * <p><i>This method is primarily for the benefit of DOM, which uses its own NodeList
     * class to represent collections of nodes. Most other object models use standard
     * Java collection objects such as java.util.List</i></p>
     *
     * @param node an example of the kind of node used in this model
     * @return if the model does not recognize this node as one of its own, return null. Otherwise
     * return a PJConverter that takes a list of XPath nodes (represented as NodeInfo objects) and
     * returns a collection of nodes in this object model
     */
    @Override
    public PJConverter getNodeListCreator(Object node) {
        return null;
    }

    /**
     * Wrap or unwrap a node using this object model to return the corresponding Saxon node. If the supplied
     * source does not belong to this object model, return null
     *
     * @param source a JAXP Source object
     * @param config the Saxon configuration
     * @return a NodeInfo corresponding to the Source, if this can be constructed; otherwise null
     */
    @Override
    public NodeInfo unravel(Source source, Configuration config) {
        if (source instanceof DOMSource) {
            Node startNode = ((DOMSource) source).getNode();
            if (startNode instanceof Document) {
                try {
                    DominoTree tree = DominoTree.makeTree((Document) startNode, config, source.getSystemId());
                    return tree.getRootNode();
                } catch (XPathException e) {
                    throw new AssertionError(e);
                }
            }
        }
        return null;
    }
}

// Copyright (c) 2017-2022 Saxonica Limited


package com.saxonica.ee.stream.watch;

import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.trans.XPathException;

/**
 * A Watch represents a class that is interested in looking at nodes for the purpose
 * of evaluating uniqueness and key constraints, or for XSLT streaming.
 */

public abstract class AbstractWatch implements Watch {

    private PipelineConfiguration pipe;
    private NamespaceResolver namespaceResolver;


    public AbstractWatch() {
    }

    /**
     * Set the PipelineConfiguration in use
     *
     * @param pipe the PipelineConfiguration in use
     */

    public final void setPipelineConfiguration(PipelineConfiguration pipe) {
        this.pipe = pipe;
    }

    /**
     * Get the pipeline configuration in use
     *
     * @return the pipeline configuration
     */

    public final PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }

    /**
     * Set a NamespaceResolver for use by the Watch
     */

    public final void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    /**
     * Get the namespace resolver in use by the Watch
     *
     * @return the namespace resolver
     */

    public final NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Open the watch. This is done at the point where it first starts watching for nodes
     * that match the selection.
     * @param terminator
     */

    public void open(Terminator terminator) throws XPathException {
        //
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item       the node that was matched.
     *
     */

    public void processItem(Item<?> item) throws XPathException {
        //
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope. The default implementation does nothing.
     */

    public void close() throws XPathException {
        //
    }

    /**
     * Get a receiver to which start and end events for documents and elements can be
     * written. By default this will be the same as the receiver for the result feed,
     * because very few XSLT constructs (specifically, variables and functions) allow
     * the results of document and element constructors to be captured for further
     * processing. A construct that does need to process these events will intercept
     * them by providing its own receiver, typically a SequenceOutputter.
     */
    public SequenceReceiver getReceiver() {
        throw new UnsupportedOperationException();
    }
}

// Copyright (c) 2009 Saxonica Limited. No warranty.

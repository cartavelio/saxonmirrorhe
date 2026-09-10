package com.saxonica.ee.stream.watch;

import net.sf.saxon.event.PipelineConfiguration;
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

    @Override
    public final void setPipelineConfiguration(PipelineConfiguration pipe) {
        this.pipe = pipe;
    }

    /**
     * Get the pipeline configuration in use
     *
     * @return the pipeline configuration
     */

    @Override
    public final PipelineConfiguration getPipelineConfiguration() {
        return pipe;
    }

    /**
     * Set a NamespaceResolver for use by the Watch
     */

    @Override
    public final void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    /**
     * Get the namespace resolver in use by the Watch
     *
     * @return the namespace resolver
     */

    @Override
    public final NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Open the watch. This is done at the point where it first starts watching for nodes
     * that match the selection.
     * @param terminator action to be taken on failure
     */

    @Override
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

    @Override
    public void processItem(Item item) throws XPathException {
        //
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope. The default implementation does nothing.
     */

    @Override
    public void close() throws XPathException {
        //
    }

}

// Copyright (c) 2009-2022 Saxonica Limited

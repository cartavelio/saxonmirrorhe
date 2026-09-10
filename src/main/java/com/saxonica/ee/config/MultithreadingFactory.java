package com.saxonica.ee.config;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.parallel.MultithreadedItemMappingIterator;
import com.saxonica.ee.parallel.ThreadManagerEE;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * This class defines a number of methods that support the use of multi-threading in Saxon.
 * By subclassing <code>MultithreadingFactory</code> and registering the subclass with the
 * configuration using the method {@link EnterpriseConfiguration#setMultithreadingFactory(MultithreadingFactory)},
 * it is possible to customize Saxon's multithreading behavior.
 */

public class MultithreadingFactory {

    private final EnterpriseConfiguration config;

    /**
     * Create a <code>MultithreadingFactory</code>
     * @param config the Saxon Configuration (must be a Saxon-EE configuration)
     */

    public MultithreadingFactory(EnterpriseConfiguration config) {
        this.config = config;
    }

    /**
     * Get an item mapping iterator suitable for multi-threaded execution. The returned
     * iterator delivers the result of applying a mapping function to each item in an input
     * sequence. The result sequence respects the ordering of the input sequence, but the
     * order of processing (the timing of the executions of the mapping function) is
     * implementation-defined.
     *
     * @param base iterator over the input sequence
     * @param action mapping function to be applied to each item in the input sequence.
     * @return an iterator over the result sequence
     */

    public SequenceIterator makeMultithreadedItemMappingIterator(
            SequenceIterator base, ItemMappingFunction action) throws XPathException {
        return new MultithreadedItemMappingIterator(base, action, this);
    }

    /**
     * Make a ThreadManager for asynchronous xsl:result-document instructions
     *
     * @return a new ThreadManager (or null in the case of Saxon-HE, or if multithreading
     * is disabled)
     */

    public XPathContextMajor.ThreadManager makeThreadManager() {
        return new ThreadManagerEE(config);
    }

    /**
     * Make an ExecutorService for a given number of threads
     * @param threads the required number of threads
     */

    @CSharpReplaceBody(code="return null;") // temporary until we know whether we need this
    public ExecutorService makeExecutorService(int threads) {
        return Executors.newFixedThreadPool(threads);
    }

    /**
     * Make a new Thread
     * @param runnable the Runnable to be executed by the thread
     */

    public Thread makeThread(Runnable runnable) {
        return new Thread(runnable);
    }

}

// Copyright (c) 2018-2023 Saxonica Limited
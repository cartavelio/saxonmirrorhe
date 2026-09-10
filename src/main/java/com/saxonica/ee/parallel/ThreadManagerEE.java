package com.saxonica.ee.parallel;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.trans.Outcome;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.ThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

/**
 * The ThreadManager is used to manage asynchronous execution of xsl:result-document instructions in Saxon-EE.
 * It holds an ExecutorService which manages the pool of threads, and allows an exception to be notified if
 * any of the asynchronous threads fails with a run-time error. A ThreadManager is created when an XSLT
 * transformation starts, and is shut down when it ends, which forces the main thread to wait until all
 * asynchronous threads have finished. Executing a try/catch creates a new ThreadManager on the context stack,
 * which is used to manage all xsl:result-document instructions fired off within that try/catch block; the
 * xsl:try waits until it is sure that all such threads have completed, and handles the exception appropriately
 * if any of the threads fails.
 */

public class ThreadManagerEE extends XPathContextMajor.ThreadManager {
    private final EnterpriseConfiguration config;
    private ExecutorService executorService = null;
    private CountingCompletionService<Outcome<Boolean>> completionService = null;

    /**
     * Create a thread manager. This initializes an ExecutorService using a fixed thread pool,
     * the number of threads being determined (by default) by the number of available CPUs
     */

    public ThreadManagerEE(EnterpriseConfiguration config) {
        this.config = config;
    }

    /**
     * Get the ExecutorService (so that new work can be submitted)
     * @return the ExecutorService
     */

//    public ExecutorService getExecutorService() {
//        System.err.println("getExecutorService in thread " + Thread.currentThread().getId());
//        if (executorService == null) {
//            activate();
//        }
//        return executorService;
//    }

    public CountingCompletionService<Outcome<Boolean>> getCompletionService() {
        // See bug 4074. Marking this method synchronized causes test result-document-1006 to deadlock.
        // For reasons not fully understood, synchronizing activate() does not have the same problem.
        if (completionService == null) {
            activate();
        }
        return completionService;
    }

    private synchronized void activate() {
        if (completionService == null) {
            int threads = config.getResultDocumentThreadsLimit();
            if (threads < 1) {
                threads = 1;
            }
            executorService = config.getMultithreadingFactory().makeExecutorService(threads);
            //#if CSHARP==false
            // Following line ensures that if the queue gets too long, an xsl:result-document request is executed
            // in the caller thread, which gives the worker threads some respite from new submissions
            if (executorService instanceof ThreadPoolExecutor) {
                ((ThreadPoolExecutor) executorService).setRejectedExecutionHandler(
                        new ThreadPoolExecutor.CallerRunsPolicy());
            }
            //#endif

            completionService = new CountingCompletionService<>(executorService);
        }
    }

    /**
     * Wait for child threads started under the control of this context to finish.
     * This is called at the end of the (main thread of a) transformation, and also
     * at the end of the "try" part of a try/catch. The threads affected are those
     * used to implement xsl:result-document instructions.
     *
     * @throws XPathException if any of the child threads have failed with a dynamic
     *                        error.
     */

    @Override
    public synchronized void waitForChildThreads() throws XPathException {

        if (executorService == null) {
            // never activated
            return;
        }

        // It's not enough to just call shutdown() on the ExecutorService, followed by
        // awaitTermination. That's because at this stage child threads (representing
        // xsl:result-document instructions) might not only be running, but they might still
        // submit further xsl:result-document tasks, which would not be executed if the
        // ExecutorService were already shut down.

        // So the mechanism we use is that xsl:result-document returns an Outcome (either success
        // or an exception), and the Outcome is made available to a CompletionService. The
        // CompletionService tracks how many tasks have been started, and it ensures that the same
        // number of tasks have finished.

//        System.err.println("*** Submitted tasks: " + completionService.getNumberOfSubmittedTasks());
//        System.err.println("*** Completed tasks: " + completionService.getNumberOfCompletedTasks());

        try {
            while (completionService.hasUncompletedTasks()) {
                Outcome<Boolean> result = completionService.take().get();
                if (!result.isSuccess()) {
                    Exception e = result.getException();
                    if (!(e instanceof XPathException)) {
                        e.printStackTrace();
                    }
                    throw XPathException.makeXPathException(e);
                }
            }
        } catch (InterruptedException | ExecutionException e) {
            e.printStackTrace();
            throw new XPathException(e);
        }

        //System.err.println("*** Submitted tasks: " + completionService.getNumberOfSubmittedTasks());
        //System.err.println("*** Completed tasks: " + completionService.getNumberOfCompletedTasks());

        finally {
            executorService.shutdown();
            boolean finished;
            try {
                // At this point everything should have finished, because we know the completion service has no uncompleted
                // tasks. So the timeout here should be a formality.
                finished = awaitTermination();
                if (!finished) {
                    System.err.println("Some child xsl:result-document threads have not finished");
                }
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }


    }

    @CSharpReplaceBody(code="return executorService.awaitTermination();")
    private boolean awaitTermination() throws InterruptedException {
        return executorService.awaitTermination(10, TimeUnit.SECONDS);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited
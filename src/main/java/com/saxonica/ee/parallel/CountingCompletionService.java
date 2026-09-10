package com.saxonica.ee.parallel;

import net.sf.saxon.om.AtomicCounter;
import net.sf.saxon.transpile.CSharpModifiers;

import java.util.concurrent.*;

/**
 * Extension of ExecutorCompletionService to count the number of tasks submitted
 * and take that many completed tasks before calling it a day.
 *
 * <p>This is necessary because the base class, ExecutorCompletionService, does not provide
 * any way of knowing how many results you need to read off before you know that you have
 * read them all.</p>
 */

public class CountingCompletionService<V> extends ExecutorCompletionService<V> {
    private final AtomicCounter submittedTasks = new AtomicCounter(0);
    private final AtomicCounter completedTasks = new AtomicCounter(0);

    public CountingCompletionService(Executor executor) {
        super(executor);
    }

    public CountingCompletionService(
        Executor executor, BlockingQueue<Future<V>> queue) {
        super(executor, queue);
    }

    @Override
    @CSharpModifiers(code={"public", "override"})
    public Future<V> submit(Callable<V> task) {
        Future<V> future = super.submit(task);
        submittedTasks.incrementAndGet();
        return future;
    }

    @Override
    @CSharpModifiers(code = {"public", "override"})
    public Future<V> submit(Runnable task, V result) {
        Future<V> future = super.submit(task, result);
        submittedTasks.incrementAndGet();
        return future;
    }

    @Override
    @CSharpModifiers(code = {"public", "override"})
    public Future<V> take() throws InterruptedException {
        Future<V> future = super.take();
        //System.err.println("Successful take()");
        completedTasks.incrementAndGet();
        return future;
    }

    @Override
    @CSharpModifiers(code = {"public", "override"})
    public Future<V> poll() {
        Future<V> future = super.poll();
        if (future != null) {
            //System.err.println("Successful poll()");
            completedTasks.incrementAndGet();
        }
        return future;
    }

    //#if CSHARP==false
    @Override
    public Future<V> poll(long timeout, TimeUnit unit) throws InterruptedException {
        Future<V> future = super.poll(timeout, unit);
        if (future != null) {
            //System.err.println("Successful poll#2()");
            completedTasks.incrementAndGet();
        }
        return future;
    }
    //#endif

    public long getNumberOfCompletedTasks() {
        return completedTasks.get();
    }

    public long getNumberOfSubmittedTasks() {
        return submittedTasks.get();
    }

    public boolean hasUncompletedTasks() {
        return completedTasks.get() < submittedTasks.get();
    }
}


// Copyright (c) 2018-2022 Saxonica Limited
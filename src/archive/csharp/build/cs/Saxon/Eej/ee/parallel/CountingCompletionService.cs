using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.parallel {
    public class CountingCompletionService<V>  : Saxon.Ejava.util.concurrent.ExecutorCompletionService<V> {
        private readonly Saxon.Hej.om.AtomicCounter submittedTasks = new Saxon.Hej.om.AtomicCounter(0);
        private readonly Saxon.Hej.om.AtomicCounter completedTasks = new Saxon.Hej.om.AtomicCounter(0);
        public CountingCompletionService(Saxon.Ejava.util.concurrent.Executor executor) : base (executor) {
        }
        public CountingCompletionService(Saxon.Ejava.util.concurrent.Executor executor, System.Collections.Concurrent.BlockingCollection<Saxon.Ejava.util.concurrent.Future<V>> queue) : base (executor, queue) {
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.concurrent.Future<V> submit(Saxon.Ejava.util.concurrent.Callable<V> task) {
            Saxon.Ejava.util.concurrent.Future<V> future = base.submit(task);
            submittedTasks.incrementAndGet();
            return future;
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.concurrent.Future<V> submit(Saxon.Ejava.lang.Runnable task, V result) {
            Saxon.Ejava.util.concurrent.Future<V> future = base.submit(task, result);
            submittedTasks.incrementAndGet();
            return future;
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.concurrent.Future<V> take() {
            Saxon.Ejava.util.concurrent.Future<V> future = base.take();
            completedTasks.incrementAndGet();
            return future;
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.concurrent.Future<V> poll() {
            Saxon.Ejava.util.concurrent.Future<V> future = base.poll();
            if ((future != null)) {
                completedTasks.incrementAndGet();
            }
            return future;
        }
        public long getNumberOfCompletedTasks() {
            return completedTasks.get();
        }
        public long getNumberOfSubmittedTasks() {
            return submittedTasks.get();
        }
        public bool hasUncompletedTasks() {
            return (completedTasks.get()<submittedTasks.get());
        }
    
    }
}
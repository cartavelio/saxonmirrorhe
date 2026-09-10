namespace Saxon.Ejava.util.concurrent {
    public class ExecutorCompletionService<V> : CompletionService<V> {

        private Executor executor;
        private readonly System.Collections.Concurrent.BlockingCollection<Saxon.Ejava.util.concurrent.Future<V>> completionQueue;
        public ExecutorCompletionService(Executor executor) {
            this.executor = executor;
            this.completionQueue = new();
        }

        public ExecutorCompletionService(Executor executor,
                System.Collections.Concurrent.BlockingCollection<Saxon.Ejava.util.concurrent.Future<V>> queue) {
            this.executor = executor;
            this.completionQueue = queue;
        }
        
        public virtual Future<V> submit(Callable<V> task) {
            throw new System.NotImplementedException();
        }
        
        public virtual Future<V> submit(Saxon.Ejava.lang.Runnable task, V result) {
            throw new System.NotImplementedException();
            
        }

        public virtual Future<V> take() {
            return completionQueue.Take();
        }
        
        public virtual Future<V> poll() {
            throw new System.NotImplementedException();
        }
    }
}
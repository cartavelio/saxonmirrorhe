namespace java.util.concurrent {
    public class ExecutorCompletionService<V> : CompletionService<V> {

        private Executor executor;
        private readonly System.Collections.Concurrent.BlockingCollection<java.util.concurrent.Future<V>> completionQueue;
        public ExecutorCompletionService(Executor executor) {
            this.executor = executor;
            this.completionQueue = new();
        }

        public ExecutorCompletionService(Executor executor,
                System.Collections.Concurrent.BlockingCollection<java.util.concurrent.Future<V>> queue) {
            this.executor = executor;
            this.completionQueue = queue;
        }
        
        public Future<V> submit(Callable<V> task) {
            throw new System.NotImplementedException();
        }
        
        public Future<V> submit(java.lang.Runnable task, V result) {
            throw new System.NotImplementedException();
            
        }

        public Future<V> take() {
            return completionQueue.Take();
        }
        
        public Future<V> poll() {
            throw new System.NotImplementedException();
        }
    }
}
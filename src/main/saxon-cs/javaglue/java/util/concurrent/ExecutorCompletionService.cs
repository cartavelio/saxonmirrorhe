namespace java.util.concurrent {
    public class ExecutorCompletionService<V> : CompletionService<V> {

        public ExecutorCompletionService(Executor executor) {
            
        }

        public ExecutorCompletionService(Executor executor,
            System.Collections.Concurrent.BlockingCollection<java.util.concurrent.Future<V>> queue) {
        }
        
        public Future<V> submit(Callable<V> task) {
            throw new System.NotImplementedException();
        }
        
        public Future<V> submit(java.lang.Runnable task, V result) {
            throw new System.NotImplementedException();
        }

        public Future<V> take() {
            throw new System.NotImplementedException();
        }
        
        public Future<V> poll() {
            throw new System.NotImplementedException();
        }
    }
}
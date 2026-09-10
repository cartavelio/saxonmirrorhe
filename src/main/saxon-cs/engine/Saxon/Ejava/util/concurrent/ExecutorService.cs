namespace Saxon.Ejava.util.concurrent {
    public interface ExecutorService : Executor {

        bool awaitTermination();

        void shutdown();

        bool isShutdown();
        
        Future<T> submit<T>(Callable<T> task);

    }
}
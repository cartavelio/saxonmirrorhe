namespace java.util.concurrent {
    public interface ExecutorService {

        void shutdown();

        bool isShutdown();
        
        Future<T> submit<T>(Callable<T> task);

    }
}
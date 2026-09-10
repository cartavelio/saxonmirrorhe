namespace Saxon.Ejava.util.concurrent {
    public interface CompletionService<T> {

        
        Future<T> submit(Callable<T> task);

    }
}
using Saxon.Ejava.lang;

namespace Saxon.Ejava.util.concurrent {
    public interface RunnableFuture<V> : Runnable, Future<V> {
        //new void run();
    }
}
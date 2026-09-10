using java.lang;

namespace java.util.concurrent {
    public interface RunnableFuture<V> : Runnable, Future<V> {
        //new void run();
    }
}
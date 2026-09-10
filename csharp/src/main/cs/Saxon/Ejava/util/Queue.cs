namespace Saxon.Ejava.util {
    public interface Queue<T> : Collection<T> {

        T poll();
    }
}
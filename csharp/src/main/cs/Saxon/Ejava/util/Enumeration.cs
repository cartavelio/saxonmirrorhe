namespace Saxon.Ejava.util {
    public interface Enumeration<T> {
        bool hasMoreElements();
        T nextElement();
    }
}
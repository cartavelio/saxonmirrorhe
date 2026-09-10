namespace Saxon.Ejava.util {
    public interface Iterator<T> {
        bool hasNext();
        T next();
        //We don't need Iterator#remove() so don't include it in the definition 
        //void remove();
    }
}
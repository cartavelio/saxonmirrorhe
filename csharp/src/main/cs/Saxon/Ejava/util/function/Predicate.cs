namespace Saxon.Ejava.util.function {
    // Not currently implemented as a delegate, because this causes problems with NodeTest.
    // May need two flavours?
    public interface Predicate<T> {
        bool test(T value);
    }
}
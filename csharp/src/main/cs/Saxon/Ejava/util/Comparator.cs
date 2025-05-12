/*
 * A C# emulation of java.util.Comparator
 */
 
namespace Saxon.Ejava.util {
    public interface Comparator<in T> {
        int compare(T a, T b);
    }
}
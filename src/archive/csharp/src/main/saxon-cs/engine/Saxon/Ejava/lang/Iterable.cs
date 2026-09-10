using Saxon.Hej.expr;
using Saxon.Hej.trans;
using SysGen = System.Collections.Generic;
namespace Saxon.Ejava.lang {
    // This class emulates Java's Iterable interface, but it also extends C#'s IEnumerable,
    // to ensure that any implementation of the interface works in a C# foreach loop
    public interface Iterable<T> : SysGen.IEnumerable<T> {
        Saxon.Ejava.util.Iterator<T> iterator();
    }
}
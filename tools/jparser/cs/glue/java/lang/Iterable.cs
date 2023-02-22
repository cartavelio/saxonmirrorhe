using net.sf.saxon.expr;
using net.sf.saxon.trans;
using SysGen = System.Collections.Generic;
namespace java.lang {
    // This class emulates Java's Iterable interface, but it also extends C#'s IEnumerable,
    // to ensure that any implementation of the interface works in a C# foreach loop
    public interface Iterable<T> : SysGen.IEnumerable<T> {
        java.util.Iterator<T> iterate();
    }
}
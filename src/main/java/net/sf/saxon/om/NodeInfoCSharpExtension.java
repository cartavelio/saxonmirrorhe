package net.sf.saxon.om;

import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;

import java.util.function.Predicate;

/**
 * Extension to the NodeInfo interface used in the C# code only. The reason for this extension is that
 * in C#, a concrete class cannot implement a functional interface (=delegate), so methods such as children()
 * and iterateAxis() that accept either a NodeTest or a predicate (expressed as a lambda expression)
 * need to be supplied as two separate methods.
 */

public interface NodeInfoCSharpExtension {

    //#if CSHARP=true

    Iterable<? extends NodeInfo> children(NodeTest filter);
    AxisIterator iterateAxis(AxisInfo axis, NodeTest nodeTest);

    default AxisIterator iterateAxis(AxisInfo axis, Predicate<NodeInfo> predicate) {
        return iterateAxis(axis, new NodePredicate(predicate));
    }

    default Iterable<? extends NodeInfo> children(Predicate<NodeInfo> predicate) {
        return children(new NodePredicate(predicate));
    }

    //#endif
}


package net.sf.saxon.om;

import net.sf.saxon.pattern.AnyXNodeTest;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.tree.iter.AxisIterator;

public interface GNode extends Item {
    
    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link AxisInfo}
     * @return an AxisIterator that delivers the nodes reached by the axis in
     * turn. The nodes are returned in axis order (document order for a forwards
     * axis, reverse document order for a reverse axis). The default implementation
     * returns {@code iterateAxis(axisNumber, AnyNodeTest.getInstance()}.
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see AxisInfo
     * @since 8.4
     */

    default AxisIterator iterateAxis(int axisNumber) {
        return iterateAxis(axisNumber, AnyXNodeTest.getInstance());
    }

    /**
     * Return an iteration over all the nodes reached by the given axis from this node
     * that match a given NodeTest
     *
     * @param axisNumber an integer identifying the axis; one of the constants
     *                   defined in class {@link AxisInfo}
     * @param predicate  A condition to be satisfied by the returned nodes; nodes
     *                   that do not satisfy this condition are not included in the result
     * @return an AxisIterator that delivers the nodes reached by the axis in
     * turn.  The nodes are returned in axis order (document order for a forwards
     * axis, reverse document order for a reverse axis).
     * @throws UnsupportedOperationException if the namespace axis is
     *                                       requested and this axis is not supported for this implementation.
     * @see AxisInfo
     * @since 8.4. Changed in 10.0 to accept any {@code Predicate<NodeInfo>} as the second argument. It is still
     * possible to supply a {@link NodeTest}, because {@code NodeTest} implements {@code Predicate<NodeInfo>}.
     */
    AxisIterator iterateAxis(int axisNumber, NodePredicate predicate);

    /**
     * Get the parent of this node
     * @return the parent node if there is one, or null otherwise
     */

    GNode getParent();

    /**
     * Ask whether the node has children
     * @return true if the node has children
     */

    boolean hasChildNodes();

}


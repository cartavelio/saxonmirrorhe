package net.sf.saxon.ma.jnode;

import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.IncrementalIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;

public abstract class JNode implements GNode {

    public abstract GroundedValue getValue();

    public boolean hasChildNodes() {
        GroundedValue value = getValue();
        for (Item item : value.asIterable()) {
            if (item instanceof ArrayItem && !((ArrayItem)item).isEmpty()) {
                return true;
            }
            if (item instanceof MapItem && !((MapItem) item).isEmpty()) {
                return true;
            }
        }
        return false;
    }

    public abstract int getPosition();

    public abstract AtomicValue getSelector();

    @Override
    public abstract JNode getParent();

    public int getNodeKind() {
        return Type.JNODE;
    }

    /**
     * Get the genre of this item (to distinguish the top-level categories of item,
     * such as nodes, atomic values, and functions)
     *
     * @return the genre
     */
    @Override
    public Genre getGenre() {
        return Genre.JNODE;
    }

    /**
     * Get the value of the item as a Unicode string. For nodes, this is the string value of the
     * node as defined in the XPath 2.0 data model, except that all nodes are treated as being
     * untyped: it is not an error to get the string value of a node with a complex type.
     * For atomic values, the method returns the result of casting the atomic value to a string.
     *
     * @return the string value of the item
     * @throws UnsupportedOperationException if the item is a function item (an unchecked exception
     *                                       is used here to avoid introducing exception handling to a large number of paths where it is not
     *                                       needed)
     * @since 8.4
     */
    @Override
    public UnicodeString getUnicodeStringValue() {
        try {
            return getValue().getUnicodeStringValue();
        } catch (XPathException e) {
            throw new UncheckedXPathException(e);
        }
    }

    /**
     * Atomize the item.
     *
     * @return the result of atomization
     * @throws XPathException if atomization is not allowed
     *                        for this kind of item
     */
    @Override
    public AtomicSequence atomize() throws XPathException {
        return Atomizer.atomize(getValue());
    }

    public SequenceIterator iterateAxis(int axis) {
        switch (axis) {
            case AxisInfo.SELF:
                return SingletonIterator.makeIterator(this);
            case AxisInfo.CHILD:
                return getChildren();
            case AxisInfo.PARENT:
                if (getParent() == null) {
                    return EmptyIterator.getInstance();
                } else {
                    return SingletonIterator.makeIterator(getParent());
                }
            case AxisInfo.ANCESTOR:
                if (getParent() == null) {
                    return EmptyIterator.getInstance();
                } else {
                    return new IncrementalIterator(getParent(), GNode::getParent);
                }
            case AxisInfo.ANCESTOR_OR_SELF:
                return new IncrementalIterator(getParent(), GNode::getParent);
            case AxisInfo.DESCENDANT:
                return new Navigator.DescendantIterator(this, false, true);
            case AxisInfo.DESCENDANT_OR_SELF:
                return new Navigator.DescendantIterator(this, true, true);
            default:
                throw new IllegalArgumentException("Unrecognized JNode axis number " + axis);
        }
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
    @Override
    public SequenceIterator iterateAxis(int axisNumber, NodePredicate predicate) {
        return iterateAxis(axisNumber); //TEMPORARY
    }

    /**
     * Returns a string representation of the object.
     */
    @Override
    public String toString() {
        return "JNode(" + getValue().toString() + ")";
    }

    @Override
    public String toShortString() {
        return "JNode(" + getValue().toShortString() + ")";
    }

    public abstract SequenceIterator getChildren();


}


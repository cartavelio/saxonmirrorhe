package net.sf.saxon.ma.jnode;

import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
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


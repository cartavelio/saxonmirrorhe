package net.sf.saxon.ma.jnode;

import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.functions.Reverse;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

public abstract class JNode implements Item {

    public abstract GroundedValue getValue();

    public SequenceIterator getChildren() {
        // TODO: avoid materializing the list         )
        List<JNode> children = new ArrayList<>();
        int position = 1;
        for (Item item : getValue().asIterable()) {
            if (item instanceof ArrayItem) {
                int selector = 1;
                for (GroundedValue member : ((ArrayItem) item).members()) {
                    final ChildJNode jNode = new ChildJNode(
                            this, position, Int64Value.makeIntegerValue(selector++), member);
                    children.add(jNode);
                }
            } else if (item instanceof MapItem) {
                for (KeyValuePair pair : ((MapItem) item).keyValuePairs()) {
                    final ChildJNode jNode = new ChildJNode(
                            this, position, pair.key, pair.value);
                    children.add(jNode);
                }
            }
            position++;
        }
        return new ListIterator.Of<>(children);
    }

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

    public abstract JNode getParent();

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
                return new SingletonIterator(this);
            case AxisInfo.CHILD:
                return getChildren();
            case AxisInfo.PARENT:
                if (getParent() == null) {
                    return EmptyIterator.getInstance();
                } else {
                    return new SingletonIterator(getParent());
                }
            case AxisInfo.ANCESTOR:
                if (getParent() == null) {
                    return EmptyIterator.getInstance();
                } else {
                    return new IncrementalIterator(getParent(), JNode::getParent);
                }
            case AxisInfo.ANCESTOR_OR_SELF:
                return new IncrementalIterator(getParent(), JNode::getParent);
            case AxisInfo.DESCENDANT:
                return new DescendantIterator(this, false, true);
            case AxisInfo.DESCENDANT_OR_SELF:
                return new DescendantIterator(this, true, true);
            default:
                throw new IllegalArgumentException("Unrecognized JNode axis number " + axis);
        }
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

    private static class IncrementalIterator implements SequenceIterator {

        // Note: based on net.sf.saxon.tree.IncrementalIterator. TODO: combine these classes

        private final Function<JNode, JNode> stepper;
        private JNode nextItem;

        /**
         * Create an IncrementalIterator
         *
         * @param start   the first item to be returned by the iterator
         * @param stepper a function that computes the next item, given the current item, or returns
         *                null at the end of the sequence.
         */
        public IncrementalIterator(JNode start, java.util.function.Function<JNode, JNode> stepper) {
            this.stepper = stepper;
            this.nextItem = stepper.apply(start);
        }

        /**
         * Get the next item in the sequence
         *
         * @return the next item, or null if there are no more items to be returned.
         */
        public JNode next() {
            JNode current = nextItem;
            if (current != null) {
                nextItem = stepper.apply(current);
            }
            return current;
        }

    }

    private static final class DescendantIterator implements SequenceIterator {

        // TODO: recombine with Navigator.DescendantIterator, from which this is derived

        private SequenceIterator children = null;
        private SequenceIterator descendants = null;
        private final JNode start;
        private final boolean includeSelf;
        private final boolean forwards;
        private boolean atEnd = false;

        /**
         * Create an iterator over the descendant or descendant-or-self axis
         *
         * @param start       the initial context node
         * @param includeSelf true if the "self" node is to be included
         * @param forwards    true for a forwards iteration, false for reverse order
         */

        public DescendantIterator(JNode start,
                                  boolean includeSelf, boolean forwards) {
            this.start = start;
            this.includeSelf = includeSelf;
            this.forwards = forwards;
        }

        @Override
        public JNode next() {
            if (descendants != null) {
                JNode nextd = (JNode)descendants.next();
                if (nextd != null) {
                    return nextd;
                } else {
                    descendants = null;
                }
            }
            if (children != null) {
                JNode n = (JNode)children.next();
                if (n != null) {
                    if (n.hasChildNodes()) {
                        if (forwards) {
                            descendants = new DescendantIterator(n, false, true);
                            return n;
                        } else {
                            descendants = new DescendantIterator(n, true, false);
                            return next();
                        }
                    } else {
                        return n;
                    }
                } else {
                    if (forwards || !includeSelf) {
                        return null;
                    } else {
                        atEnd = true;
                        children = null;
                        return start;
                    }
                }
            } else if (atEnd) {
                // we're just finishing a backwards scan
                return null;
            } else {
                // we're just starting...
                if (start.hasChildNodes()) {
                    children = start.iterateAxis(AxisInfo.CHILD);
                    if (!forwards) {
                        children = Reverse.getReverseIterator(children);
                    }
                } else {
                    children = EmptyIterator.ofNodes();
                }
                if (forwards && includeSelf) {
                    return start;
                } else {
                    return next();
                }
            }
        }

        public void advance() {

        }

    }
}


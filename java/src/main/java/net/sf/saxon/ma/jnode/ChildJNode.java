package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.nodetest.NodePredicate;
import net.sf.saxon.tree.iter.SequenceIteratorOverJavaIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.tree.wrapper.SiblingCountingNode;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

import java.util.Iterator;

public class ChildJNode extends JNode implements SiblingCountingNode {
    private final JNode parent;
    private final int position;
    private final AtomicValue selector;
    private final GroundedValue value;
    private int serialNr;

    public ChildJNode(JNode parent, int position, AtomicValue selector, GroundedValue value, int serialNr) {
        this.parent = parent;
        this.position = position;
        this.selector = selector;
        this.value = value;
        this.serialNr = serialNr;
    }

    @Override
    public GroundedValue getContent() {
        return value;
    }


    @Override
    public int getPosition() {
        return position;
    }

    @Override
    public AtomicValue getSelector() {
        return selector;
    }

    @Override
    public JNode getParent() {
        return parent;
    }

    /**
     * Get the index position of this node among its siblings (starting from 0)
     *
     * @return 0 for the first child, 1 for the second child, etc.
     */
    @Override
    public int getSiblingPosition() {
        if (serialNr == -1) {
            allocateSerialNr();
        }
        return serialNr;
    }

    public SequenceIterator getChildren() {
        return new JNodeChildIterator(this);
    }

    /**
     * Get an iterator over the child axis, starting at this node; the nodes will
     * be in document order.
     *
     * @param predicate a condition that the nodes must satisfy, or null
     * @return the required iterator
     */
    @Override
    public SequenceIterator iterateChildAxis(NodePredicate predicate) {
        JNodeChildIterator iter = new JNodeChildIterator(this);
        if (predicate != null) {
            return Navigator.filter(iter, predicate);
        }
        return iter;
    }

    /**
     * Get an iterator over the following-sibling axis, starting at this node; the nodes will
     * be in document order.
     *
     * @param predicate a condition that the nodes must satisfy, or null
     * @return the required iterator
     */
    @Override
    public SequenceIterator iterateFollowingSiblingAxis(NodePredicate predicate) {
        if (parent.getContent() instanceof MapItem) {
            Iterator<KeyValuePair> iter = ((MapItem)parent.getContent()).followingKeyValuePairs(getSelector());
            return new SequenceIteratorOverJavaIterator<>(
                    iter,
                    kvp -> new ChildJNode(parent, getPosition(), kvp.key, kvp.value, -1));
        }
        if (parent.getContent() instanceof ArrayItem) {
            throw new UnsupportedOperationException();
        }
        throw new UnsupportedOperationException();
    }

    /**
     * Get an iterator over the preceding-sibling axis, starting at this node; the nodes will
     * be in reverse document order.
     *
     * @param predicate a condition that the nodes must satisfy, or null
     * @return the required iterator
     */
    @Override
    public SequenceIterator iteratePrecedingSiblingAxis(NodePredicate predicate) {
        if (parent.getContent() instanceof MapItem) {
            Iterator<KeyValuePair> iter = ((MapItem) parent.getContent()).precedingKeyValuePairs(getSelector());
            return new SequenceIteratorOverJavaIterator<>(
                    iter,
                    kvp -> new ChildJNode(parent, getPosition(), kvp.key, kvp.value, -1));
        }
        if (parent.getContent() instanceof ArrayItem) {
            throw new UnsupportedOperationException();
        }
        throw new UnsupportedOperationException();
    }

    /**
     * Compare document order of this node against another node.
     * <p>The other node must always be in the same tree; the effect of calling this method
     * when the two nodes are in different trees is undefined. To obtain a global ordering
     * of nodes, the application should first compare the result of getDocumentNumber(),
     * and only if the document number is the same should compareOrder() be called.</p>
     *
     * @param other the other node
     * @return -1 if this node precedes the other, 0 if they are the same node, +1 if
     * this node follows the other
     */
    @Override
    public int compareOrder(GNode other) {
        if (!(other instanceof JNode)) {
            throw new IllegalArgumentException("Trying to compare JNodes and XNodes");
        }
        if (other.equals(this)) {
            return 0;
        }
        if (other instanceof RootJNode) {
            return -other.compareOrder(this);
        }
        if (getParent().getContent() == ((JNode)other.getParent()).getContent()) {
            // Comparing two entries in the same map or array
            if (serialNr == -1) {
                allocateSerialNr();
            }
            if (((ChildJNode)other).serialNr == -1) {
                ((ChildJNode) other).allocateSerialNr();
            }
            return serialNr - ((ChildJNode) other).serialNr;
        }
        return Navigator.compareOrder(this, (SiblingCountingNode)other);
    }

    public boolean equals(Object other) {
        return other instanceof ChildJNode
                && getParent().equals(((ChildJNode)other).getParent())
                && getSelector().asMapKey().equals(((ChildJNode) other).getSelector().asMapKey())
                && getPosition() == ((ChildJNode) other).getPosition();
    }

    public int hashCode() {
        return getParent().hashCode()
                ^ getSelector().asMapKey().hashCode()
                ^ getPosition();
    }

    private void allocateSerialNr() {
        if (parent.getContent() instanceof MapItem) {
            Iterator<KeyValuePair> iter = ((MapItem) parent.getContent()).precedingKeyValuePairs(getSelector());
            int count = 0;
            while (iter.hasNext()) {
                count++;
                iter.next();
            }
            this.serialNr = count;
        }
        if (parent.getContent() instanceof ArrayItem) {
            this.serialNr = (int)((Int64Value)selector).longValue();
        }
    }

    /**
     * Iterator over the children of this JNode
     */

    private static class JNodeChildIterator implements SequenceIterator {

        private final ChildJNode origin;
        private final SequenceIterator valueIterator;
        private Iterator<KeyValuePair> mapEntryIterator;
        private Iterator<GroundedValue> arrayEntryIterator;
        private int position = 0;
        private int arrayIndex = 0;
        private int serialNr = 0;

        public JNodeChildIterator(ChildJNode origin) {
            this.origin = origin;
            valueIterator = origin.getContent().iterate();
        }

        /**
         * Get the next item in the sequence.
         */
        @Override
        public GNode next() {
            if (mapEntryIterator != null) {
                if (mapEntryIterator.hasNext()) {
                    KeyValuePair nextItem = mapEntryIterator.next();
                    return new ChildJNode(
                            origin, position, nextItem.key, nextItem.value, serialNr++);
                } else {
                    mapEntryIterator = null;
                }
            }
            if (arrayEntryIterator != null) {
                if (arrayEntryIterator.hasNext()) {
                    GroundedValue nextMember = arrayEntryIterator.next();
                    return new ChildJNode(
                            origin, position, Int64Value.makeIntegerValue(++arrayIndex), nextMember, serialNr++);
                } else {
                    arrayEntryIterator = null;
                }
            }
            while (true) {
                position++;
                Item nextInValue = valueIterator.next();
                if (nextInValue == null) {
                    return null;
                }
                if (nextInValue instanceof ArrayItem) {
                    arrayEntryIterator = ((ArrayItem) nextInValue).members().iterator();
                    arrayIndex = 0;
                    return next();
                }
                if (nextInValue instanceof MapItem) {
                    mapEntryIterator = ((MapItem) nextInValue).keyValuePairs().iterator();
                    return next();
                }
            }
        }
    }


}


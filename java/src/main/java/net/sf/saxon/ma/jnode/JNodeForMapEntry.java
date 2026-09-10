package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.map.AbstractFixedMap;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.nodetest.NodePredicate;
import net.sf.saxon.tree.iter.SequenceIteratorOverJavaIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.tree.wrapper.SiblingCountingNode;
import net.sf.saxon.value.AtomicValue;

import java.util.Iterator;

/**
 * A child JNode that wraps an entry in a map
 */
public class JNodeForMapEntry extends ChildJNode implements SiblingCountingNode {

    private int serialNr = -1;

    /**
     * Create a JNode wrapping an entry in a map
     * @param parent the JNode whose content includes the containing map
     * @param position the 1-based position of the containing map within the content of the parent
     * @param selector the key of the relevant map entry within its containing map
     * @param value the value corresponding to this key
     * @param serialNr a sequential number representing the sibling position of this entry;
     *                 may be -1 if not initially known
     */
    public JNodeForMapEntry(JNode parent, int position, AtomicValue selector, GroundedValue value, int serialNr) {
        super(parent, position, selector, value);
        this.serialNr = serialNr;
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

    /**
     * Get an iterator over the following-sibling axis, starting at this node; the nodes will
     * be in document order.
     *
     * @param predicate a condition that the nodes must satisfy, or null
     * @return the required iterator
     */
    @Override
    public SequenceIterator iterateFollowingSiblingAxis(NodePredicate predicate) {
        Iterator<KeyValuePair> iter = ((MapItem)parent.getContent()).followingKeyValuePairs(getSelector());
        final int startPos = serialNr + 1;
        SequenceIterator all = new SequenceIteratorOverJavaIterator<>(
                iter,
                (kvp, position) -> new JNodeForMapEntry(parent, getPosition(),
                                                        kvp.key,
                                                        kvp.value,
                                                        position + startPos));
        return Navigator.filter(all, predicate);
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
        Iterator<KeyValuePair> iter = ((MapItem) parent.getContent()).precedingKeyValuePairs(getSelector());
        final int endPos = serialNr - 1;
        SequenceIterator all = new SequenceIteratorOverJavaIterator<>(
                iter,
                (kvp, position) -> new JNodeForMapEntry(parent, getPosition(), kvp.key, kvp.value, endPos - position));
        return Navigator.filter(all, predicate);
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
            // Comparing two entries in the same map
            if (serialNr == -1) {
                allocateSerialNr();
            }
            if (((JNodeForMapEntry)other).serialNr == -1) {
                ((JNodeForMapEntry) other).allocateSerialNr();
            }
            return serialNr - ((JNodeForMapEntry) other).serialNr;
        }
        return Navigator.compareOrder(this, (SiblingCountingNode)other);
    }

    public boolean equals(Object other) {
        return other instanceof JNodeForMapEntry
                && getParent().equals(((JNodeForMapEntry)other).getParent())
                && getSelector().asMapKey().equals(((JNodeForMapEntry) other).getSelector().asMapKey())
                && getPosition() == ((JNodeForMapEntry) other).getPosition();
    }

    public int hashCode() {
        return getParent().hashCode()
                ^ getSelector().asMapKey().hashCode()
                ^ getPosition();
    }

    protected void allocateSerialNr() {
        if (parent.getContent() instanceof AbstractFixedMap) {
            this.serialNr = ((AbstractFixedMap)parent.getContent()).getIndex(getSelector());
        }
        Iterator<KeyValuePair> iter = ((MapItem) parent.getContent()).precedingKeyValuePairs(getSelector());
        int count = 0;
        while (iter.hasNext()) {
            count++;
            iter.next();
        }
        this.serialNr = count;
    }

    /**
     * Construct a character string that uniquely identifies this node.
     * Note: a.isSameNode(b) if and only if generateId(a)==generateId(b)
     *
     * @param buffer a buffer which will be updated to hold a string
     *               that uniquely identifies this node, across all documents.
     * @since 8.7
     * <p>Changed in Saxon 8.7 to generate the ID value in a client-supplied buffer</p>
     */
    @Override
    public void generateId(StringBuilder buffer) {
        getParent().generateId(buffer);
        buffer.append("c").append(serialNr);
    }



}


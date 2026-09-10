package net.sf.saxon.ma.jnode;

import net.sf.saxon.functions.Reverse;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.nodetest.NodePredicate;
import net.sf.saxon.tree.iter.SequenceIteratorOverJavaIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.tree.wrapper.SiblingCountingNode;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;

import java.util.Iterator;

public class JNodeForArrayMember extends ChildJNode implements SiblingCountingNode {

    public JNodeForArrayMember(JNode parent, int position, Int64Value selector, GroundedValue value) {
        super(parent, position, selector, value);
    }

    /**
     * Get the index position of this node among its siblings (starting from 0)
     *
     * @return 0 for the first child, 1 for the second child, etc.
     */
    @Override
    public int getSiblingPosition() {
        return (int) ((Int64Value) selector).longValue();
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
        ArrayItem wholeArray = (ArrayItem) parent.getContent();
        ArrayItem subarray = wholeArray.subArray(((IntegerValue)getSelector()).asSubscript(), wholeArray.arrayLength());
        Iterator<GroundedValue> iter = subarray.members().iterator();
        final int startPos = getSiblingPosition() + 1;
        SequenceIterator all = new SequenceIteratorOverJavaIterator<>(
                iter,
                (mem, position) -> new JNodeForArrayMember(parent, getPosition(),
                                                           new Int64Value(position + startPos + 1),
                                                           mem
                ));
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
        ArrayItem wholeArray = (ArrayItem) parent.getContent();
        int thisSubscript = getSiblingPosition();
        ArrayItem subarray = wholeArray.subArray(0, thisSubscript - 1);
        Iterator<GroundedValue> iter = subarray.members().iterator();
        SequenceIterator forwards = new SequenceIteratorOverJavaIterator<>(
                iter,
                (mem, position) -> new JNodeForArrayMember(parent,
                                                           getPosition(),
                                                           new Int64Value(position + 1),
                                                           mem
                ));
        SequenceIterator all = Reverse.getReverseIterator(forwards);
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
        if (getParent().equals(other.getParent())) {
            // Comparing two members in the same array
            return getSiblingPosition() - ((JNodeForArrayMember) other).getSiblingPosition();
        }
        return Navigator.compareOrder(this, (SiblingCountingNode)other);
    }

    public boolean equals(Object other) {
        return other instanceof JNodeForArrayMember
                && getParent().equals(((JNodeForArrayMember)other).getParent())
                && getSiblingPosition() == (((JNodeForArrayMember) other).getSiblingPosition())
                && getPosition() == ((JNodeForArrayMember) other).getPosition();
    }

    public int hashCode() {
        return getParent().hashCode()
                ^ getSiblingPosition() << 12
                ^ getPosition() << 24;
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
        buffer.append("m").append(getSiblingPosition());
    }


}


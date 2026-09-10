package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.MapOrArray;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodePredicate;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

import java.util.Iterator;

public class RootJNode extends JNode {

    private final MapOrArray value;

    public RootJNode(MapOrArray value) {
        this.value = value;
    }

    @Override
    public MapOrArray getValue() {
        return value;
    }

    @Override
    public int getPosition() {
        return -1;
    }

    @Override
    public AtomicValue getSelector() {
        return null;
    }

    @Override
    public JNode getParent() {
        return null;
    }

    @Override
    public SequenceIterator getChildren() {
        if (value instanceof MapItem) {
            return new JNodeMapIterator(this);
        } else {
            return new JNodeArrayIterator(this);
        }
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
        if (predicate == null) {
            return getChildren();
        }
        return Navigator.filter(getChildren(), predicate);
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
        return EmptyIterator.getInstance();
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
        return EmptyIterator.getInstance();
    }

    private static class JNodeMapIterator implements SequenceIterator {

        private final RootJNode origin;
        private final Iterator<KeyValuePair> mapEntryIterator;

        public JNodeMapIterator(RootJNode origin) {
            this.origin = origin;
            mapEntryIterator = ((MapItem)origin.getValue()).keyValuePairs().iterator();
        }

        /**
         * Get the next item in the sequence.
         */
        @Override
        public GNode next() {
            if (mapEntryIterator.hasNext()) {
                KeyValuePair nextItem = mapEntryIterator.next();
                return new ChildJNode(origin, 1, nextItem.key, nextItem.value);
            } else {
                return null;
            }
        }
    }

    private static class JNodeArrayIterator implements SequenceIterator {

        private final RootJNode origin;
        private final Iterator<GroundedValue> arrayEntryIterator;
        private int index = 1;

        public JNodeArrayIterator(RootJNode origin) {
            this.origin = origin;
            arrayEntryIterator = ((ArrayItem) origin.getValue()).members().iterator();
        }

        /**
         * Get the next item in the sequence.
         */
        @Override
        public GNode next() {
            if (arrayEntryIterator.hasNext()) {
                GroundedValue nextItem = arrayEntryIterator.next();
                return new ChildJNode(origin, 1, Int64Value.makeIntegerValue(index++), nextItem);
            } else {
                return null;
            }
        }
    }

}


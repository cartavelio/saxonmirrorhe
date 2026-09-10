package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.MapOrArray;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.AxisIterator;
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
    public AxisIterator getChildren() {
        if (value instanceof MapItem) {
            return new JNodeMapIterator(this);
        } else {
            return new JNodeArrayIterator(this);
        }
    }

    private static class JNodeMapIterator implements AxisIterator {

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

    private static class JNodeArrayIterator implements AxisIterator {

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


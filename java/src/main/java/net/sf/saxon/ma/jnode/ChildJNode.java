package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

import java.util.Iterator;

public class ChildJNode extends JNode {
    private JNode parent;
    private int position;
    private AtomicValue selector;
    private GroundedValue value;

    public ChildJNode(JNode parent, int position, AtomicValue selector, GroundedValue value) {
        this.parent = parent;
        this.position = position;
        this.selector = selector;
        this.value = value;
    }

    @Override
    public GroundedValue getValue() {
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


    public SequenceIterator getChildren() {
        return new JNodeChildIterator(this);
    }

    private static class JNodeChildIterator implements SequenceIterator {

        private final ChildJNode origin;
        private final SequenceIterator valueIterator;
        private Iterator<KeyValuePair> mapEntryIterator;
        private Iterator<GroundedValue> arrayEntryIterator;
        private int position = 0;
        private int arrayIndex = 0;

        public JNodeChildIterator(ChildJNode origin) {
            this.origin = origin;
            valueIterator = origin.getValue().iterate();
        }

        /**
         * Get the next item in the sequence.
         */
        @Override
        public GNode next() {
            if (mapEntryIterator != null) {
                KeyValuePair nextItem = mapEntryIterator.next();
                if (nextItem != null) {
                    return new ChildJNode(origin, position, nextItem.key, nextItem.value);
                } else {
                    mapEntryIterator = null;
                }
            }
            if (arrayEntryIterator != null) {
                GroundedValue nextMember = arrayEntryIterator.next();
                if (nextMember != null) {
                    return new ChildJNode(origin, position, Int64Value.makeIntegerValue(++arrayIndex), nextMember);
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


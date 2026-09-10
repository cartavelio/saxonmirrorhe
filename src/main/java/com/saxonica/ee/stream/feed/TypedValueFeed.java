package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;

/**
 * This class is used when a streamed template requests evaluation of the typed value
 * of the nodes selected by a path expression.
 */


public class TypedValueFeed extends AbsorptionFeed {


    public TypedValueFeed(Feed result, XPathContext context) {
        super(result, context);
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) {
        return new PendingTypedValue(sequenceNr, node);
    }

    @Override
    protected Sequence<?> processGroundedItem(Item item) throws XPathException {
        if (item instanceof AtomicValue) {
            return item;
        } else if (item instanceof NodeInfo) {
            return item.atomize();
        } else if (item instanceof ArrayItem) {
            return item.atomize();
        } else {
            dynamicError(new XPathException("Cannot atomize item of type " + Type.displayTypeName(item), "FOTY0013"));
            return null;
        }
    }

    /**
     * A pending item represents a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private static class PendingTypedValue extends PendingItem {
        public FastStringBuffer buffer;
        public FleetingParentNode node;

        public PendingTypedValue(int sequence, FleetingParentNode node) {
            super(sequence);
            buffer = new FastStringBuffer(100);
            this.node = node;
        }

        public Receiver getGatherer() {
            return new StringValueGatherer(buffer);
        }

        @Override
        public Sequence<?> deliver() throws XPathException {
            node.setStringValue(buffer);
            return node.atomize();
        }

        @Override
        public void rollback() {
            buffer.setLength(0);
        }
    }
}

// Copyright (c) 2009-2014 Saxonica Limited
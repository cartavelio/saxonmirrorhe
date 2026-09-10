package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.str.UnicodeBuilder;

/**
 * This class is used when a streamed template requests evaluation of the string value
 * of the nodes selected by a path expression, typically in response to an xsl:value-of instruction.
 * The expression delivers a sequence of strings, one for each selected node.
 */


public class StringValueFeed extends AbsorptionFeed {


    public StringValueFeed(ItemFeed result, XPathContext context) {
        super(result, context);
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) {
        return new PendingStringValue(valuesReadyToBeOutput.size(), node);

    }

    @Override
    protected Sequence processGroundedItem(Item item) throws XPathException {
        return new StringValue(item.getUnicodeStringValue());
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope.
     *
     * @throws net.sf.saxon.trans.XPathException
     *          May be raised if a dynamic error occurs
     */
    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            getNextOutputter().close();
        }
    }

    /**
     * A pending item represents either a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private static class PendingStringValue extends PendingItem {
        public UnicodeBuilder buffer;
        public FleetingParentNode node;

        public PendingStringValue(int sequence, FleetingParentNode node) {
            super(sequence);
            buffer = new UnicodeBuilder();
            this.node = node;
        }

        @Override
        public Receiver getGatherer() {
            return new StringValueGatherer(buffer);
        }

        @Override
        public Sequence deliver() {
            return new StringValue(buffer.toUnicodeString());
        }

        @Override
        public void rollback() {
            buffer.clear();
        }
    }
}

// Copyright (c) 2009-2022 Saxonica Limited

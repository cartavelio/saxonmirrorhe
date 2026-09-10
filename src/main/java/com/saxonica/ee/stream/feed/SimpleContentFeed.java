package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;


/**
 * This class is used when a streaming template requests evaluation of a simple content constructor
 * in streaming mode (that is, when it constructs a text, attribute, comment, or PI node based on the
 * contents of the streamed document).
 * <p>The result of this Watch is a single string, constructed using the rules of "Constructing Simple Content"
 * in the XSLT specification.</p>
 * <p>The AdjacentTextNodeMergerFeed is used when the node is constructed as a result of a computation on the streamed data,
 * for example <code>xsl:value-of select="distinct-values(.//@price)"</code>. By contrast, a SimpleContentWatch
 * is used when the streamed nodes are selected directly, for example <code>xsl:value-of select=".//@price"</code>.</p>
 * <p>Both a Feed and a Watch are needed because a simple content constructor operates on a sequence of nodes without
 * simply atomizing the sequence.</p>
 */

public class SimpleContentFeed extends AbsorptionFeed {



    public SimpleContentFeed(ItemFeed result) {
        super(result, null);
    }

    @Override
    protected PendingItem makePendingItem(int sequenceNr, FleetingParentNode node) {
        return new PendingContent(sequenceNr, node);
    }


    @Override
    protected Sequence processGroundedItem(Item item) throws XPathException {
        if (item instanceof NodeInfo && ((NodeInfo)item).getNodeKind() == Type.TEXT) {
            // discard zero-length text nodes
            UnicodeString cs = item.getUnicodeStringValue();
            if (cs.isEmpty()) {
                return EmptySequence.getInstance();
            } else {
                return item.atomize();
            }
        } else {
            if (item instanceof AtomicValue) {
                return item;
            } else if (item instanceof NodeInfo) {
                return item.atomize();
            } else {
                XPathException xe = new XPathException(
                    "Cannot atomize item of type " + Type.displayTypeName(item), "FOTY0013", getExpression().getLocation());
                xe.setIsTypeError(true);
                throw xe;
            }
        }
    }

    /**
     * A pending item represents a tree under construction.
     * The sequence number represents its position relative to other trees under construction.
     * The builder is the builder being used to construct the tree.
     */

    private static class PendingContent extends PendingItem {
        public UnicodeBuilder buffer;
        public FleetingParentNode node;

        public PendingContent(int sequence, FleetingParentNode node) {
            super(sequence);
            buffer = new UnicodeBuilder();
            this.node = node;
        }

        @Override
        public Receiver getGatherer() {
            return new StringValueGatherer(buffer);
        }

        @Override
        public Sequence deliver() throws XPathException {
            if (node instanceof FleetingElementNode) {
                FleetingElementNode element = (FleetingElementNode) node;
                element.setStringValue(buffer.toUnicodeString());
                return element.atomize();
            } else {
                FleetingDocumentNode document = (FleetingDocumentNode) node;
                document.setStringValue(buffer.toUnicodeString());
                return document.atomize();
            }
        }

        @Override
        public void rollback() {
            buffer.clear();
        }
    }
}


// Copyright (c) 2009-2022 Saxonica Limited
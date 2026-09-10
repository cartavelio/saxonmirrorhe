package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.SimpleNodeConstructor;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.tree.util.Orphan;

/**
 * This class is used when a streaming template requests evaluation of a simple content constructor
 * in streaming mode (that is, when it constructs a text, attribute, comment, or PI node based on the
 * contents of the streamed document).
 * <p>The SimpleContentFeed is used when the node is constructed as a result of a computation on the streamed data,
 * for example <code>xsl:value-of select="distinct-values(.//@price)"</code>. By contrast, a SimpleContentWatch
 * is used when the streamed nodes are selected directly, for example <code>xsl:value-of select=".//@price"</code>.</p>
 * <p>Both a Feed and a Watch are needed because a simple content constructor operates on a sequence of nodes without
 * simply atomizing the sequence.</p>
 */


public class SimpleNodeConstructorFeed extends ItemFeed {

    private FastStringBuffer buffer = new FastStringBuffer(100);
    private boolean disableOutputEscaping;

    public SimpleNodeConstructorFeed(Expression expr, Feed result, XPathContext context) {
        super(expr, result, context);
        disableOutputEscaping = expr instanceof ValueOf && ((ValueOf)expr).isDisableOutputEscaping();
    }

    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void processItem(Item<?> item) throws XPathException {
        if (!hasFailed()) {
            buffer.append(item.getStringValueCS());
        }
    }

    public void close() throws XPathException {
        if (!hasFailed()) {
            SimpleNodeConstructor constructor = (SimpleNodeConstructor) getExpression();
            NodeName nameCode;
            try {
                nameCode = constructor.evaluateNodeName(getContext());
            } catch (XPathException e) {
                dynamicError(e);
                return;
            }
            Orphan node = new Orphan(getContext().getConfiguration());
            node.setNodeKind((short) constructor.getItemType().getPrimitiveType());
            node.setNodeName(nameCode);
            node.setStringValue(buffer);
            if (disableOutputEscaping) {
                node.setDisableOutputEscaping(true);
            }
            buffer = new FastStringBuffer(100);
            getResult().processItem(node);
            super.close();
        }
    }



}

// Copyright (c) 2009-2014 Saxonica Limited
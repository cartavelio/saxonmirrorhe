package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NameOfNode;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.util.NamespaceIterator;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.Type;

/**
 * This event feed implements an implicit xsl:copy instruction used in a built-in template. The events
 * representing the start and end of the input element or document are written to the destination
 * receiver during the open() and close() calls. Between these two calls, the child instruction
 * representing the content of the document or element pushes its output to the same destination
 * receiver.
 * <p>The instruction-dependent code is contained in the processLeft() and processRight() methods </p>
 */
public class ShallowCopyEventFeed extends ItemFeed {

    Mode mode;

    public ShallowCopyEventFeed(Mode mode, Feed parentExpressionFeed, XPathContext context) {
        super(null, parentExpressionFeed, context);
        this.mode = mode;
    }

    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        XPathContext context = getContext();
        Receiver receiver = getResult().getReceiver();
        NodeInfo node = (NodeInfo) context.getContextItem();
        if (node.getNodeKind() == Type.DOCUMENT) {
            receiver.startDocument(0);
        } else {
            receiver.startElement(NameOfNode.makeName(node), AnyType.getInstance(), ExplicitLocation.UNKNOWN_LOCATION, 0);
            NamespaceIterator.sendNamespaces(node, receiver);

            AxisIterator attributes = node.iterateAxis(AxisInfo.ATTRIBUTE);
            if (attributes != EmptyIterator.OfNodes.THE_INSTANCE) {
                XPathContextMajor c2 = context.newContext();
                c2.setReceiver(getResult().getReceiver());
                c2.trackFocus(node.iterateAxis(AxisInfo.ATTRIBUTE));
                mode.applyTemplates(context.getLocalParameters(), context.getTunnelParameters(),
                                    c2, ExplicitLocation.UNKNOWN_LOCATION);
            }
        }
    }

    public void processItem(Item<?> item) throws XPathException {
        getResult().getReceiver().append(item);
    }

    public void close() throws XPathException {
        XPathContext context = getContext();
        Receiver receiver = getResult().getReceiver();
        NodeInfo node = (NodeInfo) context.getContextItem();
        if (node.getNodeKind() == Type.DOCUMENT) {
            receiver.endDocument();
        } else {
            receiver.endElement();
        }
        super.close();
    }
}

// Copyright (c) 2012 Saxonica Limited.




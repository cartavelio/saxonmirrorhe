package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

/**
 * This event feed supports instructions that create a document or element node. The events
 * representing the start and end of the element or document are written to the destination
 * receiver during the open() and close() calls. Between these two calls, the child instruction
 * representing the content of the document or element pushes its output to the same destination
 * receiver.
 * <p>The instruction-dependent code is contained in the processLeft() and processRight() methods</p>
 */
public abstract class ComplexNodeEventFeed extends ItemFeed {

    private Receiver out;
    private boolean open = false;

    public ComplexNodeEventFeed(Expression exp, Feed parentExpressionFeed, XPathContext context) {
        super(exp, parentExpressionFeed, context);
        //out = getResult().getReceiver();
    }

    public void open(Terminator terminator) throws XPathException {
        if (!open) {
            super.open(terminator);
            out = getResult().getReceiver();
            try {
                processLeft(getContext());
            } catch (XPathException e) {
                dynamicError(e);
            }
            open = true;
        }
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (open && !hasFailed()) {
            try {
                return getResult().startSelectedParentNode(node, locationId);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        return null;
    }

    public void processItem(Item<?> item) throws XPathException {
        if (item instanceof FleetingParentNode) {
            throw new XPathException(
                "Internal streaming error: streamed node cannot be used directly to construct output node",
                SaxonErrorCode.SXST0067, getExpression().getLocation());
        }
        if (open && !hasFailed()) {
            try {
                //out.characters(item.getStringValueCS(), getExpression().getLocation(), 0);
                out.append(item);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (open && !hasFailed()) {
            try {
                getResult().endSelectedParentNode(locationId);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
    }

    public void close() throws XPathException {
        if (open && !hasFailed()) {
            try {
                processRight(getContext());
            } catch (XPathException e) {
                dynamicError(e);
                return;
            }
            super.close();
            open = false;
        }
    }

    @Override
    public Receiver getReceiver() {
        return out;
    }

    protected void setReceiver(Receiver out) {
        this.out = out;
    }

    /**
     * In streaming mode, process the first half of the instruction (for example, to start a new document or element)
     *
     *
     * @param context The dynamic context
     * @throws XPathException if a dynamic error occurs
     */

    abstract protected void processLeft(XPathContext context) throws XPathException;

    /**
     * In streaming mode, process the right half of the instruction (for example, to end a new document or element)
     *
     *
     * @param context The dynamic context
     * @throws XPathException if a dynamic error occurs
     */

    abstract protected void processRight(XPathContext context) throws XPathException;


}

// Copyright (c) 2009-2014 Saxonica Limited.




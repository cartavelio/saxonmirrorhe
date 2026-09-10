package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.Mode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.Untyped;

/**
 * This event feed implements an implicit xsl:copy instruction used in a built-in template. The events
 * representing the start and end of the input element or document are written to the destination
 * receiver during the open() and close() calls. Between these two calls, the child instruction
 * representing the content of the document or element pushes its output to the same destination
 * receiver.
 * <p>The instruction-dependent code is contained in the processLeft() and processRight() methods </p>
 */
public class ShallowCopyEventFeed extends ItemFeed {

    private final Mode mode;

    public ShallowCopyEventFeed(Mode mode, ItemFeed parentExpressionFeed, XPathContext context) {
        super(null, parentExpressionFeed, context);
        this.mode = mode;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        XPathContext context = getContext();
        Outputter receiver = getNextOutputter();
        XPathContextMajor c2;
        NodeInfo node = (NodeInfo) context.getContextItem();
        if (node.getNodeKind() == Type.DOCUMENT) {
            receiver.startDocument(ReceiverOption.NONE);
        } else {
            c2 = context.newContext();
            boolean schemaAware = context.getController().getExecutable().isSchemaAware();
            SchemaType schemaType = schemaAware ? AnyType.getInstance() : Untyped.getInstance();
            receiver.startElement(NameOfNode.makeName(node), schemaType, Loc.NONE, ReceiverOption.NONE);
            for (NamespaceBinding ns : node.getAllNamespaces()) {
                receiver.namespace(ns.getPrefix(), ns.getNamespaceUri(), ReceiverOption.NONE);
            }
            AxisIterator attributes = node.iterateAxis(AxisInfo.ATTRIBUTE);
            if (attributes != EmptyIterator.ofNodes()) {
                c2.trackFocus(attributes);
                mode.applyTemplates(context.getLocalParameters(), context.getTunnelParameters(),
                                    null, receiver, c2, Loc.NONE);
            }
        }
    }

    @Override
    public void append(Item item) throws XPathException {
        getNextOutputter().append(item);
    }

    @Override
    public void close() throws XPathException {
        XPathContext context = getContext();
        NodeInfo node = (NodeInfo) context.getContextItem();
        if (node.getNodeKind() == Type.DOCUMENT) {
            getNextOutputter().endDocument();
        } else {
            getNextOutputter().endElement();
        }
        super.close();
    }
}

// Copyright (c) 2012-2023 Saxonica Limited




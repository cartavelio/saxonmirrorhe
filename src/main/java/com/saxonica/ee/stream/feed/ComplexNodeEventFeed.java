package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.Terminator;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringConstants;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AtomicValue;

/**
 * This event feed supports instructions that create a document or element node. The events
 * representing the start and end of the element or document are written to the destination
 * receiver during the open() and close() calls. Between these two calls, the child instruction
 * representing the content of the document or element pushes its output to the same destination
 * receiver.
 * <p>The instruction-dependent code is contained in the processLeft() and processRight() methods</p>
 */
public abstract class ComplexNodeEventFeed extends ItemFeed {

    private boolean opened = false;
    private boolean previousAtomic = false;
    protected Outputter next;

    public ComplexNodeEventFeed(Expression exp, ItemFeed parentExpressionFeed, XPathContext context) {
        super(exp, parentExpressionFeed, context);
        next = parentExpressionFeed;
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        if (!opened) {
            super.open(terminator);
            try {
                processLeft(getContext());
            } catch (XPathException e) {
                dynamicError(e);
            }
            opened = true;
        }
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        return null;
    }

    @Override
    public void append(Item item) throws XPathException {
        if (item instanceof FleetingParentNode) {
            throw new XPathException(
                "Internal streaming error: streamed node cannot be used directly to construct output node",
                SaxonErrorCode.SXST0067, getExpression().getLocation());
        }
        if (opened && !hasFailed()) {
            try {
                Outputter out = next;
                if (item instanceof NodeInfo) {
                    NodeInfo node = (NodeInfo)item;
                    int kind = node.getNodeKind();
                    if (kind == Type.ATTRIBUTE) {
                        out.attribute(NameOfNode.makeName(node), (SimpleType)node.getSchemaType(),
                                      node.getStringValue(), getExpression().getLocation(), ReceiverOption.NONE);
                    } else {
                        ((NodeInfo) item).copy(out, CopyOptions.ALL_NAMESPACES | CopyOptions.TYPE_ANNOTATIONS, getExpression().getLocation());
                    }
                    previousAtomic = false;
                } else if (item instanceof AtomicValue) {
                    if (previousAtomic) {
                        out.characters(StringConstants.SINGLE_SPACE, Loc.NONE, ReceiverOption.SEPARATOR);
                    }
                    out.characters(item.getUnicodeStringValue(), getExpression().getLocation(), ReceiverOption.NONE);
                    previousAtomic = true;
                } else if (item instanceof ArrayItem) {
                    for (GroundedValue members : ((ArrayItem)item).members()) {
                        for (Item it : members.asIterable()) {
                            append(it);
                        }
                    }
                } else {
                    throw new XPathException("Cannot write a function item to the content of an XDM tree", "XTDE0450");
                }
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        // no action
    }

    @Override
    public void close() throws XPathException {
        if (opened && !hasFailed()) {
            try {
                processRight(getContext());
            } catch (XPathException e) {
                dynamicError(e);
                return;
            }
            super.close();
            opened = false;
        }
    }

    protected void setComplexContentOutputter(ComplexContentOutputter out) {
        //this.out = out;
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

//    /**
//     * Supply one item towards the streamed input of the expression
//     *
//     * @param item the item to be supplied
//     */
//    @Override
//    public void append(Item item) throws XPathException {
//        if (!hasFailed()) {
//            try {
//                next.append(item);
//            } catch (XPathException e) {
//                dynamicError(e);
//            }
//        }
//    }

    @Override
    public void startDocument(int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.startDocument(properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void endDocument() throws XPathException {
        if (!hasFailed()) {
            try {
                next.endDocument();
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.startElement(elemName, typeCode, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type, AttributeMap attributes, NamespaceMap namespaces, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.startElement(elemName, type, attributes, namespaces, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.namespace(prefix, namespaceUri, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.attribute(attName, typeCode, value, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void startContent() throws XPathException {
        if (!hasFailed()) {
            try {
                next.startContent();
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void endElement() throws XPathException {
        if (!hasFailed()) {
            try {
                next.endElement();
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void characters(UnicodeString chars, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.characters(chars, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void processingInstruction(String name, UnicodeString data, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.processingInstruction(name, data, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }

    @Override
    public void comment(UnicodeString content, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            try {
                next.comment(content, location, properties);
            } catch (XPathException e) {
                dynamicError(e);
            }
        }
        previousAtomic = false;
    }


}

// Copyright (c) 2009-2022 Saxonica Limited




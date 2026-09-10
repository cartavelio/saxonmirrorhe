package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.SimpleNodeConstructor;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.CharSequenceConsumer;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;

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
    private boolean isSimpleText;

    public SimpleNodeConstructorFeed(Expression expr, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        disableOutputEscaping = expr instanceof ValueOf && ((ValueOf)expr).isDisableOutputEscaping();
        SimpleNodeConstructor constructor = (SimpleNodeConstructor) getExpression();
        int kind = constructor.getItemType().getPrimitiveType();
        if (kind == Type.TEXT && !disableOutputEscaping) {
            isSimpleText = true;
        }
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!hasFailed()) {
            if (isSimpleText) {
                try {
                    getNextOutputter().characters(item.getStringValueCS(), Loc.NONE, ReceiverOption.NONE);
                } catch (XPathException e) {
                    dynamicError(e);
                }
            } else {
                buffer.cat(item.getStringValueCS());
            }
        }
    }

    @Override
    public CharSequenceConsumer getStringReceiver(boolean asTextNode) {
        if (isSimpleText) {
            return new CharSequenceConsumer() {
                @Override
                public CharSequenceConsumer cat(CharSequence chars) throws XPathException {
                    try {
                        getNextOutputter().characters(chars, Loc.NONE, ReceiverOption.NONE);
                    } catch (XPathException e) {
                        dynamicError(e);
                    }
                    return this;
                }
            };
        } else {
            return super.getStringReceiver(asTextNode);
        }
    }

    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            SimpleNodeConstructor constructor = (SimpleNodeConstructor) getExpression();
            int kind = constructor.getItemType().getPrimitiveType();
            switch (kind) {
                case Type.ATTRIBUTE:
                    try {
                        getNextOutputter().attribute(
                                constructor.evaluateNodeName(getContext()),
                                BuiltInAtomicType.UNTYPED_ATOMIC,
                                buffer.toString(),
                                Loc.NONE,
                                ReceiverOption.NONE);
                    } catch (XPathException e) {
                        dynamicError(e);
                    }
                    break;
                case Type.TEXT:
                    if (!isSimpleText) {
                        try {
                            int options = disableOutputEscaping? ReceiverOption.DISABLE_ESCAPING : ReceiverOption.NONE;
                            getNextOutputter().characters(buffer, Loc.NONE, options);
                        } catch (XPathException e) {
                            dynamicError(e);
                        }
                    }
                    break;
                case Type.COMMENT:
                    try {
                        getNextOutputter().comment(
                                buffer.toString(),
                                Loc.NONE,
                                ReceiverOption.NONE);
                    } catch (XPathException e) {
                        dynamicError(e);
                    }
                    break;
                case Type.PROCESSING_INSTRUCTION:
                    try {
                        getNextOutputter().processingInstruction(
                                constructor.evaluateNodeName(getContext()).getLocalPart(),
                                buffer.toString(),
                                Loc.NONE,
                                ReceiverOption.NONE);
                    } catch (XPathException e) {
                        dynamicError(e);
                    }
                    break;
                default:
                    throw new IllegalArgumentException("Unknown node kind " + kind);
            }
            buffer = new FastStringBuffer(100);
            super.close();
        }
    }



}

// Copyright (c) 2009-2020 Saxonica Limited
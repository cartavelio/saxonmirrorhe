package com.saxonica.ee.stream.feed;

import net.sf.saxon.Controller;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOptions;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.event.TreeReceiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ElementCreator;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;

/**
 * This event feed supports instructions that create a document or element node. The events
 * representing the start and end of the element or document are written to the destination
 * receiver during the open() and close() calls. Between these two calls, the child instruction
 * representing the content of the document or element pushes its output to the same destination
 * receiver.
 * <p>The instruction-dependent code is contained in the processLeft() and processRight() methods </p>
 */
public class ElementCreatorFeed extends ComplexNodeEventFeed {


    public ElementCreatorFeed(Expression exp, Feed parentExpressionFeed, XPathContext context) {
        super(exp, new FeedToReceiver(parentExpressionFeed, context), context);
    }

    /**
     * In streaming mode, process the first half of the instruction (for example, to start a new document or element)
     *
     * @param context The dynamic context
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */
    @Override
    protected void processLeft(XPathContext context) throws XPathException {
        generateElementStart(this, context);
    }

    /**
     * In streaming mode, process the right half of the instruction (for example, to end a new document or element)
     *
     * @param context The dynamic context
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs
     */
    @Override
    protected void processRight(XPathContext context) throws XPathException {
        generateElementEnd(this, context);
    }

    /**
     * In streaming mode, process the first half of an element creation instruction (to start a new element)
     *
     * @param feed the Feed that is processing the instruction in streaming mode
     * @param context XPathContext object.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
     */

    protected static void generateElementStart(ComplexNodeEventFeed feed, XPathContext context) throws XPathException {
        ElementCreator instr = (ElementCreator)feed.getExpression();
        try {
            NodeName elemName = instr.getElementName(context, null);
            SchemaType typeCode = instr.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();

            Receiver out = feed.getReceiver();
            Receiver elemOut = out;
            if (!instr.isPreservingTypes()) {
                Controller controller = context.getController();
                assert controller != null;
                ParseOptions options = new ParseOptions(instr.getValidationOptions());
                options.setTopLevelElement(elemName.getStructuredQName());
                Receiver validator = context.getConfiguration().getElementValidator(out, options, instr.getLocation());

                if (validator != out) {
                    out = new TreeReceiver(validator);
                    context.setReceiver(out);
                }
                elemOut = out;
            }

            if (elemOut.getSystemId() == null) {
                elemOut.setSystemId(instr.getNewBaseURI(context, null));
            }
            int properties = instr.isInheritNamespacesToChildren() ? 0 : ReceiverOptions.DISINHERIT_NAMESPACES;
            if (elemOut instanceof SequenceReceiver) {
                feed.setReceiver((SequenceReceiver) elemOut);
            } else {
                feed.setReceiver(new TreeReceiver(elemOut));
            }
            elemOut.startElement(elemName, typeCode, instr.getLocation(), properties);

            // output the required namespace nodes via a callback

            instr.outputNamespaceNodes(context, elemOut, elemName, null);

        } catch (XPathException e) {
            e.maybeSetLocation(instr.getLocation());
            e.maybeSetContext(context);
            throw e;
        }
    }

    /**
     * In streaming mode, process the right half of the instruction (for example, to end a new document or element)
     *
     * @param feed the feed to which the element end event should be sent
     * @param context XPathContext object.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs
     */

    protected static void generateElementEnd(ComplexNodeEventFeed feed, XPathContext context) throws XPathException {
        try {
            feed.getReceiver().endElement();
        } catch (XPathException e) {
            feed.dynamicError(e);
        }
    }

    public String toString() {
        return getExpression().toShortString();
    }


}

// Copyright (c) 2009-2014 Saxonica Limited.




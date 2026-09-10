package com.saxonica.ee.stream.feed;

import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
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


    public ElementCreatorFeed(Expression exp, ItemFeed parentExpressionFeed, XPathContext context) {
        super(exp, parentExpressionFeed, context);
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
        ElementCreator instr = (ElementCreator) getExpression();
        try {
            NodeName elemName = instr.getElementName(context, null);
            SchemaType typeCode = instr.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();

            if (!instr.isPreservingTypes()) {
//                Controller controller = context.getController();
//                assert controller != null;
                ParseOptions options = new ParseOptions(instr.getValidationOptions());
                options.setTopLevelElement(elemName.getStructuredQName());
                Receiver validator = context.getConfiguration().getElementValidator(
                        next, options, instr.getLocation());
                next = new ComplexContentOutputter(validator);
            }

            if (next.getSystemId() == null) {
                next.setSystemId(instr.getNewBaseURI(context, null));
            }
            boolean condition = !instr.isBequeathNamespacesToChildren();
            int properties =
                    condition ? ReceiverOption.DISINHERIT_NAMESPACES : ReceiverOption.NONE;
//            if (elemOut instanceof SequenceReceiver) {
//                feed.setReceiver(elemOut);
//            } else {
//                feed.setReceiver(new TreeReceiver2(elemOut));
//            }
            next.startElement(elemName, typeCode, instr.getLocation(), properties);

            // output the required namespace nodes via a callback

            instr.outputNamespaceNodes(next, elemName, null);

        } catch (XPathException e) {
            e.maybeSetLocation(instr.getLocation());
            e.maybeSetContext(context);
            throw e;
        }
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
        try {
            endElement();
        } catch (XPathException e) {
            dynamicError(e);
        }
    }

    public String toString() {
        return getExpression().toShortString();
    }



}

// Copyright (c) 2009-2022 Saxonica Limited




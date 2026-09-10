
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.functions.TreatFn;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.Cardinality;

/**
 * A Feed to go on the streaming pipeline that checks the cardinality of the sequence represented
 * by the events passing down the pipeline. This includes the ability to handled decomposed items
 * represented as a sequence of startElement/endElement calls.
 *
 * <p>This class handles two kinds of expression: a cardinality checker (which is generally implementing
 * the dynamic type check required by the function conversion rules), and the "TreatFn" functions
 * (exactly-one, zero-or-one, one-or-more) which perform a similar function but have slightly different
 * failure semantics.</p>
 */
public class CardinalityCheckingFeed extends ItemFeed {

    private int count = 0;
    private int requiredCardinality;
    private RoleDiagnostic role;

    public CardinalityCheckingFeed(Expression checker, Feed result, XPathContext context) {
        super(checker, result, context);
        if (checker instanceof CardinalityChecker) {
            requiredCardinality = ((CardinalityChecker) checker).getRequiredCardinality();
            role = ((CardinalityChecker) checker).getRoleLocator();
        } else if (checker.isCallOn(TreatFn.class)) {
            TreatFn fn = (TreatFn)((SystemFunctionCall)checker).getTargetFunction();
            requiredCardinality = fn.getRequiredCardinality();
            role = fn.makeRoleDiagnostic();
        } else {
            throw new IllegalArgumentException();
        }
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!hasFailed()) {
            checkCount();
            return getResult().startSelectedParentNode(node, locationId);
        }
        return null;
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (!hasFailed()) {
            getResult().endSelectedParentNode(locationId);
        }
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
            checkCount();
            getResult().processItem(item);
        }
    }

    private void checkCount() throws XPathException {
        if (!hasFailed() && count <= 2) {
            count++;
            if (count == 1 && requiredCardinality == StaticProperty.EMPTY) {
                typeError("The only value allowed for the " +
                        role.getMessage() + " is an empty sequence", role.getErrorCode());
            } else if (count == 2 && !Cardinality.allowsMany(requiredCardinality)) {
                typeError("A sequence of more than one item is not allowed as the " + role.getMessage(),
                        role.getErrorCode());
            }
        }
    }

    /**
     * Signal the end of the sequence of items
     *
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    public void close() throws XPathException {
        if (!hasFailed()) {
            if (count == 0 && !Cardinality.allowsZero(requiredCardinality)) {
                typeError("An empty sequence is not allowed as the " +
                        role.getMessage(), role.getErrorCode());
                return;
            }
            getResult().close();
        }
    }

    private void typeError(String message, String errorCode) throws XPathException {
        XPathException e = new XPathException(message, errorCode, getExpression().getLocation());
        e.setIsTypeError(getExpression() instanceof CardinalityChecker);
        dynamicError(e);
    }

    @Override
    public SequenceReceiver getReceiver() {
        // Unlike most expressions, a cardinality checker needs to take note of start/end element/document
        // events sent by the Receiver route, since these contribute to the item count;
        // see test cases such as si-copy-121. Top-level events sent to the target receiver are counted,
        // and all events are passed on unchanged.
        Receiver next = getResult().getReceiver();
        return new ProxyReceiver(next) {
            int level = 0;
            @Override
            public void startDocument(int properties) throws XPathException {
                if (level++ == 0) {
                    checkCount();
                }
                super.startDocument(properties);
            }

            @Override
            public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
                if (!hasFailed()) {
                    if (level++ == 0) {
                        checkCount();
                    }
                    super.startElement(elemName, typeCode, location, properties);
                }
            }

            @Override
            public void endElement() throws XPathException {
                if (!hasFailed()) {
                    level--;
                    super.endElement();
                }
            }

            @Override
            public void endDocument() throws XPathException {
                level--;
                super.endDocument();
            }

            @Override
            public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                    // This event might represent more than one node, but it doesn't matter for cardinality checking
                }
                super.namespace(namespaceBindings, properties);
            }

            @Override
            public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                }
                super.attribute(nameCode, typeCode, value, locationId, properties);
            }

            @Override
            public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                }
                super.characters(chars, locationId, properties);
            }

            @Override
            public void processingInstruction(String target, CharSequence data, Location locationId, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                }
                super.processingInstruction(target, data, locationId, properties);
            }

            @Override
            public void comment(CharSequence chars, Location locationId, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                }
                super.comment(chars, locationId, properties);
            }

            @Override
            public void append(Item item, Location locationId, int properties) throws XPathException {
                if (level == 0) {
                    checkCount();
                }
                super.append(item, locationId, properties);
            }
        };
    }
}

// Copyright (c) 2018 Saxonica Limited.


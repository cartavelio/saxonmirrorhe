
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.functions.TreatFn;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
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

    // TODO: simplify this by using a DelegatingFeed that delegates to a TypeCheckingFilter

    private int level = 0;
    private int count = 0;
    private final int requiredCardinality;
    private final RoleDiagnostic role;

    public CardinalityCheckingFeed(Expression checker, ItemFeed result, XPathContext context) {
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
            return getResultFeed().startSelectedParentNode(node, locationId);
        }
        return null;
    }

    @Override
    public void endSelectedParentNode(Location locationId) throws XPathException {
        if (!hasFailed()) {
            getResultFeed().endSelectedParentNode(locationId);
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
            checkCount();
            getNextOutputter().append(item);
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

    @Override
    public void close() throws XPathException {
        if (!hasFailed()) {
            if (count == 0 && !Cardinality.allowsZero(requiredCardinality)) {
                typeError("An empty sequence is not allowed as the " +
                        role.getMessage(), role.getErrorCode());
                return;
            }
            getNextOutputter().close();
        }
    }

    private void typeError(String message, String errorCode) throws XPathException {
        XPathException e = new XPathException(message, errorCode, getExpression().getLocation());
        e.setIsTypeError(getExpression() instanceof CardinalityChecker);
        dynamicError(e);
    }


    @Override
    public void startDocument(int properties) throws XPathException {
        if (level++ == 0) {
            checkCount();
        }
        super.startDocument(properties);
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            if (level++ == 0) {
                checkCount();
            }
            super.startElement(elemName, type, location, properties);
        }
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            if (level++ == 0) {
                checkCount();
            }
            super.startElement(elemName, type, attributes, namespaces, location, properties);
        }
    }

    @Override
    public void namespace(String prefix, String namespaceUri, int properties) throws XPathException {
        if (!hasFailed()) {
            if (level++ == 0) {
                checkCount();
            }
            super.namespace(prefix, namespaceUri, properties);
        }
    }

    @Override
    public void attribute(NodeName attName, SimpleType typeCode, String value, Location location, int properties) throws XPathException {
        if (!hasFailed()) {
            if (level++ == 0) {
                checkCount();
            }
            super.attribute(attName, typeCode, value, location, properties);
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
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (level == 0) {
            checkCount();
        }
        super.characters(chars, locationId, properties);
    }

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) throws XPathException {
        if (level == 0) {
            checkCount();
        }
        super.processingInstruction(target, data, locationId, properties);
    }

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (level == 0) {
            checkCount();
        }
        super.comment(chars, locationId, properties);
    }


}

// Copyright (c) 2018-2022 Saxonica Limited


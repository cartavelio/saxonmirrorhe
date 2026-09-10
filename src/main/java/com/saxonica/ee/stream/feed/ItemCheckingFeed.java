
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemChecker;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.*;

/**
 * A Feed to go on the streaming pipeline that checks the item type of the items in the sequence represented
 * by the events passing down the pipeline. This includes the ability to handled decomposed items
 * represented as a sequence of startElement/endElement calls.
 */
public class ItemCheckingFeed extends ItemFeed {

    // TODO: simplify this by using a DelegatingFeed that delegates to a TypeCheckingFilter

    private final ItemType requiredType;
    private final RoleDiagnostic role;
    private final Configuration config;
    private int level = 0;
    private final boolean allowAnyElement;

    public ItemCheckingFeed(Expression checker, ItemFeed result, XPathContext context) {
        super(checker, result, context);
        requiredType = ((ItemChecker) checker).getRequiredType();
        role = ((ItemChecker) checker).getRoleLocator();
        config = context.getConfiguration();
        Affinity aff = config.getTypeHierarchy().relationship(requiredType, NodeKindTest.ELEMENT);
        allowAnyElement = aff == Affinity.SAME_TYPE || aff == Affinity.SUBSUMES;
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!hasFailed()) {
            checkItemType(node);
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
            checkItemType(item);
            getNextOutputter().append(item);
        }
    }

    private void checkItemType(Item item) throws XPathException {
        final TypeHierarchy th = config.getTypeHierarchy();
        if (!hasFailed() && !requiredType.matches(item, th)) {

            String message = role.composeErrorMessage(requiredType, item, th);

            String errorCode = role.getErrorCode();
            if ("XPDY0050".equals(errorCode)) {
                // error in "treat as" assertion
                XPathException te = new XPathException(message, errorCode, getExpression().getLocation());
                te.setIsTypeError(false);
                dynamicError(te);
            } else {
                typeError(message, errorCode);
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
            getResultFeed().close();
        }
    }

    private void typeError(String message, String errorCode) throws XPathException {
        XPathException e = new XPathException(message, errorCode, getExpression().getLocation());
        e.setIsTypeError(true);
        dynamicError(e);
    }


    @Override
    public void startDocument(int properties) throws XPathException {
        if (level++ == 0) {
            checkItemType(new FleetingDocumentNode(-1));
        }
        super.startDocument(properties);
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             Location location, int properties) throws XPathException {
        if (level++ == 0 && !allowAnyElement) {
            Orphan node = new Orphan(config);
            node.setNodeKind(Type.ELEMENT);
            node.setNodeName(elemName);
            node.setTypeAnnotation(type);
            checkItemType(node);
        }
        super.startElement(elemName, type, location, properties);
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        if (level++ == 0 && !allowAnyElement) {
            Orphan node = new Orphan(config);
            node.setNodeKind(Type.ELEMENT);
            node.setNodeName(elemName);
            node.setTypeAnnotation(type);
            checkItemType(node);
        }
        super.startElement(elemName, type, attributes, namespaces, location, properties);
    }

    @Override
    public void endElement() throws XPathException {
        level--;
        super.endElement();
    }

    @Override
    public void endDocument() throws XPathException {
        level--;
        super.endDocument();
    }

}

// Copyright (c) 2018-2023 Saxonica Limited


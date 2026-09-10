
package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.om.FleetingDocumentNode;
import com.saxonica.ee.stream.om.FleetingElementNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceReceiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemChecker;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.TypeHierarchy;

/**
 * A Feed to go on the streaming pipeline that checks the item type of the items in the sequence represented
 * by the events passing down the pipeline. This includes the ability to handled decomposed items
 * represented as a sequence of startElement/endElement calls.
 */
public class ItemCheckingFeed extends ItemFeed {

    private ItemType requiredType;
    private RoleDiagnostic role;
    private Configuration config;

    public ItemCheckingFeed(Expression checker, Feed result, XPathContext context) {
        super(checker, result, context);
        requiredType = ((ItemChecker) checker).getRequiredType();
        role = ((ItemChecker) checker).getRoleLocator();
        config = context.getConfiguration();
    }

    @Override
    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        if (!hasFailed()) {
            checkItemType(node);
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
            checkItemType(item);
            getResult().processItem(item);
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

    public void close() throws XPathException {
        if (!hasFailed()) {
            getResult().close();
        }
    }

    private void typeError(String message, String errorCode) throws XPathException {
        XPathException e = new XPathException(message, errorCode, getExpression().getLocation());
        e.setIsTypeError(true);
        dynamicError(e);
    }

    @Override
    public SequenceReceiver getReceiver() {
        // Unlike most expressions, an item checker needs to take note of start/end element/document
        // events sent by the Receiver route, since these may be subject to type checking (typically,
        // when the consuming pipeline is within xsl:variable and the variable has a required type).
        // See test cases such as si-copy-130. Top-level nodes sent to the target receiver are checked,
        // and all events are passed on unchanged.
        Receiver next = getResult().getReceiver();

        final FleetingDocumentNode root = new FleetingDocumentNode(-1);
        root.setConfiguration(config);

        return new ProxyReceiver(next) {
            int level = 0;
            @Override
            public void startDocument(int properties) throws XPathException {
                if (level++ == 0) {
                    checkItemType(new FleetingDocumentNode(-1));
                }
                super.startDocument(properties);
            }

            @Override
            public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
                if (level++ == 0) {
                    FleetingElementNode node = new FleetingElementNode(elemName, typeCode);
                    node.setParent(root);  // needed to provide access to a namepool
                    checkItemType(node);
                }
                super.startElement(elemName, typeCode, location, properties);
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
        };
    }
}

// Copyright (c) 2018 Saxonica Limited.


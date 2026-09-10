package com.saxonica.ee.validate;

import com.saxonica.ee.stream.om.FleetingNode;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.PatternWatch;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Item;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;

/**
 * A FieldWatch is a class that is looking for nodes that match a field
 * selector within a uniqueness or key constraint.
 */

public class FieldWatch extends PatternWatch {

    private final SelectorWatch parentWatch;
    private final AtomicSequence[] currentRow;
    private final int column;

    /**
     * Create a new FieldWatch
     *
     * @param parentWatch The SelectorWatch to which field values should be
     *                    notified
     * @param selection   The selection (that is, the restricted XPath expression)
     *                    used in this field of the constraint
     * @param currentRow  represents the selected node whose fields are being
     *                    evaluated
     * @param column      sequence number for this field within the constraint being
     *                    evaluated
     */

    public FieldWatch(SelectorWatch parentWatch,
                      Pattern selection,
                      AtomicSequence[] currentRow,
                      int column) {
        this.parentWatch = parentWatch;
        setSelection(selection);
        this.currentRow = currentRow;
        this.column = column;
    }

    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the ConstraintChecker while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @throws ValidationException May be raised if the constraint implemented by this
     *                             Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @return a FieldChecker - a receiver that collects the contents of the selected node, or null
     */

    @Override
    public FieldChecker startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        FieldChecker fc = new FieldChecker(parentWatch, getSelection(), currentRow, column);
        fc.setPipelineConfiguration(getPipelineConfiguration());
        fc.setNamespaceResolver(getNamespaceResolver());
        return fc;
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @throws ValidationException May be raised if the constraint implemented by this
     *                             Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     * @param locationId the location of the node in the source, or of the instruction that created it
     */

    @Override
    public void endSelectedParentNode(Location locationId) throws ValidationException {
    }

    /**
     * Signal that a node (other than an element) that matches the selection
     * has been found.
     *
     * @param item       the node that was matched.
     *
     */

    @Override
    public void processItem(Item item) throws XPathException {
        FieldChecker fc = new FieldChecker(parentWatch, getSelection(), currentRow, column);
        fc.setPipelineConfiguration(getPipelineConfiguration());
        fc.setNamespaceResolver(getNamespaceResolver());
        fc.notifySelectedAttribute((FleetingNode) item, Loc.NONE);
    }
}

// Copyright (c) 2004-2022 Saxonica Limited

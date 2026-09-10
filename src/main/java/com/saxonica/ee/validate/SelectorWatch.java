package com.saxonica.ee.validate;

import com.saxonica.ee.schema.*;
import com.saxonica.ee.stream.om.FleetingParentNode;
import com.saxonica.ee.stream.watch.PatternWatch;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.InvalidityHandler;
import net.sf.saxon.lib.InvalidityHandlerWrappingErrorListener;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.StringValue;

import java.util.HashMap;
import java.util.Stack;

/**
 * A Watch represents a class that is interested in looking at nodes for the purpose
 * of evaluating uniqueness and key constraints. A SelectorWatch looks for the nodes
 * that match the selector in a unique, key, or keyref constraint. There are two
 * concrete subclasses: one for key/unique, the other for keyref
 */

public abstract class SelectorWatch extends PatternWatch {

    IdentityConstraint identityConstraint;
    ConstraintChecker constraintChecker;
    int numberOfColumns;
    AtomicSequence[] currentRow;
    Stack<AtomicSequence[]> activationStack = null;
    HashMap<ValueEntry, Integer> table = new HashMap<ValueEntry, Integer>(100);

    public SelectorWatch(ConstraintChecker checker, IdentityConstraint uniqueConstraint) {
        identityConstraint = uniqueConstraint;
        constraintChecker = checker;
        setSelection(uniqueConstraint.getSelector().getSelection());
        numberOfColumns = uniqueConstraint.getFields().size();
    }

    /**
     * Get the identityConstraint implemented by this SelectorWatch
     *
     * @return the identity constraint
     */

    public IdentityConstraint getIdentityConstraint() {
        return identityConstraint;
    }

    /**
     * Signal that an element has been found that matches the selection that this
     * Watch is looking for. This method is called by the ConstraintChecker while
     * processing the startElement event that matches the selection. For this purpose,
     * any attributes selected by the last step in the selection are ignored
     *
     * @return a Receiver to be notified of events for the subtree rooted at the element
     * matched by this Watch. A SelectorWatch does not require such notification, so it
     * returns null. @param locationId the location of the node in the source, or of the instruction that created it
     * @throws ValidationException May be raised if the constraint implemented by this
     *                             Watch is violated
     */

    public Receiver startSelectedParentNode(FleetingParentNode node, Location locationId) throws XPathException {
        // if there is already an existing activation, we are dealing
        // with nested elements that each contain key values
        if (currentRow != null) {
            if (activationStack == null) {
                activationStack = new Stack<AtomicSequence[]>();
            }
            activationStack.push(currentRow);
        }
        currentRow = new AtomicSequence[numberOfColumns];
        int column = 0;
        WatchManager fieldHandler = constraintChecker;
        for (IdentityField field : identityConstraint.getFields()) {
            Pattern fieldSelection = field.getSelection();
            FieldWatch fieldWatch =
                    new FieldWatch(this, fieldSelection, currentRow, column++);
            fieldWatch.setAnchorNode(node);
            fieldWatch.setNamespaceResolver(getNamespaceResolver());
            fieldHandler.addWatch(fieldWatch, true);
        }
        return null;
    }

    /**
     * Signal that the endElement event has occurred for the element whose startElement
     * event caused the Watch to be activated.
     *
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @throws ValidationException May be raised if the constraint implemented by this
     *                             Watch is violated @param locationId the location of the node in the source, or of the instruction that created it
     */

    public void endSelectedParentNode(Location locationId) throws XPathException {
        checkRow(currentRow, false, locationId);
        if (activationStack != null && !activationStack.isEmpty()) {
            currentRow = activationStack.pop();
        } else {
            currentRow = null;
        }
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope.
     */

    public void close() throws XPathException {
    }

    /**
     * The addFieldValue method is a callback from the FieldWatch created when this
     * SelectorWatch is activated. It indicates that a value for one of the
     * fields participating in this constraint has been encountered.
     *
     * @param currentRow      identifying the node to which this field value
     *                        applies
     * @param column          identifies which of the fields participating in this constraint
     *                        the value relates to
     * @param value           The value of the field that was encountered
     * @param nillableElement True if the field represents a nillable element
     * @param locationId      the location of the data in the source document
     * @throws ValidationException May be raised if the value of the field violates
     *                             the constraint
     */

    public void addFieldValue(AtomicSequence[] currentRow, int column, AtomicSequence value,
                              boolean nillableElement, Location locationId) throws XPathException {
        if (currentRow[column] != null) {
            ValidationFailure ve = new ValidationFailure("Identity constraint " +
                                                                 Err.wrap(identityConstraint.getName()) +
                                                                 ": the " +
                                                                 identifyField(column) +
                                                                 " selects more than one node");
            ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
            ve.setConstraintReference(1, "cvc-identity-constraint", "3");
            reportValidationError(ve, locationId);
        }
        if (nillableElement && identityConstraint instanceof Key) {
            // for <key>, it's a validity error if the element declaration was nillable.
            // See Rule 4.2.3 in section 3.11.4 of Schema Part 1
            ValidationFailure ve = new ValidationFailure(
                    "The element declaration of a field in an xs:key constraint must not be nillable");
            ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
            ve.setConstraintReference(1, "cvc-identity-constraint", "4.2.3");
            reportValidationError(ve, locationId);
            //throw ve;
        }
        currentRow[column] = value;
    }

    /**
     * Identify a field for diagnostics
     *
     * @param column the 0-based index of the field among all the fields selected
     * @return a string that can be used to identify one of the fields in diagnostics
     */

    private String identifyField(int column) {
        if (numberOfColumns == 1) {
            return "field";
        } else if (column == 0) {
            return "first field";
        } else if (column == 1) {
            return "second field";
        } else if (column == 2) {
            return "third field";
        } else if (column < 20) {
            return (column + 1) + "th field";
        } else {
            return "field " + (column + 1);
        }
    }

    /**
     * Method to check that a row (that is, the collection of fields corresponding
     * to one selected node) is valid.
     *
     * @param currentRow the values of the set of fields
     * @param isTarget   set to true if this is a value of the key referred to
     *                   by a key reference, rather than a value for this constraint's own selected
     *                   fields.
     * @param locationId identifies the location of the selected node
     * @throws ValidationException is raised if the value of the row (that is,
     *                             the combination of fields) violates the constraint
     */

    protected void checkRow(AtomicSequence[] currentRow, boolean isTarget, Location locationId) throws XPathException {
        for (int i = 0; i < numberOfColumns; i++) {
            if (currentRow[i] == null) {
                // A field is missing
                if (isTarget) {
                    return;
                } else if (identityConstraint instanceof Unique) {
                    // for <unique>, the node is ignored unless all fields are present
                    return;
                } else if (identityConstraint instanceof Key) {
                    // for <key>, an absent field is a validity error
                    ValidationFailure ve = new ValidationFailure(
                            "The " + identifyField(i) + " in constraint " +
                                    Err.wrap(identityConstraint.getName()) +
                                    " has no value");
                    ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                    ve.setConstraintReference(1, "cvc-identity-constraint", "4.2.1");
                    reportValidationError(ve, locationId);
                    //throw ve;
                } else {
                    // for <keyref>, the node is ignored unless all fields are present
                    return;
                }
            } else {
                //hash.append(currentRow[i].getSchemaComparable().hashCode());
            }
        }

        ValueEntry entry = ValueEntry.makeValueEntry(currentRow);
        Integer existingInfo = table.get(entry);
        if (existingInfo != null) {
            int flags = existingInfo;
            if (isTarget) {
                int count = flags >> 16;
                count++;
                flags = (count << 16) | (flags & 0xffff);
                if (count > 1 && ((flags & 1) != 0)) {
                    ValidationFailure ve = new ValidationFailure(
                            "More than one referenced value found for keyRef " +
                                    Err.wrap(identityConstraint.getName()) +
                                    "\n    " + entry);
                    ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                    ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                    reportValidationError(ve, locationId);
                }
                table.put(entry, flags);
            } else if (identityConstraint instanceof KeyRef) {
                // for <keyref>, we can ignore duplicates; however,
                // we need to mark the entry to show that at least one
                // reference exists
                flags |= 1;
                if ((flags >> 16) > 1) {
                    ValidationFailure ve = new ValidationFailure(
                            "More than one referenced value found for keyRef " +
                                    Err.wrap(identityConstraint.getName()) +
                                    "\n    " + entry);
                    ve.setErrorCode(constraintChecker.getValidationContext().getErrorCode());
                    ve.setConstraintReference(1, "cvc-identity-constraint", "3");
                    reportValidationError(ve, locationId);
                }
                table.put(entry, flags);
            } else {
                ValidationFailure ve = new ValidationFailure(
                        "Non-unique value found for constraint " +
                                identityConstraint.getName() +
                                ": " + entry);
                String err = constraintChecker.getValidationContext().getErrorCode();
//                if (err == null || err.startsWith("XTTE")) {
//                    err = "XTTE1555"; // more specific error code in XSLT
//                }
                ve.setErrorCode(err);
                ve.setConstraintReference(1, "cvc-identity-constraint",
                                          identityConstraint instanceof Unique ? "4.1" : "4.2.2");
                reportValidationError(ve, locationId);
            }
        } else {
            Integer details = isTarget ? REFERENCED_VALUE : PRESENT_VALUE;
            table.put(entry, details);
        }
    }

    private static final Integer REFERENCED_VALUE = 0x10000;
    private static final Integer PRESENT_VALUE = 1;

    /**
     * An entry in the key table, representing the values of a set of selected fields.
     * To economize on space in the table, there are subclasses handling the common special
     * cases where there are four or fewer fields; this avoids creating the extra array object
     * which is used in the general case.
     */

    static abstract class ValueEntry {

        public static ValueEntry makeValueEntry(AtomicSequence[] values) {
            switch (values.length) {
                case 1:
                    return new ValueEntry1(values[0]);
                case 2:
                    return new ValueEntry2(values[0], values[1]);
                case 3:
                    return new ValueEntry3(values[0], values[1], values[2]);
                case 4:
                    return new ValueEntry4(values[0], values[1], values[2], values[3]);
                default:
                    return new MultiValueEntry(values);
            }
        }

        protected boolean compareValues(AtomicSequence val0, AtomicSequence val1) {
            if (val0 == null) {
                return val1 == null;
            } else {
                return val1 != null && val0.getSchemaComparable().equals(val1.getSchemaComparable());
            }
        }

        protected String displayValue(AtomicSequence val) {
            if (val == null) {
                return "()";
            } else {
                return val.toString();
            }
        }

        protected int hash(AtomicSequence value) {
            return value == null ? 0 : value.getSchemaComparable().hashCode();
        }
    }

    private static class MultiValueEntry extends ValueEntry {

        /**
         * An array of values, one for each of the fields named in the constraint
         */
        AtomicSequence[] values;

        public MultiValueEntry(AtomicSequence[] values) {
            this.values = values;
            for (int i = 0; i < values.length; i++) {
                if (values[i] instanceof StringValue) {
                    values[i] = new CompactStringValue(values[i].getStringValue());
                }
            }
        }

        @Override
        public boolean equals(Object obj) {
            if (obj instanceof MultiValueEntry && ((MultiValueEntry) obj).values.length == values.length) {
                for (int i = 0; i < values.length; i++) {
                    if (!compareValues(values[i], ((MultiValueEntry) obj).values[i])) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }

        @Override
        public int hashCode() {
            int h = 0xa012b876;
            for (AtomicSequence a : values) {
                h ^= hash(a);
            }
            return h;
        }

        public String toString() {
            FastStringBuffer sb = new FastStringBuffer(100);
            sb.append('(');
            for (int i = 0; i < values.length; i++) {
                if (i != 0) {
                    sb.append(", ");
                }
                sb.append(displayValue(values[i]));
            }
            sb.append(')');
            return sb.toString();
        }
    }

    private static class ValueEntry1 extends ValueEntry {

        AtomicSequence value;

        public ValueEntry1(AtomicSequence value) {
            if (value instanceof StringValue) {
                this.value = new CompactStringValue(value.getStringValue());
            } else {
                this.value = value;
            }
        }

        @Override
        public boolean equals(Object obj) {
            return obj instanceof ValueEntry1 &&
                    compareValues(value, ((ValueEntry1) obj).value);
        }

        @Override
        public int hashCode() {
            return 0xa012b876 ^ hash(value);
        }

        public String toString() {
            return displayValue(value);
        }
    }

    private static class ValueEntry2 extends ValueEntry {

        AtomicSequence value0;
        AtomicSequence value1;

        public ValueEntry2(AtomicSequence value0, AtomicSequence value1) {
            if (value0 instanceof StringValue) {
                this.value0 = new CompactStringValue(value0.getStringValue());
            } else {
                this.value0 = value0;
            }
            if (value1 instanceof StringValue) {
                this.value1 = new CompactStringValue(value1.getStringValue());
            } else {
                this.value1 = value1;
            }
        }

        @Override
        public boolean equals(Object obj) {
            return obj instanceof ValueEntry2 &&
                    compareValues(value0, ((ValueEntry2) obj).value0) &&
                    compareValues(value1, ((ValueEntry2) obj).value1);
        }

        @Override
        public int hashCode() {
            return 0xa012b876 ^ hash(value0) ^ (hash(value1)*31);
        }

        public String toString() {
            return "(" + displayValue(value0) + ", " + displayValue(value1) + ")";
        }
    }

    private static class ValueEntry3 extends ValueEntry {

        AtomicSequence value0;
        AtomicSequence value1;
        AtomicSequence value2;

        public ValueEntry3(AtomicSequence value0, AtomicSequence value1, AtomicSequence value2) {
            if (value0 instanceof StringValue) {
                this.value0 = new CompactStringValue(value0.getStringValue());
            } else {
                this.value0 = value0;
            }
            if (value1 instanceof StringValue) {
                this.value1 = new CompactStringValue(value1.getStringValue());
            } else {
                this.value1 = value1;
            }
            if (value2 instanceof StringValue) {
                this.value2 = new CompactStringValue(value2.getStringValue());
            } else {
                this.value2 = value2;
            }
        }

        @Override
        public boolean equals(Object obj) {
            return obj instanceof ValueEntry3 &&
                    compareValues(value0, ((ValueEntry3) obj).value0) &&
                    compareValues(value1, ((ValueEntry3) obj).value1) &&
                    compareValues(value2, ((ValueEntry3) obj).value2);
        }

        @Override
        public int hashCode() {
            return 0xa012b876 ^ hash(value0) ^ (hash(value1)*31) ^ (hash(value2)*63);
        }

        public String toString() {
            return "(" + displayValue(value0) + ", " + displayValue(value1) + ", " + displayValue(value2) + ")";
        }
    }

    private static class ValueEntry4 extends ValueEntry {

        AtomicSequence value0;
        AtomicSequence value1;
        AtomicSequence value2;
        AtomicSequence value3;

        public ValueEntry4(AtomicSequence value0, AtomicSequence value1, AtomicSequence value2, AtomicSequence value3) {
            if (value0 instanceof StringValue) {
                this.value0 = new CompactStringValue(value0.getStringValue());
            } else {
                this.value0 = value0;
            }
            if (value1 instanceof StringValue) {
                this.value1 = new CompactStringValue(value1.getStringValue());
            } else {
                this.value1 = value1;
            }
            if (value2 instanceof StringValue) {
                this.value2 = new CompactStringValue(value2.getStringValue());
            } else {
                this.value2 = value2;
            }
            if (value3 instanceof StringValue) {
                this.value3 = new CompactStringValue(value3.getStringValue());
            } else {
                this.value3 = value3;
            }
        }

        @Override
        public boolean equals(Object obj) {
            return obj instanceof ValueEntry4 &&
                    compareValues(value0, ((ValueEntry4) obj).value0) &&
                    compareValues(value1, ((ValueEntry4) obj).value1) &&
                    compareValues(value2, ((ValueEntry4) obj).value2) &&
                    compareValues(value3, ((ValueEntry4) obj).value3);
        }

        @Override
        public int hashCode() {
            return 0xa012b876 ^ hash(value0) ^ (hash(value1)*31) ^ (hash(value2)*63) ^ (hash(value3)*127);
        }

        public String toString() {
            return "(" +
                    displayValue(value0) + ", " +
                    displayValue(value1) + ", " +
                    displayValue(value2) + ", " +
                    displayValue(value3) + ")";
        }
    }


    /**
     * Report a validation error
     *
     * @param err     The validation exception
     * @param locator identifies the location in the source document or stylesheet/query where the
     *                validation error was detected
     * @throws ValidationException if the threshold on number of errors is exceeded
     */
    protected void reportValidationError(ValidationFailure err, Location locator)
            throws XPathException {
        PipelineConfiguration pipe = getPipelineConfiguration();

        ValidationContext vc = constraintChecker.getValidationContext();
        InvalidityHandler handler = vc.getInvalidityHandler();
        if (handler == null) {
            handler = new InvalidityHandlerWrappingErrorListener(pipe.getErrorListener());
        }
        err.setHasBeenReported(true);
        err.setSourceLocator(locator);
        handler.reportInvalidity(err);

        if (vc.getErrorCount() == 0) {
            vc.setErrorCode(err.getErrorCode());
        }
        if (vc.incrementErrorCount(err)) {
            throw err.makeException();
        }
    }

}

// Copyright (c) 2004-2015 Saxonica Limited.

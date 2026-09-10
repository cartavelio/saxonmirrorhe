package com.saxonica.ee.validate;

import com.saxonica.ee.schema.KeyRef;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.type.ValidationFailure;

import java.util.Map;

/**
 * This class watches the selector for a keyref constraint
 */

public class RefSelectorWatch extends SelectorWatch {

    Location lastLocation = null;

    /**
     * Create a watch for values that might satisfy a keyref constraint
     *
     * @param checker    the ConstraintChecker that initiated this watch
     * @param constraint the constraint being checked
     */

    public RefSelectorWatch(ConstraintChecker checker, KeyRef constraint) {
        super(checker, constraint);
    }

    /**
     * Notify a value selected by this KeyRef
     *
     * @param keyValue   an array holding the components (fields) making up the keyref value
     * @param locationId the location of the node containing the keyref values
     * @throws ValidationException if the value is invalid for example a duplicate
     */

    public void notifyActualKey(AtomicSequence[] keyValue, Location locationId) throws XPathException {
        checkRow(keyValue, true, locationId);
        lastLocation = locationId.saveLocation();
    }

    /**
     * The close() method is called immediately before the Watch is destroyed, that is,
     * when the element whose declaration scopes the constraint implemented by this
     * Watch goes out of scope. This implementation searches the list of all key values
     * encountered looking (a) for key references that reference no corresponding key
     * value (b) for key references that reference more than one corresponding key
     * value (which can happen if the keyref and the key are defined at different
     * levels of hierarchy.
     */

    @Override
    public void close() throws XPathException {
        int errorCount = 0;
        for (Map.Entry<ValueEntry, Integer> e : table.entrySet()) {
            ValueEntry entry = e.getKey();
            int info = e.getValue();
            if ((info & 1) != 0) {
                // the value is referenced
                int count = (info >> 16) & 0xffff;
                if (count != 1) {
                    // Note: cvc-identity-constraint says there must be an entry in the node-table.
                    // 3.11.5 says there will be entry in the node-table provided that
                    // "no two entries have the same ·key-sequence· but distinct nodes."
                    // So there must be exactly one.
                    String card = count == 0 ? "No" : "More than one";
                    ValidationFailure ve = new ValidationFailure(
                            card + " referenced value found for keyRef " +
                                    Err.wrap(identityConstraint.getName()) +
                                    ": " + entry);
                    ve.setConstraintReference(1, "cvc-identity-constraint", "4.3");
                    ve.setErrorCode(getPipelineConfiguration().isXSLT() ? "XTTE1555" : "XQDY0027");
                    reportValidationError(ve, lastLocation);
                    if (errorCount++ > 100) {
                        break;
                    }
                }
            }
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.

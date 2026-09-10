package com.saxonica.ee.validate;

import com.saxonica.ee.schema.IdentityConstraint;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.List;

/**
 * This class watches the selector for a key or unique constraint
 */

public class KeySelectorWatch extends SelectorWatch {

    // A list of all keyref Watches that reference the unique/key constraint
    // handled by this KeySelectorWatch
    private final List<RefSelectorWatch> keyrefs = new ArrayList<>(4);

    public KeySelectorWatch(ConstraintChecker checker, IdentityConstraint uniqueConstraint) {
        super(checker, uniqueConstraint);
    }

    /**
     * Add a RefSelectorWatch to support a keyRef that refers to this key
     * or unique constraints
     *
     * @param watch the RefSelectorWatch which needs to be notified of all
     *              values encountered for this key
     */

    public void addKeyRef(RefSelectorWatch watch) {
        keyrefs.add(watch);
    }

    /**
     * Method to check that a row (that is, the collection of fields corresponding
     * to one selected node) is valid. This implentation augments that in the
     * superclass by notifying all interested keyref's of the values in this row.
     *
     * @throws net.sf.saxon.type.ValidationException
     *          May be raised if the value of the row (that is,
     *          the combination of fields) violates the constraint
     */

    @Override
    protected void checkRow(AtomicSequence[] currentRow, boolean isTarget, Location locationId) throws XPathException {
        super.checkRow(currentRow, isTarget, locationId);

        // Notify that a key value has been found to all KeyRef watchers that
        // refer to this key
        for (RefSelectorWatch watch : keyrefs) {
            watch.notifyActualKey(currentRow, locationId);
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited No warranty.

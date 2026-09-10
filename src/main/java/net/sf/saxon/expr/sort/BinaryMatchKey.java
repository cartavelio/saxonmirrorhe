////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.sort;

import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BinaryValue;
import net.sf.saxon.value.HexBinaryValue;

/**
 * A match key used when comparing binary values using the "eq" operator. In 4.0 the rules
 * for the "eq" operator have changed so a hexBinary and base64Binary value can compare
 * equal, but the rules for fn:atomic-equal (used in map keys) still make them distinct,
 * for backwards compatibility reasons. Therefore a binary value acts as its own match key
 * for "eq" purposes, but is wrapped in a `BinaryMatchKey` for comparisons in maps.
 */

// TODO: it might be better to do this the other way around, since for maps, the match key has to be
//   stored, while for "eq" comparisons it can be transient.

public class BinaryMatchKey implements AtomicMatchKey {

    private final BinaryValue value;
    public BinaryMatchKey(BinaryValue value) {
        this.value = value;
    }


    @Override
    public int hashCode() {
        return value.hashCode();
    }


    @Override
    public boolean equals(Object obj) {
        return obj instanceof BinaryMatchKey
                && value.equals(((BinaryMatchKey) obj).value)
                && value instanceof HexBinaryValue == ((BinaryMatchKey) obj).value instanceof HexBinaryValue;
    }

    /**
     * Get an atomic value that encapsulates this match key. Needed to support the collation-key() function.
     *
     * @return an atomic value that encapsulates this match key. NB: this is NOT (necessarily) the atomic value
     * from which the {@code AtomicMatchKey} was derived.
     */
    @Override
    public AtomicValue asAtomic() {
        return value;
    }
}

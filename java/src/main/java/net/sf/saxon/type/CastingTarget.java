////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.type;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.trans.XPathException;

/**
 * This marker interface represents a type that can be used as the target of a cast or castable
 * operation, that is, an atomic type, list type, or plain union type
 */

public interface CastingTarget {

    /**
     * Cast a value to this type
     *
     * @param it         the value to be cast
     * @param context    the dynamic context
     * @param resolver   the namespace prefix resolver, for namespace-sensitive types
     * @param allowEmpty true if an empty sequence is accepted
     * @return the result of the cast if successful
     * @throws XPathException if the cast is unsuccessful
     */

    default Item cast(Item it, XPathContext context, NamespaceResolver resolver, boolean allowEmpty) throws XPathException {
        throw new UnsupportedOperationException("Not implemented yet");
    };
}


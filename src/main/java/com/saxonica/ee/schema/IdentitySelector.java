package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaException;

/**
 * The selector of an IdentityConstraint
 */

public class IdentitySelector extends FieldOrSelector {

    /**
     * Create a new IdentitySelector.
     *
     * @param xpath the xpath for the IdentitySelector. Must not be null.
     * @throws net.sf.saxon.type.SchemaException
     *          if xpath is null.
     */
    public IdentitySelector(String xpath) throws SchemaException {
        setXPath(xpath);
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
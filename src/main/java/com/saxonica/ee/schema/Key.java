package com.saxonica.ee.schema;


/**
 * A class that represents the XML Schema Identity Constraint: Key.
 */

public class Key extends IdentityConstraint {

    @Override
    public String getConstraintCategory() {
        return "key";
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
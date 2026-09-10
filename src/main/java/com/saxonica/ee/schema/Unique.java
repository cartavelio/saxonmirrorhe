package com.saxonica.ee.schema;


/**
 * This class represents an xs:unique identity constraint
 */
public class Unique extends IdentityConstraint {

    @Override
    public String getConstraintCategory() {
        return "unique";
    }
}

// Copyright (c) 2004-2023 Saxonica Limited
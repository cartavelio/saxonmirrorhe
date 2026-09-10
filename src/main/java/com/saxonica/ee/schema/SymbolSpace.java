package com.saxonica.ee.schema;

/**
 * This class holds constants used to identify the various symbol spaces defined
 * in XML Schema: element declarations, attribute declarations, types, groups, and so on.
 */

public abstract class SymbolSpace {

    public static final int ELEMENT = 801;
    public static final int ATTRIBUTE = 802;
    public static final int TYPE = 803;
    public static final int GROUP = 804;
    public static final int ATTRIBUTE_GROUP = 805;
    public static final int NOTATION = 806;
    public static final int IDENTITY_CONSTRAINT = 807;

    private SymbolSpace() {
    }

    public static String getSymbolSpaceName(int space) {
        switch (space) {
            case ELEMENT:
                return "element";
            case ATTRIBUTE:
                return "attribute";
            case TYPE:
                return "type";
            case GROUP:
                return "group";
            case ATTRIBUTE_GROUP:
                return "attributeGroup";
            case NOTATION:
                return "notation";
            case IDENTITY_CONSTRAINT:
                return "identityConstraint";
        }
        throw new IllegalArgumentException();
    }
}

// Copyright (c) 2004-2022 Saxonica Limited



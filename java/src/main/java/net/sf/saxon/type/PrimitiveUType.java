////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.type;


import net.sf.saxon.ma.jnode.AnyJNodeType;
import net.sf.saxon.transpile.CSharpModifiers;
import net.sf.saxon.type.gnode.NodeKindType;

public enum PrimitiveUType {
    
    DOCUMENT (0),
    ELEMENT (1),
    ATTRIBUTE (2),
    TEXT (3),
    COMMENT (4),
    PI (5),
    NAMESPACE (6),

    JNODE (7),

    FUNCTION (8),

    STRING (9),
    BOOLEAN (10),
    DECIMAL (11),
    FLOAT (12),
    DOUBLE (13),
    DURATION (14),
    DATE_TIME (15),
    TIME (16),
    DATE (17),
    G_YEAR_MONTH (18),
    G_YEAR (19),
    G_MONTH_DAY (20),
    G_DAY (21),
    G_MONTH (22),
    HEX_BINARY (23),
    BASE64_BINARY (24),
    ANY_URI (25),
    QNAME (26),
    NOTATION (27),

    UNTYPED_ATOMIC (28),

    EXTENSION (30);


    private final int bit;

    PrimitiveUType(int bit) {
        this.bit = bit;
    }

    public int getBit() {
        return bit;
    }

    public UType toUType() {
        return new UType(1<<bit);
    }

    public static PrimitiveUType forBit(int bit) {
        return values()[bit];
    }

    @CSharpModifiers(code={"public", "override"})
    public String toString() {
        switch (this) {
            case DOCUMENT:
                return "document";
            case ELEMENT:
                return "element";
            case ATTRIBUTE:
                return "attribute";
            case TEXT:
                return "text";
            case COMMENT:
                return "comment";
            case PI:
                return "processing-instruction";
            case NAMESPACE:
                return "namespace";

            case JNODE:
                return "JNode";

            case FUNCTION:
                return "function";

            case STRING:
                return "string";
            case BOOLEAN:
                return "boolean";
            case DECIMAL:
                return "decimal";
            case FLOAT:
                return "float";
            case DOUBLE:
                return "double";
            case DURATION:
                return "duration";
            case DATE_TIME:
                return "dateTime";
            case TIME:
                return "time";
            case DATE:
                return "date";
            case G_YEAR_MONTH:
                return "gYearMonth";
            case G_YEAR:
                return "gYear";
            case G_MONTH_DAY:
                return "gMonthDay";
            case G_DAY:
                return "gDay";
            case G_MONTH:
                return "gMoonth";
            case HEX_BINARY:
                return "hexBinary";
            case BASE64_BINARY:
                return "base64Binary";
            case ANY_URI:
                return "anyURI";
            case QNAME:
                return "QName";
            case NOTATION:
                return "NOTATION";
            case UNTYPED_ATOMIC:
                return "untypedAtomic";

            case EXTENSION:
                return "external object";
            default:
                return "???";

        }
    }

    public ItemType toItemType() {
        switch (this) {
            case DOCUMENT:
                return NodeKindType.DOCUMENT;
            case ELEMENT:
                return NodeKindType.ELEMENT;
            case ATTRIBUTE:
                return NodeKindType.ATTRIBUTE;
            case TEXT:
                return NodeKindType.TEXT;
            case COMMENT:
                return NodeKindType.COMMENT;
            case PI:
                return NodeKindType.PROCESSING_INSTRUCTION;
            case NAMESPACE:
                return NodeKindType.NAMESPACE;

            case JNODE:
                return AnyJNodeType.getInstance();

            case FUNCTION:
                return AnyFunctionType.getInstance();

            case STRING:
                return BuiltInAtomicType.STRING;
            case BOOLEAN:
                return BuiltInAtomicType.BOOLEAN;
            case DECIMAL:
                return BuiltInAtomicType.DECIMAL;
            case FLOAT:
                return BuiltInAtomicType.FLOAT;
            case DOUBLE:
                return BuiltInAtomicType.DOUBLE;
            case DURATION:
                return BuiltInAtomicType.DURATION;
            case DATE_TIME:
                return BuiltInAtomicType.DATE_TIME;
            case TIME:
                return BuiltInAtomicType.TIME;
            case DATE:
                return BuiltInAtomicType.DATE;
            case G_YEAR_MONTH:
                return BuiltInAtomicType.G_YEAR_MONTH;
            case G_YEAR:
                return BuiltInAtomicType.G_YEAR;
            case G_MONTH_DAY:
                return BuiltInAtomicType.G_MONTH_DAY;
            case G_DAY:
                return BuiltInAtomicType.G_DAY;
            case G_MONTH:
                return BuiltInAtomicType.G_MONTH;
            case HEX_BINARY:
                return BuiltInAtomicType.HEX_BINARY;
            case BASE64_BINARY:
                return BuiltInAtomicType.BASE64_BINARY;
            case ANY_URI:
                return BuiltInAtomicType.ANY_URI;
            case QNAME:
                return BuiltInAtomicType.QNAME;
            case NOTATION:
                return BuiltInAtomicType.NOTATION;
            case UNTYPED_ATOMIC:
                return BuiltInAtomicType.UNTYPED_ATOMIC;

            case EXTENSION:
                //return JavaExternalObjectType.EXTERNAL_OBJECT_TYPE;
                return AnyItemType.getInstance();
            default:
                throw new IllegalArgumentException();

        }
    }
}


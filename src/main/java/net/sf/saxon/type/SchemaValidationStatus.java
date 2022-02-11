package net.sf.saxon.type;

import net.sf.saxon.transpile.CSharpSimpleEnum;

@CSharpSimpleEnum
public enum SchemaValidationStatus {
    UNVALIDATED,
    FIXED_UP,
    VALIDATING,
    VALIDATED,
    INVALID,
    INCOMPLETE
}


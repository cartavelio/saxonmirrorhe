package com.saxonica.ee.stream;

import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * The posture of an expression defines the relationship of the result to a streamed input document,
 * as defined in the XSLT 3.0 specification.
 */
@CSharpSimpleEnum
public enum Sweep {

    MOTIONLESS,
    CONSUMING,
    FREE_RANGING;

}

// Copyright (c) 2018-2022 Saxonica Limited



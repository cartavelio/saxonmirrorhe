package net.sf.saxon.type;

import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * The different varieties of complex types
 */

@CSharpSimpleEnum
public enum ComplexVariety {

    EMPTY, SIMPLE, ELEMENT_ONLY, MIXED;
}



package net.sf.saxon.type;

import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * The different varieties of open content for complex types
 */

@CSharpSimpleEnum
public enum OpenContentVariety {

    ABSENT,
    NONE,
    INTERLEAVE,
    SUFFIX
}



package net.sf.saxon.expr.parser;

import net.sf.saxon.transpile.CSharpSimpleEnum;

/**
 * Indicates whether something is required, prohibited, or optional. Used for
 * an attributeUse in a schema, also for the existence of a context item
 * in the static context.
 */

@CSharpSimpleEnum
public enum Optionality {REQUIRED, PROHIBITED, OPTIONAL}


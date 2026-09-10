package net.sf.saxon.expr.parser;

/**
 * Indicates whether something is required, prohibited, or optional. Used for
 * an attributeUse in a schema, also for the existence of a context item
 * in the static context.
 */

public enum Optionality {REQUIRED, PROHIBITED, OPTIONAL}


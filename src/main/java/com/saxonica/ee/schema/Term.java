package com.saxonica.ee.schema;

/**
 * Represents a Term in a content model as defined by XML Schema. The
 * term may be an element declaration, a wildcard, a sequence, a choice, or all.
 * Terms are not schema components in their own right. Essentially a Particle
 * is a Term together with a maxOccurs and minOccurs. Therefore, terms do not
 * have their own properties, which is why Term here is just a marker interface.
 */

public interface Term {

}

// Copyright (c) 2004-2023 Saxonica Limited
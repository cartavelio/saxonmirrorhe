package com.saxonica.ee.schema;

import net.sf.saxon.type.SimpleType;

/**
 * Corresponds to a Content Type Property Record as defined in the XML Schema 1.1 specification
 */
public class ContentType {

    public int variety;

    public Particle particle;

    public Wildcard openContentWildcard;

    public String openContentMode;

    public SimpleType simpleTypeDefinition;
}

// Copyright (c) 2008-2022 Saxonica Limited




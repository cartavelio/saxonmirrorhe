package com.saxonica.ee.schema;

import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.MissingComponentException;

/**
 * A component that can be serialized by the SchemaSerializer.
 * Most such components are instances of UserSchemaComponent,
 * but a few, for example Facets, are not.
 */

public interface SerializableSchemaComponent {

    /**
     * Serialize the schema component
     *
     * @param serializer the object responsible for performing the serialization
     * @throws MissingComponentException if the component cannot be serialized because it contains dangling references
     * @throws XPathException if serialization fails
     */

    void serialize(SchemaModelSerializer serializer) throws MissingComponentException, XPathException;

}

// Copyright (c) 2006-2023 Saxonica Limited No warranty.

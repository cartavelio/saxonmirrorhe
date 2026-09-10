package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.type.SchemaException;

/**
 * An unknown element in an XML Schema document. This may be an element with an unknown
 * name in the schema namespace, or an element in a user namespace. In both cases the
 * element is valid within xs:appInfo and xs:documentation, but not elsewhere. We don't
 * validate the contents of the element.
 */

public class UnknownElement extends SchemaElement {

    /**
     * Creates a new unknown element
     */
    public UnknownElement() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

    }

    /**
     * Recursive walk through the stylesheet to validate all nodes: this implementation
     * suppresses the subtree validation.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void validateSubtree(SchemaCompiler compiler) {
        // Suppress validation of the subtree for these elements. This has the side-effect of permitting
        // character data where all other elements prohibit it.

        // Technically we should check that if the element contains elements such as xs:element, then
        // these are valid according to the schema for schemas.
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     */

    @Override
    public void validate(SchemaCompiler compiler) {
        // This method is not called, because validateSubtree() suppresses it
    }

}

// Copyright (c) 2011-2023 Saxonica Limited

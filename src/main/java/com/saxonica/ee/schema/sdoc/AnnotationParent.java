package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.type.SchemaException;

/**
 * An abstract superclass for schema elements whose only permitted content is
 * an Annotation element
 */

public abstract class AnnotationParent extends SchemaElement {

    public AnnotationParent() {
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        checkOnlyChildIsAnnotation();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited


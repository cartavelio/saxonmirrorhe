package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.MissingComponentException;

/**
 * A SequenceCompositor represents an xs:sequence element in the schema
 */

public class SequenceModelGroup extends ModelGroup {

    /**
     * Determine if empty content is allowed
     */

    @Override
    public boolean isEmptiable() throws MissingComponentException {
        for (Particle aContentModel : contentModel) {
            if (!aContentModel.isEmptiable()) {
                return false;
            }
        }
        return true;
    }

    /**
     * Test whether the compositor is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     */

    @Override
    public boolean isPointless(ModelGroup container) {
        return contentModel.size() == 0 || contentModel.size() == 1 || container instanceof SequenceModelGroup;
    }

    /**
     * Checks the validity of this Compositor definition.
     *
     * @throws SchemaException when the error listener decides an error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (containsAll(false)) {
            compiler.error("An <xs:sequence> model group must not contain an <xs:all> model group", this);
            return false;
        }
        return super.validate(compiler);
    }

    /**
     * Get the name of this compositor
     */

    @Override
    public String getCompositorName() {
        return "sequence";
    }
}

// Copyright (c) 2004-2022 Saxonica Limited


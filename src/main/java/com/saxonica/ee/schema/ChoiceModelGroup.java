package com.saxonica.ee.schema;

import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.MissingComponentException;

/**
 * A ChoiceCompositor represents an xs:choice element in the schema
 */

public class ChoiceModelGroup extends ModelGroup {

    /**
     * Determine if empty content is allowed
     *
     * @return true if empty content matches this compositor, which is true if the compositor
     *         as a whole has minOccurs=0, or if any of the choices allows empty content.
     */

    @Override
    public boolean isEmptiable() throws MissingComponentException {
        for (Particle aContentModel : contentModel) {
            if (aContentModel.isEmptiable()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Test whether the compositor is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     */

    @Override
    public boolean isPointless(ModelGroup container) {
        return contentModel.isEmpty() ||
                contentModel.size() == 1 ||
                container instanceof ChoiceModelGroup;
    }

    /**
     * Mark contained particles as vulnerable. A particle is marked as vulnerable if everything both before
     * and after it in the content model is optional. This contributes to the algorithm for deciding whether
     * counting states can be used for this particle in the finite state machine
     *
     * @param isRepeatable set to true if this part of the content model can be repeated
     */

    @Override
    public void markVulnerableSubParticles(boolean isRepeatable) throws MissingComponentException {
        // in a repeated choice, all particles are vulnerable
        if (isRepeatable) {
            for (Particle p : contentModel) {
                p.setVulnerable(true);
            }
        }
        for (Particle p : contentModel) {
            if (p instanceof ModelGroupParticle) {
                p.markVulnerableSubParticles();
            }
        }
    }


    /**
     * Checks the validity of this Compositor definition.
     *
     * @throws SchemaException when the error listener decides an error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (containsAll(false)) {
            compiler.error("An <xs:choice> model group must not contain an <xs:all> model group", this);
            return false;
        }
        return super.validate(compiler);
    }

    /**
     * Get the name of this compositor
     */

    @Override
    public String getCompositorName() {
        return "choice";
    }

}

// Copyright (c) 2004-2023 Saxonica Limited


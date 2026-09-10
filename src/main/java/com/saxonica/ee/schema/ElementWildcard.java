package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntHashSet;

import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;

/**
 * This class represents an <code>&lt;any&gt;</code> element in the schema.
 */
public class ElementWildcard extends Particle {

    /**
     * The wildcard details are held in a separate object, designed to allow
     * reuse between ElementWildcard and AttributeWildcard
     */

    private Wildcard wildcard;
    private final PreparedSchema schema;

    public ElementWildcard(PreparedSchema schema) {
        this.schema = schema;
        wildcard = new Wildcard(schema);
        wildcard.setConfiguration(schema.getConfiguration());
        setConfiguration(schema.getConfiguration());
    }

    public ElementWildcard copy() {
        ElementWildcard e = new ElementWildcard(schema);
        e.wildcard = wildcard;
        e.setMinOccurs(getMinOccurs());
        e.setMaxOccurs(getMaxOccurs());
        return e;
    }

    public ElementWildcard(Wildcard wc) {
        wildcard = wc;
        schema = wc.getSchema();
    }

    public void setWildcard(Wildcard wildcard) {
        this.wildcard = wildcard;
    }

    public Wildcard getWildcard() {
        return wildcard;
    }


    /**
     * Determine if this particle can be empty
     */

    @Override
    public boolean isEmptiable() {
        return (getMinOccurs() == 0);
    }

    /**
     * Test whether the particle is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     */

    @Override
    public boolean isPointless(ModelGroup container) {
        return false;
    }

    /**
     * Get the term represented by this particle
     */

    @Override
    public Term getTerm() {
        return wildcard;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler the schema compiler
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) {
        setFixupStatus(VALIDATED);
        return true;
    }

    @Override
    public void compile(SchemaCompiler compiler) {
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by elementName), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the elementName matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param elementName Identifies the name of the child element within this content model
     */

    /*@Nullable*/
    @Override
    public SchemaType getElementParticleType(int elementName) {
        EnterpriseConfiguration config = getConfiguration();
        if (wildcard.matches(elementName, true, config, null)) {
            if (wildcard.getProcessContents().equals("skip")) {
                return AnyType.getInstance();
            } else if (wildcard.getProcessContents().equals("strict")) {
                ElementDecl decl = getConfiguration().getUnnamedSchema().getElementDecl(elementName);
                if (decl == null) {
                    return null;
                } else {
                    try {
                        return decl.getType();
                    } catch (MissingComponentException e) {
                        throw new IllegalStateException(e.getMessage());
                    }
                }
            } else { // processContents = "lax"
                ElementDecl decl = getConfiguration().getUnnamedSchema().getElementDecl(elementName);
                if (decl == null) {
                    return AnyType.getInstance();
                } else {
                    try {
                        return decl.getType();
                    } catch (MissingComponentException e) {
                        throw new IllegalStateException(e.getMessage());
                    }
                }
            }
        }
        return null;
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link StaticProperty#EXACTLY_ONE}, {@link StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link StaticProperty#ALLOWS_ZERO}.
     *
     * @param elementName Identifies the name of the child element within this content model
     */

    @Override
    public int getElementParticleCardinality(int elementName) {
        EnterpriseConfiguration config = getConfiguration();
        if (wildcard.matches(elementName, true, config, null)) {
            return Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
        }
        return StaticProperty.ALLOWS_ZERO;
    }

    /**
     * Ask whether the particle contains an element wildcard
     *
     * @return true if the particle is an element wildcard, or contains one
     */

    @Override
    public boolean containsElementWildcard() {
        return true;
    }

    /**
     * Find the set of all element particles allowed within the content model of this particle, identified
     * by their integer fingerprints.
     * @param result          an initially-empty integer set in which the results are accumulated
     * @param ignoreWildcards true if wildcards are to be ignored. If false, the value XS_INVALID_NAME is added
     */

    @Override
    public void gatherAllPermittedElements(IntHashSet result, boolean ignoreWildcards) throws SchemaException {
        if (!ignoreWildcards) {
            result.add(-1);
        }
    }

    /**
     * Check the validity (consistency) of this element wildcard definition.
     *
     * @param compiler the schema compiler
     * @return true if validation succeeded, false if errors were found.
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is found and the
     *          error listener decides that the error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        return wildcard.validate(compiler);
    }

    /**
     * Serialize the particle
     */

    @Override
    public void serializeParticle(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(wildcard, false);
        serializer.startElement("elementWildcard");
        serializer.emitAttribute("minOccurs", "" + getMinOccurs());
        serializer.emitAttribute("maxOccurs", getMaxOccurs() < 0 ? "unbounded" : "" + getMaxOccurs());
        serializer.emitAttribute("ref", id);
        serializer.endElement();
    }

    public FunctionItem getTermComponentAsFunction() {
        return wildcard.getComponentAsFunction();
    }
}

// Copyright (c) 2004-2023 Saxonica Limited

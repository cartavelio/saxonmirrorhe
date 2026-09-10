package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntHashSet;

import javax.xml.transform.SourceLocator;
import java.util.Collection;
import java.util.Stack;

import static net.sf.saxon.type.SchemaValidationStatus.INCOMPLETE;
import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;


/**
 * A reference to an element declaration within the content model of a complex type.
 * This corresponds to an &lt;xs:element ref="x"&gt; element in the XML representation
 * of a schema. Where a local element declaration appears in a content model, using
 * the form &lt;xs:element name="x"&gt;, both an element declaration and an element
 * particle are created.
 */

public class ElementParticle extends Particle {

    /**
     * Creates a new element particle
     *
     * @param config  the Configuration to which this element declaration
     *                belongs
     * @param locator identifies the location of the element declaration in the
     *                source schema document
     */

    public ElementParticle(EnterpriseConfiguration config, SourceLocator locator) {
        setMinOccurs(1);
        setMaxOccurs(1);
        setLocator(locator);
        setConfiguration(config);
        symbolSpace = SymbolSpace.ELEMENT;
    }

    /**
     * Copy an element particle
     *
     * @return a copy of this particle
     */

    /*@NotNull*/
    public ElementParticle copy() {
        ElementParticle p = new ElementParticle(getConfiguration(), null);
        p.setTargetComponentName(getTargetComponentName());
        p.setReference((ElementDecl)target);
        p.setMinOccurs(getMinOccurs());
        p.setMaxOccurs(getMaxOccurs());
        return p;
    }

    /**
     * Returns the display name of this Element particle. This will be in the form
     * of a lexical QName typically using the original prefix
     *
     * @return the local name of this element declaration
     */

    public String getDisplayName() {
        return getTargetComponentName().getDisplayName();
    }

    /**
     * Get the namespace URI of this element particle
     *
     * @return the target namespace of the element. Returns "" for the no-namespace case.
     */

    /*@NotNull*/
    public NamespaceUri getNamespaceURI() {
        return getTargetComponentName().getNamespaceUri();
    }

    /**
     * Returns the local name of this Element particle. The name
     * returned, if not null, will be an NCName.
     *
     * @return the name of this element declaration
     */

    public String getName() {
        return getTargetComponentName().getLocalPart();
    }

    /**
     * Returns the ElementDecl that this element definition references.
     *
     * @return the ElementDecl that this element definition references.
     * @throws MissingComponentException
     *          if the reference from the element
     *          particle to the corresponding element declaration has not been resolved
     */

    public ElementDecl getDeclaration() throws MissingComponentException {
        return (ElementDecl) getTarget();
    }


    /**
     * Set the reference to a specific (typically non-global) element declaration
     *
     * @param declaration the element declaration to which this particle refers.
     */

    public void setReference(ElementDecl declaration) {
        setTargetComponentName(declaration.getComponentName());
        setTarget(declaration);
    }

    /**
     * Determine if this particle can be empty
     *
     * @return true if this element, considered as a particle in a content model, can be matched by
     *         empty content (which is true if the minOccurs value is zero). The result is undefined for a global
     *         element declaration.
     */

    @Override
    public boolean isEmptiable() {
        return getMinOccurs() == 0;
    }

    /**
     * Test whether the particle is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     */

    @Override
    public boolean isPointless(ModelGroup container) {
        return false;
    }

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException, MissingComponentException {
        tryToResolve(compiler, false);
    }


    /**
     * Check references from this component to other components
     *
     * @param compiler is used for reporting errors
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        boolean result = true;
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            ElementDecl decl = (ElementDecl) PreparedSchema.validateReference(this, compiler, false);
            if (decl == null) {
                setFixupStatus(INVALID);
                return false;
            }
            result = decl.fixup(compiler);
        }
        setFixupStatus(VALIDATED);
        return result;
    }


    /**
     * Check the validity (consistency) of this element definition.
     *
     * @param compiler the schema compiler
     * @return true if validation succeeded, false if errors were found.
     * @throws net.sf.saxon.type.SchemaException
     *          if an error is found and the
     *          error listener decides that the error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        switch (getValidationStatus()) {
            case VALIDATED:
                //case FIXED_UP:
                return true;
            case INVALID:
            case INCOMPLETE:
                return false;
            case VALIDATING:
                return true;
            // recursive element definitions are allowed
            default:
                break;
        }

        //-- If this element merely references another element definition
        //-- just check that we can resolve the reference
        if (!isResolved()) {
            tryToResolve(compiler, false);
            if (!isResolved()) {
                // already reported during fixup()
//                String err = "Element " + Err.wrap(getDisplayName()) + " is referenced, but has not been declared";
//                compiler.warning(err, this);
                setValidationStatus(INCOMPLETE);
                return false;
            }
        }
        setValidationStatus(VALIDATING);
        boolean result = getDeclaration().validate(compiler);
        setValidationStatus(result ? VALIDATED : INVALID);
        return result;
    }

    /**
     * Compile this local element declaration, considered as a particle of some complex type,
     * by compiling its own complex type definition.
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if compilation fails.
     */

    @Override
    public void compile(SchemaCompiler compiler) throws SchemaException {
        if (getValidationStatus() != VALIDATED) {
            return;     // errors already reported
        }
        SchemaType type = getDeclaration().getType();
        if (type instanceof UserComplexType) {
            UserComplexType complexType = (UserComplexType) type;
            complexType.compile(compiler);
        }
    }

    /**
     * Get the term corresponding to this particle
     *
     * @return the corresponding term. For an Element particle, the corresponding term is the element declaration.
     */

    @Override
    public Term getTerm() throws MissingComponentException {
        return getDeclaration();
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by elementName), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the elementName matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param elementFingerprint Identifies the name of the child element within this content model
     */

    @Override
    public SchemaType getElementParticleType(int elementFingerprint, Schema schema) {
        try {
            ElementDecl decl = (ElementDecl) getTarget();
            if (elementFingerprint == getTargetFingerprint()) {
                return decl.getType();
            } else {
                Collection<ElementDecl> subsGroup = ((PreparedSchema)schema).getSubstitutionGroupMembers(decl);
                for (ElementDecl mem : subsGroup) {
                    if (mem.getFingerprint() == elementFingerprint) {
                        return mem.getType();
                    }
                }
            }
        } catch (MissingComponentException e) {
            throw new IllegalStateException(e.getMessage());
        }
        return null;
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link StaticProperty#EXACTLY_ONE}, {@link StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link StaticProperty#EMPTY}.
     *
     * @param elementName Identifies the name of the child element within this content model
     */

    @Override
    public int getElementParticleCardinality(int elementName, Schema schema) throws MissingComponentException {
        if (elementName == getTargetFingerprint()) {
            return Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
        }
        ElementDecl decl = (ElementDecl) getTarget();
        Collection<ElementDecl> subsGroup = ((PreparedSchema)schema).getSubstitutionGroupMembers(decl);
        for (ElementDecl mem : subsGroup) {
            if (mem.getFingerprint() == elementName) {
                return Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
            }
        }
        return StaticProperty.EMPTY;
    }

    /**
     * Ask whether the particle contains an element wildcard
     *
     * @return true if the particle is an element wildcard, or contains one
     */

    @Override
    public boolean containsElementWildcard() {
        return false;
    }

    /**
     * Find the set of all element particles allowed within the content model of this particle, identified
     * by their integer fingerprints. Ignore wildcards.
     *
     * @param schema
     * @param result          an initially-empty integer set in which the results are accumulated
     * @param ignoreWildcards if true, wildcards are ignored. If false, the result will be
     *                        a set containing the value XS_INVALID, meaning that it is not possible to enumerate
     */

    @Override
    public void gatherAllPermittedElements(Schema schema, IntHashSet result, boolean ignoreWildcards) throws MissingComponentException {
        ElementDecl decl = (ElementDecl) getTarget();
        result.add(decl.getFingerprint());
        Collection<ElementDecl> subsGroup = ((PreparedSchema)schema).getSubstitutionGroupMembers(decl);
        for (ElementDecl mem : subsGroup) {
            result.add(mem.getFingerprint());
        }
    }


    public String toString() {
        return "ElementParticle " + getName();
    }

    /**
     * Serialize the particle
     */

    @Override
    public void serializeParticle(SchemaModelSerializer serializer) throws XPathException, MissingComponentException {
        serializer.startElement("elementParticle");
        serializer.emitAttribute("minOccurs", "" + getMinOccurs());
        serializer.emitAttribute("maxOccurs", getMaxOccurs() < 0 ? "unbounded" : "" + getMaxOccurs());
        serializer.emitAttribute("ref", serializer.getId(getTarget(), false));
        serializer.endElement();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
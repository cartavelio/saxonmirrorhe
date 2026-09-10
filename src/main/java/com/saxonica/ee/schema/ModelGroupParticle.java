package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.z.IntHashSet;

import javax.xml.transform.SourceLocator;
import java.util.List;
import java.util.Stack;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;

/**
 * A reference within the content model of a complex type to a Model Group Definition.
 * Corresponds to a non-top-level xs:group element in a schema document, that is, an
 * xs:group element with a ref attribute.
 * <p>There is no direct equivalent of a ModelGroupParticle in the W3C schema component model,
 * because in that model the structure of a complex type is always fully expanded: that is,
 * any group references are replaced by the content of the group they refer to. This model
 * retains group references to allow late binding and redefinition. However, methods are
 * provided on the {@link ModelGroup} class to give a view of the content model in which the
 * group references disappear.</p>
 */

public class ModelGroupParticle extends Particle {

    /**
     * Creates a new Group reference
     *
     * @param config  the Configuration to which this Group belongs
     * @param locator the location of the corresponding declaration in the source schema document
     */
    public ModelGroupParticle(EnterpriseConfiguration config, SourceLocator locator) {
        setLocator(locator);
        setConfiguration(config);
    }

    /**
     * Determine whether this reference has been resolved
     */

    @Override
    public boolean isResolved() {
        return (target != null);
    }

    public ModelGroup getGroup() throws MissingComponentException {
        return (ModelGroup) getTarget();
    }

    /**
     * Get the symbol space of the target of this reference
     */

    @Override
    public int getSymbolSpace() {
        return SymbolSpace.GROUP;
    }

    /**
     * Determine if empty content is allowed
     */

    @Override
    public boolean isEmptiable() throws MissingComponentException {
        return getMinOccurs() == 0 || getGroup().isEmptiable();
    }

    /**
     * Fix up references to other elements in the schema.
     *
     * @param compiler the schema compiler
     * @return true if successful
     * @throws SchemaException if a fatal error occurs
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        SchemaComponent decl = PreparedSchema.validateReference(this, compiler, false);
        if (decl == null) {
            setFixupStatus(INVALID);
            return false;
        }
        lookForCycles(new Stack<SchemaComponent>(), compiler);
        setFixupStatus(VALIDATED);
        return true;
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     *
     * @param references A list of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   The schema compiler
     * @throws net.sf.saxon.type.SchemaException if cycles are found
     *
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException, MissingComponentException {
        if (references.contains(this)) {
            String err = "The definition of group " +
                    Err.wrap(getTargetComponentName().getDisplayName()) +
                    " is circular";
            // Empty the content model to prevent any further errors
            //contentModel = new ArrayList(20);
            throw new SchemaException(err);
        }
        tryToResolve(compiler, false);
        if (isResolved()) {
            getGroup().lookForCycles(references, compiler);
        }
    }

    /**
     * Returns the group that this group object is a reference to.
     * Note this method doesn't bind irrevocably to the target group declaration,
     * because this may change following a redefine.
     *
     * @return the schema component if the reference has been resolved.
     * @throws MissingComponentException if the reference has not been resolved. Note that
     *                                      this is an unchecked exception.
     */

    @Override
    public SchemaComponent getTarget() throws MissingComponentException {
        if (target == null) {
            throw new MissingComponentExceptionImpl(this);
        }
        return target;
    }

    /**
     * Attempt to resolved this reference using the groups defined in a given schema.
     * No error results if the reference cannot be resolved.
     */

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) throws SchemaException {
        if (target == null) {
            target = compiler.getPreparedSchema().getGroup(getTargetComponentName());
        }
        if (target == null) {
            target = compiler.getConfiguration().getUnnamedSchema().getGroup(getTargetComponentName());
        }
        if (target == null) {
            String name = getTargetComponentName().getEQName();
            if (compiler.isAllowDanglingReferences() && !fatal) {
                compiler.warning("Unknown model group " + name + " (substituting empty group)", SaxonErrorCode.SXSD1006, this);
                ModelGroupDefinition decl = new ModelGroupDefinition(getConfiguration(), this);
                decl.setModelGroupName(getTargetComponentName());
                target = decl;
            } else{
                throw new SchemaException("Unknown attribute declaration " + name, this);
            }
        }
    }

    /**
     * Test whether the particle is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     */

    @Override
    public boolean isPointless(ModelGroup container) throws MissingComponentException {
        return getMinOccurs() == 1 && getMaxOccurs() == 1 &&
                ((ModelGroup) getTarget()).isPointless(container);
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
    public SchemaType getElementParticleType(int elementName, PreparedSchema schema) throws MissingComponentException {
        return getGroup().getElementParticleType(elementName, schema);
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by fingerprint), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link net.sf.saxon.expr.StaticProperty#EXACTLY_ONE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link net.sf.saxon.expr.StaticProperty#ALLOWS_ZERO}.
     *
     * @param elementName Identifies the name of the child element within this content model
     */

    @Override
    public int getElementParticleCardinality(int elementName, PreparedSchema schema) throws MissingComponentException {
        int card = getGroup().getElementParticleCardinality(elementName, schema);
        return Cardinality.multiply(card,
                computeParticleCardinality(getMinOccurs(), getMaxOccurs()));
    }

    /**
     * Ask whether the particle contains an element wildcard
     *
     * @return true if the particle is an element wildcard, or contains one
     */

    @Override
    public boolean containsElementWildcard() throws MissingComponentException {
        return getGroup().containsElementWildcard();
    }

    /**
     * Find the set of all element particles allowed within the content model of this particle, identified
     * by their integer fingerprints. Ignore wildcards.
     * @param result          an initially-empty integer set in which the results are accumulated
     * @param ignoreWildcards true if wildcard particles are to be ignored. If false, the resulting set
     */

    @Override
    public void gatherAllPermittedElements(IntHashSet result, boolean ignoreWildcards) throws SchemaException {
        getGroup().gatherAllPermittedElements(result, ignoreWildcards);
    }

    /**
     * Mark the vulnerable subParticles of this particle
     */

    @Override
    public void markVulnerableSubParticles() throws MissingComponentException {
        getGroup().markVulnerableSubParticles(getMaxOccurs() > 1 || getMaxOccurs() == -1);
    }

    /**
     * Checks the validity of this Schema definition.
     *
     * @throws net.sf.saxon.type.SchemaException if it is found to be invalid
     *
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        switch (getValidationStatus()) {
            case VALIDATED:
                return true;
            case INVALID:
                return false;
            case VALIDATING:
                return false;
            // circularities already detected
            case UNVALIDATED:
                break;
        }


        ModelGroup decl = (ModelGroup) PreparedSchema.validateReference(this, compiler, false);
        if (isResolved()) {
            boolean result = decl.validate(compiler);
            if (result) {
                List<Particle> particles;
                Particle first;
                if ((getMaxOccurs() == -1 || getMaxOccurs() > 1) &&
                        !(particles = decl.getParticles()).isEmpty() &&
                        (first = particles.get(0)) instanceof ModelGroupParticle &&
                        first.getTerm() instanceof AllModelGroup) {
                    compiler.error("A group defined using <xs:all> cannot occur more than once", this);
                    result = false;
                }
            }

            if (getMaxOccurs() > 1 || getMaxOccurs() == -1) {
                decl.markVulnerableSubParticles(true);
            }
            return result;
        } else {
            setValidationStatus(INVALID);
            return false;
        }

    }

    /**
     * Compile a particle
     *
     * @param compiler the schema compmiler
     */

    @Override
    public void compile(SchemaCompiler compiler) throws SchemaException {
        ((ModelGroup) getTarget()).compile(compiler);
    }

    /**
     * Get the term corresponding to this particle
     *
     * @return the corresponding term
     */

    @Override
    public Term getTerm() throws MissingComponentException {
        return getGroup();
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     * @throws SchemaException if the schema is found to be invalid
     */

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        getGroup().elaborate(compiler);
    }

    /**
     * Serialize the schema component as a particle
     */

    @Override
    public void serializeParticle(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("modelGroupParticle");
        serializer.emitAttribute("minOccurs", "" + getMinOccurs());
        serializer.emitAttribute("maxOccurs", getMaxOccurs() < 0 ? "unbounded" : "" + getMaxOccurs());
        if (getGroup() instanceof ModelGroupDefinition) {
            serializer.emitAttribute("ref", serializer.getId(getGroup(), false));
        } else {
            getGroup().serialize(serializer);
        }
        serializer.endElement();
    }
}

// Copyright (c) 2004-2023 Saxonica Limited
package com.saxonica.ee.schema;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntHashSet;

import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INCOMPLETE;
import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * An XML Schema Model Group. This corresponds to a sequence, choice, or all in the
 * schema document representation. The model group is typically referenced by a
 * ModelGroupParticle, which links it into some containing content model. Named model
 * groups, corresponding to global xs:group elements in a schema document, are represented
 * by the subclass {@link ModelGroupDefinition}.
 */

public abstract class ModelGroup extends SchemaStructure implements UserSchemaComponent, Term {

    /**
     * The list of particles making up the content model of this ModelGroup. In this list,
     * group references are not expanded: that is, the particles can be real particles, or
     * group references. In an AllModelGroup, this list does not include wildcard particles
     */

    protected List<Particle> contentModel;

    /**
     * The reduced list of particles. Pointless particles are removed, and group references
     * are expanded. The list is evaluated lazily.
     */

    protected List<Particle> simplifiedContentModel;

    /**
     * Creates a new ModelGroup, with no name
     */

    public ModelGroup() {
        contentModel = new ArrayList<>(20);
    }

    /**
     * Get the name of the compositor used in this model group
     *
     * @return one of "sequence", "choice", or "all"
     */

    public abstract String getCompositorName();

    /**
     * Adds a given Particle to the content model defined by this model group
     *
     * @param particle the Particle to be added. Note that the particle is ignored
     *                 (and is not added to the content model) if it has minOccurs=maxOccurs=0.
     */

    public void addParticle(Particle particle) {
        // If minOccurs=maxOccurs=0, the declaration contributes nothing to the content model.
        // See MS test mgH014 where this occurs in a choice, and does not make the choice emptiable
        // The relevant rule appears separately in the spec for each kind of particle:
        // "...(unless minOccurs=maxOccurs=0, in which case the item corresponds to no component at all)."
        if (particle != null && (particle.getMinOccurs() != 0 || particle.getMaxOccurs() != 0)) {
            contentModel.add(particle);
        }
    }

    /**
     * Set the entire content model
     *
     * @param particles a List whose members are Particle objects
     */

    public void setContentModel(List<Particle> particles) {
        contentModel = particles;
    }

    /**
     * Determine if empty content is allowed
     *
     * @return true if the content model of the model group allows empty content
     * @throws MissingComponentException
     *          if the schema contains
     *          unresolved references to required components
     */

    public abstract boolean isEmptiable() throws MissingComponentException;

    /**
     * Test whether the particle is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     *
     * @param container not used
     * @return true if the particle is pointless (which effectively means it can't match anything)
     */

    public boolean isPointless(ModelGroup container) throws MissingComponentException {
        return getSimplifiedContentModel().isEmpty();
        //return false;
    }

    /**
     * Get the (unsimplified list of) Particles of the content model defined by this model group.
     *
     * @return the immediately contained particles in the content model.
     *         Note that the iteration may return group references as well as "true" particles.
     */

    public List<Particle> getParticles() {
        return contentModel;
    }

    /**
     * Get the number of particles in the (unsimplified) content model
     *
     * @return the number of particles in the (unsimplified) content model
     */

    public int getContentModelSize() {
        return contentModel.size();
    }

    /**
     * Return true if the content model is empty
     *
     * @return true if the content model is empty
     */

    public boolean isEmpty() {
        return getParticles().isEmpty();
    }

    /**
     * Test whether this model group contains an xs:all compositor
     *
     * @param simplified set to false if we want to look at the original unsimplified
     *                   content model, to true if we want the model after removing pointless particles
     * @return true if this model group contains an xs:all compositor
     * @throws MissingComponentException
     *          if the schema contains unresolved
     *          references to required components
     */

    public boolean containsAll(boolean simplified) throws MissingComponentException {
        List<Particle> particles = simplified ? getSimplifiedContentModel() : contentModel;
        for (Particle p : particles) {
            if (p instanceof ModelGroupParticle && ((ModelGroupParticle) p).getGroup().containsAll(simplified)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Remove pointless particles from this content model (recursively),
     * and replace group references by the content of the identified group
     *
     * @return the simplified content model
     */

    public List<Particle> getSimplifiedContentModel() throws MissingComponentException {
        if (simplifiedContentModel == null) {
            simplifiedContentModel = new ArrayList<>(contentModel.size());
            for (Particle p : contentModel) {
                if (p instanceof ModelGroupParticle) {
                    ModelGroup mgd = ((ModelGroupParticle) p).getGroup();
                    if (p.isPointless(this)) {
                        simplifiedContentModel.addAll(mgd.getSimplifiedContentModel());
                    } else if (this instanceof AllModelGroup && mgd instanceof AllModelGroup) {
                        simplifiedContentModel.addAll(mgd.getSimplifiedContentModel());
                    } else {
                        simplifiedContentModel.add(p);
                    }
                } else {
                    simplifiedContentModel.add(p);
                }
            }
        }
        return simplifiedContentModel;
    }

    /**
     * Mark contained particles as vulnerable. A particle is marked as vulnerable if everything both before
     * and after it in the content model is optional. This is used in deciding whether to use counting states
     * in a finite state machine.
     *
     * @param isRepeatable true if this particle repeats within the content model
     */

    public void markVulnerableSubParticles(boolean isRepeatable) throws MissingComponentException {
        if (isRepeatable) {
            int numberOfOptionalSubParticles = 0;
            for (Particle p : contentModel) {
                if (p.getMinOccurs() == 0 || p.isEmptiable()) {
                    numberOfOptionalSubParticles++;
                }
            }
            if (numberOfOptionalSubParticles == contentModel.size()) {
                // if all particles are optional, then all are vulnerable
                for (Particle p : contentModel) {
                    p.setVulnerable(true);
                }
            } else if (numberOfOptionalSubParticles == contentModel.size() - 1) {
                // if exactly one particle is non-optional, then that particle is vulnerable
                for (Particle p : contentModel) {
                    if (!(p.getMinOccurs() == 0 || p.isEmptiable())) {
                        p.setVulnerable(true);
                        break;
                    }
                }
            }
            // otherwise, at least two particles are non-optional, so no particles are vulnerable.
        }
        for (Particle p : contentModel) {
            if (p instanceof ModelGroupParticle) {
                p.markVulnerableSubParticles();
            }
        }
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     *
     * @param references A list of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   used for error reporting
     * @throws SchemaException if cycles are found
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException {
        references.push(this);
        for (Particle p : getParticles()) {
            p.lookForCycles(references, compiler);
        }
        references.pop();
    }

    /**
     * Checks the validity of this model group.
     *
     * @throws SchemaException when the error listener decides an error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        boolean result = true;
        for (Particle p : getParticles()) {
            if (p instanceof ModelGroupParticle) {
                ModelGroupParticle ref = (ModelGroupParticle) p;
                ref.tryToResolve(compiler, false);
                if (ref.isResolved()) {
                    result &= ref.getGroup().validate(compiler);
                } else {
                    String err = "Group " + ref.getTargetComponentName().getDisplayName() +
                            " is referenced, but has not been declared";
                    compiler.error(err, this);
                    setValidationStatus(INCOMPLETE);
                    return false;
                }
            } else {
                result &= p.validate(compiler);
            }
        }

        if (!result) {
            setValidationStatus(INVALID);
            return false;
        }

        // Check that if the same element appears more than once (at any depth), then
        // the types are the same

        // 3.8.6 Schema Component Constraint: Element Declarations Consistent

        IntHashMap<ElementDecl> elements = new IntHashMap<>(20);
        // gather all the elements appearing in this content model, checking them for consistency
        //noinspection ConstantConditions
        result &= checkElements(elements, compiler);
        if (!result) {
            setValidationStatus(INVALID);
            return false;
        }
        return true;
    }

    /**
     * Check that the elements defined as particles in this model group are consistent
     *
     * @param map      a HashMap that maps integers to ElementDecl's
     * @param compiler used for error reporting
     * @return true of the elements are consistent
     * @throws net.sf.saxon.type.SchemaException
     *          if the schema is invalid
     * @throws MissingComponentException
     *          if the schema contains unresolved references
     *          to required components
     */

    protected boolean checkElements(IntHashMap<ElementDecl> map, SchemaCompiler compiler)
            throws SchemaException, MissingComponentException {
        PreparedSchema schema = compiler.getPreparedSchema();
        Set<ElementWildcard> wildcards = new HashSet<>();
        for (Particle p : getParticles()) {
            if (p instanceof ModelGroupParticle) {
                // Expand the element map recursively to include the indirectly-contained particles
                boolean b = ((ModelGroupParticle) p).getGroup().checkElements(map, compiler);
                if (!b) {
                    return false;
                }
            } else if (p instanceof ElementWildcard) {
                wildcards.add((ElementWildcard) p);
            } else if (p instanceof ElementParticle) {
                if (!p.isResolved()) {
                    // unresolved reference:
                    String err = "Unresolved reference to element " +
                            Err.wrap(p.getTargetComponentName().getDisplayName(), Err.ELEMENT);
                    compiler.error(err, this);
                    return false;
                }
                ElementDecl e = ((ElementParticle) p).getDeclaration();
                int key = e.getFingerprint();
                ElementDecl prev = map.get(key);
                if (prev == null) {
                    map.put(key, e);
                } else {
                    if (!e.getType().equals(prev.getType())) {
                        String err = "Two elements in the content model have the same name " +
                                Err.wrap(e.getDisplayName(), Err.ELEMENT) +
                                " but different types. " +
                                "This violates the constraint 'Element Declarations Consistent'.";
                        compiler.error(err, this);
                        return false;
                    }
                    if (!e.hasSameTypeTable(prev)) {
                        String err = "Two elements in the content model have the same name " +
                                Err.wrap(e.getDisplayName(), Err.ELEMENT) +
                                " but different type alternatives. " +
                                "This violates the constraint 'Element Declarations Consistent'.";
                        compiler.error(err, this);
                        return false;
                    }
                }
            }
        }

        // If two elements in the content model have the same name, then under "Element Declarations Consistent"
        // they must also have the same type. We have already checked this for elements explicitly included in the
        // content model (directly or indirectly), this method now does the check for elements that are implicitly
        // included by virtue of substitution group membership. By definition, such element declarations must be global,
        // and the only possibility of a clash is if the element has the same name as a local element declaration
        // that is an explicit member of the content model. So for each explicit element declaration in the content
        // model that is a global declaration, and for each member of its substitution group, we check to see whether
        // it has the same name as any other member of the model group, and if so, we compare the types

        Iterator<ElementDecl> iterM = map.valueIterator();
        while (iterM.hasNext()) {
            ElementDecl e = iterM.next();
            if (e.isGlobal()) {
                for (ElementDecl member : schema.getSubstitutionGroupMembers(e)) {
                    ElementDecl local = map.get(member.getFingerprint());
                    if (local != null && local != member) {
                        if (!e.getType().isSameType(local.getType())) {
                            String err = "An element " + Err.wrap(member.getDisplayName(), Err.ELEMENT) +
                                    " appears in the content model both as a local element and also as " +
                                    "a member of the substitution group of element " +
                                    Err.wrap(e.getDisplayName(), Err.ELEMENT) +
                                    ", and the two element declarations have different types. " +
                                    "This violates the constraint 'Element Declarations Consistent'.";
                            compiler.error(err, this);
                            setValidationStatus(INVALID);
                            return false;
                        }
                        if (!e.hasSameTypeTable(local)) {
                            String err = "An element " + Err.wrap(member.getDisplayName(), Err.ELEMENT) +
                                    " appears in the content model both as a local element and also as " +
                                    "a member of the substitution group of element " +
                                    Err.wrap(e.getDisplayName(), Err.ELEMENT) +
                                    ", and the two element declarations have different type alternatives. " +
                                    "This violates the constraint 'Element Declarations Consistent'.";
                            compiler.error(err, this);
                            return false;
                        }
                    }
                }
            }
        }

        // If a strict or lax wildcard particle in the model group matches a global element declaration with the same
        // name as an element particle within the group, then under "Element Declarations Consistent" in XSD 1.1
        // they must also have the same type tables. We don't need to do the check for substitution group members in this
        // case, because they will always be global and therefore match the global element declaration automatically.

        if (wildcards.isEmpty() || compiler.getConfiguration().getXsdVersion() == Configuration.XSD10) {
            return true;
        }

        iterM = map.valueIterator();
        while (iterM.hasNext()) {
            ElementDecl local = iterM.next();
            if (!local.isGlobal()) {
                // SaxonCS:
                // noinspection RedundantCast
                ElementDecl global = (ElementDecl)compiler.getPreparedSchema().getElementDecl(local.getComponentName());
                if (global != null) {
                    // There's a local declaration with the same name as a global declaration. Now see if there's
                    // a wildcard that matches the same name
                    for (ElementWildcard wild : wildcards) {
                        Wildcard card = wild.getWildcard();
                        if (!card.getProcessContents().equals("skip") &&
                                card.matches(global.getComponentName(), true, compiler.getPreparedSchema(), null)) {
                            if (!local.hasSameTypeTable(global)) {
                                String err = "A local element " + Err.wrap(local.getDisplayName(), Err.ELEMENT) +
                                        " appears in the content model that has the same name as a global element " +
                                        "declaration that could match a strict or lax wildcard in the content model. " +
                                        "The table of type alternatives differs between the local element declaration " +
                                        "and the global element declaration, which violates the Element Declarations " +
                                        "Consistent constraint.";
                                compiler.error(err, this);
                                return false;
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by elementName), and return the schema type associated with that element particle.
     * If there is no such particle, return null. If the elementName matches an element wildcard,
     * return the type of the global element declaration with the given name if one exists, or AnyType
     * if none exists and lax validation is permitted by the wildcard.
     *
     * @param elementName Identifies the name of the child element within this content model
     * @return the type associated with the element particle, if found
     */

    /*@Nullable*/
    public SchemaType getElementParticleType(int elementName, Schema schema) throws MissingComponentException {
        for (Particle p : getParticles()) {
            SchemaType t = p.getElementParticleType(elementName, schema);
            if (t != null) {
                return t;
            }
        }
        return null;
    }

    /**
     * Find an element particle within this complex type definition having a given element name
     * (identified by elementName), and return the cardinality associated with that element particle,
     * that is, the number of times the element can occur within this complex type. The value is one of
     * {@link StaticProperty#EXACTLY_ONE}, {@link StaticProperty#ALLOWS_ZERO_OR_ONE},
     * {@link StaticProperty#ALLOWS_ZERO_OR_MORE}, {@link StaticProperty#ALLOWS_ONE_OR_MORE},
     * If there is no such particle, return {@link StaticProperty#EMPTY}.
     *
     * @param elementName Identifies the name of the child element within this content model
     * @return the cardinality of the element particle within this content model
     */

    public int getElementParticleCardinality(int elementName, Schema schema) throws MissingComponentException {
        int card = StaticProperty.EMPTY;
        for (Particle p : getParticles()) {
            int t = p.getElementParticleCardinality(elementName, schema);
            if (t != StaticProperty.EMPTY) {
                if (this instanceof SequenceModelGroup) {
                    card = Cardinality.sum(card, t);
                } else {
                    card = Cardinality.union(card, t);
                }
            }
        }
        return card;
    }

    /**
     * Ask whether the model group contains an element wildcard
     *
     * @return true if the model group contains an element wildcard,
     */

    public boolean containsElementWildcard() throws MissingComponentException {
        for (Particle p : getParticles()) {
            if (p.containsElementWildcard()) {
                return true;
            }
        }
        return false;
    }

    /**
     * Find the set of all element particles allowed within the content model of this particle, identified
     * by their integer fingerprints. Ignore wildcards.
     *
     * @param result          an initially-empty integer set in which the results are accumulated
     * @param ignoreWildcards if true, wildcards are ignored. If false, the result will be
     *                        an integer set containing the value XS_INVALID_NAME, meaning that it is not possible to enumerate
     *                        the elements that can appear in the content.
     * @throws net.sf.saxon.type.SchemaException
     *          if the schema is found to be invalid
     */

    public void gatherAllPermittedElements(Schema schema, IntHashSet result, boolean ignoreWildcards) throws SchemaException {
        for (Particle p : getParticles()) {
            p.gatherAllPermittedElements(schema, result, ignoreWildcards);
        }
    }


    /**
     * Compile this Compositor by compiling each of the particles contained in its
     * content model.
     *
     * @param compiler used for error reporting
     * @throws SchemaException if compilation fails.
     */

    public void compile(SchemaCompiler compiler) throws SchemaException {
        for (Particle p : getParticles()) {
            p.compile(compiler);
        }
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler is used for reporting errors
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException, MissingComponentException {
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            for (Particle p : contentModel) {
                p.fixup(compiler);
            }
            lookForCycles(new Stack<SchemaComponent>(), compiler);
        }
        setFixupStatus(VALIDATED);
        return true;
    }


    /**
     * Get the term corresponding to this particle
     *
     * @return the corresponding term
     */

    public Term getTerm() {
        return this;
    }

    /**
     * Serialize the schema component
     *
     * @param serializer used to receive the serialized output
     * @throws net.sf.saxon.trans.XPathException
     *          if serialization fails
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException, MissingComponentException {
        String compositor = getCompositorName();
        serializer.startElement(compositor);
        for (Particle p : getParticles()) {
            p.serializeParticle(serializer);
        }
        serializer.endElement();
    }

    @Override
    public FunctionItem getComponentAsFunction() {
        ModelGroup thisModelGroup = this;
        return accessorFunction(arg -> {
                String key = arg.head().getStringValue();
                switch (key) {
                    case "class":
                        return StringValue.bmp("Model Group");
                    case "implementation":
                        return new ObjectValue<>(thisModelGroup);
                    case "compositor":
                        return new StringValue(getCompositorName());
                    case "particles":
                        List<Item> particles = new ArrayList<>();
                        for (Particle p : getParticles()) {
                            particles.add(p.getComponentAsFunction());
                        }
                        return SequenceExtent.makeSequenceExtent(particles);
                    default:
                        return EmptySequence.getInstance();
                }

        });
    }


}

// Copyright (c) 2004-2023 Saxonica Limited
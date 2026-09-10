package com.saxonica.ee.schema;

import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntToIntArrayMap;
import net.sf.saxon.z.IntToIntHashMap;
import net.sf.saxon.z.IntToIntMap;

import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;


/**
 * An AllModelGroup represents an xs:all element in the schema
 */

public class AllModelGroup extends ModelGroup {

    private int[] minima;       // minOccurs values for the particles
    private int[] maxima;       // maxOccurs values for the particles
    private StructuredQName[] elementNames;     // names of the element particles (for diagnostics)
    private int numberOfElementParticles = 0;
    private IntToIntMap counterMap;
    // mapping from fingerprints of element particles to positions in minima/maxima
    private IntHashMap<ElementDecl> declarationMap;
    // mapping from fingerprint to element declaration

    private List<ElementWildcard> wildcards;     // list of element wildcard particles (XSDL 1.1 only)

    /**
     * Create an AllModelGroup
     */

    public AllModelGroup() {
    }


    /**
     * Adds a given Particle to the content model defined by this Compositor
     *
     * @param particle the Particle to be added
     */

    @Override
    public void addParticle(Particle particle) {
        super.addParticle(particle);
        if (particle instanceof ElementWildcard) {
            // allowed in XML Schema 1.1
            if (wildcards == null) {
                wildcards = new ArrayList<ElementWildcard>(4);
            }
            wildcards.add((ElementWildcard) particle);
        } else {
            numberOfElementParticles++;
        }
    }

    /**
     * Get the number of element particles
     *
     * @return the number of element particles in the content model
     */

    public int getNumberOfElementParticles() {
        return numberOfElementParticles;
    }

    /**
     * Determine if the content model uses xs:all.
     *
     * @return true (always)
     */

    @Override
    public boolean containsAll(boolean simplified) {
        return true;
    }


    /**
     * Determine if empty content is allowed
     *
     * @return true if this content model permits empty content
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
        // Can't treat an *all* group as pointless because it affects the rules for extension
        return false;
    }

    /**
     * Check that the elements defined as particles in this compositor are consistent
     * (overrides method on parent class ModelGroup)
     */

    @Override
    protected boolean checkElements(IntHashMap<ElementDecl> map, SchemaCompiler compiler)
            throws SchemaException, MissingComponentException {

        if (map.size() > 0) {
            // This check is probably redundant: other compositors already check that they don't contain xs:all
            compiler.error("<xs:all> cannot appear within the content model of another compositor", this);
            return false;
        }

        for (Particle p : getSimplifiedContentModel()) {
            if (p instanceof ElementParticle) {
                ElementParticle ep = (ElementParticle) p;
                int key = ((ElementDecl)ep.getTarget()).getFingerprint();
                ElementDecl prev = map.get(key);
                if (prev == null) {
                    map.put(key, ep.getDeclaration());
                } else {
                    String err = "Two elements with the same name " + Err.wrap(ep.getName(), Err.ELEMENT) +
                            " appear in the content model of <xs:all>. This violates the Unique Particle Attribution constraint.";
                    compiler.error(err, this);
                    return false;
                }
            }
        }

        // If two elements in the content model have the same name, this is always a UPA violation even if they
        // have the same type. So the check for "Element Declarations Consistent" is not needed in the case of an
        // xs:all group. As well as checking whether two elements have the same name, however, we must also check
        // that their substitution groups do not overlap.

        IntHashMap<ElementDecl> implicitMembers = new IntHashMap<ElementDecl>(20);
        Iterator<ElementDecl> iterM = map.valueIterator();
        while (iterM.hasNext()) {
            ElementDecl e = iterM.next();
            //implicitMembers.put(e.getFingerprint(), );
            if (e.isGlobal()) {
                for (ElementDecl member : e.getSubstitutionGroupMembers()) {
                    ElementDecl existing = implicitMembers.put(member.getFingerprint(), e);
                    if (existing != null) {
                        String err = "An element " + Err.wrap(member.getDisplayName(), Err.ELEMENT) +
                                " appears implicitly in the expansion of more than one particle of the xs:all group, " +
                                "because it is a member of the substitution groups of both element " +
                                Err.wrap(existing.getDisplayName(), Err.ELEMENT) +
                                " and element " + Err.wrap(e.getDisplayName(), Err.ELEMENT) +
                                ". This violates the Unique Particle Attribution constraint.";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                }
            }
        }

        // If the model group allows wildcards, check that no two wildcards overlap with each other

        if (wildcards != null) {
            for (int i = 0; i < wildcards.size(); i++) {
                for (int j = 0; j < i; j++) {
                    if (wildcards.get(j).getWildcard().overlaps(wildcards.get(i).getWildcard())) {
                        String err = "The <xs:all> content model contains two <xs:any> wildcards that overlap each other " +
                                "(there are elements that could match either wildcard). " +
                                "This violates the Unique Particle Attribution Constraint.";
                        compiler.error(err, this);
                        setValidationStatus(INVALID);
                        return false;
                    }
                }
            }
        }
        return true;
    }

    /**
     * Get the name of this compositor
     */

    @Override
    public String getCompositorName() {
        return "all";
    }

    /**
     * Remove pointless particles from this content model (recursively),
     * and replace group references by the content of the identified group
     *
     * @return the simplified content model
     */

    @Override
    public List<Particle> getSimplifiedContentModel() throws MissingComponentException {
        if (simplifiedContentModel == null) {
            List<Particle> s1 = super.getSimplifiedContentModel();
            // For an All group, do further simplification to ensure all named model group references are expanded
            List<Particle> s2 = new ArrayList<Particle>();
            for (Particle p : s1) {
                if (p instanceof ModelGroupParticle) {
                    ModelGroup mg = (ModelGroup) p.getTarget();
                    for (Particle q : mg.getSimplifiedContentModel()) {
                        s2.add(q);
                    }
                } else {
                    s2.add(p);
                }
            }
            simplifiedContentModel = s2;
        }
        return simplifiedContentModel;
    }

    /**
     * Checks the validity of this Compositor definition.
     *
     * @throws net.sf.saxon.type.SchemaException
     *          when the error listener decides an error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        if (!super.validate(compiler)) {
            return false;
        }
        // check that any referenced model group particles are OK
        for (Particle structure : getParticles()) {
            if (structure instanceof ModelGroupParticle) {
                ModelGroupParticle ref = (ModelGroupParticle) structure;
                if (ref.isResolved()) {
                    boolean ok = false;
                    if (ref.getMinOccurs() == 1 && ref.getMaxOccurs() == 1) {
                        ModelGroupDefinition def = (ModelGroupDefinition) ref.getGroup();
                        List<Particle> groupContents = def.getParticles();
                        if (groupContents.size() == 1 && groupContents.get(0) instanceof ModelGroupParticle) {
                            ModelGroupParticle mgp = (ModelGroupParticle) groupContents.get(0);
                            if (mgp.getMinOccurs() == 1 && mgp.getMaxOccurs() == 1 && mgp.getTarget() instanceof AllModelGroup) {
                                ok = true;
                            }
                        }
                    }
                    if (!ok) {
                        String err = "Group " + ref.getTargetComponentName().getDisplayName() +
                                " is referenced in an xs:all group, so it must itself contain an xs:all group with minOccurs = maxOccurs = 1";
                        compiler.error(err, ref);
                        setValidationStatus(INVALID);
                        return false;
                    }
                }
            } else {
                structure.validate(compiler);
            }
        }
        elaborate(compiler);
        return true;
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        List<Particle> particles = getSimplifiedContentModel();
        int len = particles.size();
        minima = new int[len];
        maxima = new int[len];
        elementNames = new StructuredQName[len];
        if (particles.size() < 8) {
            counterMap = new IntToIntArrayMap(particles.size() + 1);
        } else {
            counterMap = new IntToIntHashMap(particles.size() + 1);
        }
        declarationMap = new IntHashMap<ElementDecl>(particles.size() + 1);
        int wildcardIndex = 0;
        int elementIndex = wildcards == null ? 0 : wildcards.size();   // first n counter positions are reserved for wildcards
        for (Particle particle : particles) {
            if (particle instanceof ElementParticle) {
                minima[elementIndex] = particle.getMinOccurs();
                maxima[elementIndex] = particle.getMaxOccurs();
                ElementDecl decl = ((ElementParticle) particle).getDeclaration();
                int key = decl.getFingerprint();
                elementNames[elementIndex] = particle.getTargetComponentName();
                counterMap.put(key, elementIndex);
                declarationMap.put(key, decl);
                Collection<ElementDecl> group = decl.getSubstitutionGroupMembers();
                for (ElementDecl substituteDecl : group) {
                    int fp = substituteDecl.getFingerprint();
                    counterMap.put(fp, elementIndex);
                    declarationMap.put(fp, substituteDecl);
                }
                elementIndex++;
            } else {
                minima[wildcardIndex] = particle.getMinOccurs();
                maxima[wildcardIndex] = particle.getMaxOccurs();
                wildcardIndex++;
            }
        }
    }

    /**
     * Get the values of maxOccurs() for all the particles, as an array
     *
     * @return an array containing the maxOccurs() values. The value "unbounded" is
     *         represented as -1. The entry in the array for a given element name can be
     *         found using the counterMap.
     */

    public int[] getMaxima() {
        return maxima;
    }

    /**
     * Get the values of minOccurs() for all the particles, as an array
     *
     * @return an array containing the minOccurs() values. The entry in
     *         the array for a given element name can be found using the counterMap.
     */

    public int[] getMinima() {
        return minima;
    }

    /**
     * Get the namecodes of the element particles corresponding to each
     * item in the maxima/minima arrays.
     *
     * @return an array containing the nameCode values.
     */

    public StructuredQName[] getNameCodes() {
        return elementNames;
    }

    /**
     * Get a mapping from element name fingerprints (including fingerprints of substitution group
     * members) to entries in the minima and maxima arrays. Note that this includes entries for all
     * members of substitution groups, so several fingeprints map to the same counter position
     *
     * @return a map from element fingerprints to counter positions
     */

    public IntToIntMap getCounterMap() {
        return counterMap;
    }

    /**
     * Get a mapping from element name fingerprints (including fingerprints of substitution group
     * members) to local or global element declarations to be used to validate the content of each
     * element in the all group
     *
     * @return a map from element name fingerprints to element declarations
     */

    public IntHashMap<ElementDecl> getDeclarationMap() {
        return declarationMap;
    }

    /**
     * Get the list of element wildcards (XSDL 1.1 only)
     *
     * @return a list holding the element wildcards defined in this model group, or
     *         an empty list if there are none
     */

    public List<ElementWildcard> getWildcards() {
        if (wildcards == null) {
            return Collections.emptyList();
        } else {
            return wildcards;
        }
    }

    /**
     * Test whether a complex type constructed using "all" subsumes another complex type.
     * This test is always done after simplification of the content model
     *
     * @param base     the base type, which must be defined using xs:all
     * @param sub      the type that is (supposedly) subsumed by the base type
     * @param compiler used for reporting errors and warnings
     * @return null if the "all" particle subsumes the other; otherwise, a string explaining why not. If the string
     *         starts with "?", this means Saxon is unable to prove one way or the other whether the derived type is a
     *         valid restriction or not.
     * @throws net.sf.saxon.type.SchemaException
     *          if the schema is not valid
     */

    /*@Nullable*/
    protected static String allSubsumes(UserComplexType base, ComplexType sub, SchemaCompiler compiler) throws SchemaException {
        ModelGroupParticle all = base.getAllCompositor();
        if (sub instanceof UserComplexType) {
            Particle subp = ((UserComplexType) sub).makeExtendedParticle(compiler);
            return subsumesParticle(all, subp, compiler);
        } else {
            return "Type " + sub.getDescription() + " is not a valid restriction of a type defined using <xs:all>";
        }
    }

    private static String subsumesParticle(ModelGroupParticle all, Particle subp, SchemaCompiler compiler) throws MissingComponentException {
        while (subp instanceof ModelGroupParticle && subp.getMinOccurs() == 1 && subp.getMaxOccurs() == 1) {
            if (((ModelGroupParticle) subp).getGroup().getSimplifiedContentModel().size() == 1) {
                subp = ((ModelGroupParticle) subp).getGroup().getSimplifiedContentModel().get(0);
            } else {
                break;
            }
        }

        if (subp instanceof ElementParticle || subp instanceof ElementWildcard) {
            // Treat a singleton element as a sequence with one particle for checking purposes
            SequenceModelGroup group = new SequenceModelGroup();
            group.addParticle(subp);
            return ((AllModelGroup) all.getGroup()).subsumesAllOrSequence(group, compiler);


        } else if (subp instanceof ModelGroupParticle) {
            ModelGroup subg = ((ModelGroupParticle) subp).getGroup();
            if (all.getMinOccurs() > subp.getMinOccurs()) {
                return "The derived type has a lower minOccurs (" + subp.getMinOccurs() +
                        ") than the base type (" + all.getMinOccurs() + ")";
            }
            if (all.getMaxOccurs() > 0 && all.getMaxOccurs() < subp.getMaxOccurs()) {
                return "The derived type has a greater maxOccurs (" + subp.getMaxOccurs() +
                        ") than the base type (" + all.getMaxOccurs() + ")";
            }
            if (subg instanceof AllModelGroup || subg instanceof SequenceModelGroup || subg instanceof ModelGroupDefinition) {
                // Treat these together, because a sequence-group is always a valid restriction
                // of an all-group with the same particles

                return ((AllModelGroup) all.getGroup()).subsumesAllOrSequence(subg, compiler);

                // Note: The 1.0 spec says that for all/all, the particles should be in the same order, claiming that this restriction
                // is for the convenience of implementors.  We don't check this rule, because it's not at all
                // convenient either for implementors or users.
            } else if (subg instanceof ChoiceModelGroup) {
                // Each branch of the choice must be validly derived from the base type
                ChoiceModelGroup choice = (ChoiceModelGroup) subg;
                int branchNr = 1;
                for (Particle branch : choice.getParticles()) {
                    String result = subsumesParticle(all, branch, compiler);
                    if (result != null) {
                        return (branchNr == 1 ? "The first branch" : "Branch " + branchNr) +
                                " of the choice is not a valid restriction of the content model of the base type. " +
                                result;
                    }
                    branchNr++;
                }
                return null;
            } else {
                throw new AssertionError("Unknown class of ModelGroup " + subg.getClass().getName());
            }
        } else {
            throw new AssertionError("Unknown class of particle " + subp.getClass().getName());
        }
    }

    private String subsumesAllOrSequence(ModelGroup subg, SchemaCompiler compiler) throws MissingComponentException {
        List<Particle> baseParticles = getSimplifiedContentModel();
        IntHashMap<ElementParticle> baseElements = new IntHashMap<ElementParticle>(baseParticles.size());
        int[] minCounters = new int[getMinima().length];
        int[] maxCounters = new int[getMaxima().length];
        ElementParticle[] baseParticleArray = new ElementParticle[getMinima().length];
        for (Particle p : baseParticles) {
            if (p instanceof ElementParticle) {
                int fp = ((ElementParticle)p).getDeclaration().getFingerprint();
                baseElements.put(fp, (ElementParticle) p);
                baseParticleArray[counterMap.get(fp)] = (ElementParticle) p;
            }
        }
        List<Particle> derivedParticles = subg.getSimplifiedContentModel();
        derivedParticles = flattenParticleList(derivedParticles, 1, 1);
        for (int i = 0; i < derivedParticles.size(); i++)
            if (derivedParticles.get(i) instanceof ElementParticle) {
                ElementParticle derivedElement = (ElementParticle) derivedParticles.get(i);
                int derivedfp = derivedElement.getDeclaration().getFingerprint();

                int counterIndex = counterMap.get(derivedfp);
                if (counterIndex == counterMap.getDefaultValue()) {
                    // Element is not present in base type (no counter allocated)
                    String clarkName = derivedElement.getDisplayName();
                    return "The derived type allows element "
                        + Err.wrap(clarkName, Err.ELEMENT) + " but the base type does not";
                    // TODO: what if the derived element particle maps to a wildcard in the base type?
                }
                minCounters[counterIndex] += derivedElement.getMinOccurs();
                if (derivedElement.getMaxOccurs() == -1) {
                    if (getMaxima()[counterIndex] != -1) {
                        String clarkName = derivedElement.getDisplayName();
                        return "The derived type allows unlimited occurrences of element "
                            + Err.wrap(clarkName, Err.ELEMENT)
                            + " but the base type does not";
                    }
                } else {
                    maxCounters[counterIndex] += derivedElement.getMaxOccurs();
                    int baseMax = getMaxima()[counterIndex];
                    if (baseMax != -1 && maxCounters[counterIndex] > baseMax) {
                        String clarkName = derivedElement.getDisplayName();
                        return "The derived type allows more occurrences (" + maxCounters[counterIndex] + ") of element "
                            + Err.wrap(clarkName, Err.ELEMENT)
                            + " than the base type allows (" + baseMax + ")";
                    }
                }

                ElementParticle baseParticle = baseParticleArray[counterIndex];
                ElementDecl baseElement = baseParticle.getDeclaration();
                String err = UserComplexType.elementParticleMatches(
                    baseElement, derivedElement.getDeclaration(),
                    compiler);
                if (err != null) {
                    return err;
                }

                try {
                    derivedElement.getDeclaration().getType().checkTypeDerivationIsOK(
                        baseElement.getType(), baseElement.getType().getBlock());
                } catch (SchemaException err2) {
                    String clarkName = derivedElement.getDisplayName();
                    return "The type of element " + Err.wrap(clarkName, Err.ELEMENT) +
                        " in the derived type is not validly derived from the type of the corresponding element in the base type";
                }
            } else if (derivedParticles.get(i) instanceof ElementWildcard) {
                ElementWildcard wp = (ElementWildcard) derivedParticles.get(i);
                Wildcard w = wp.getWildcard();
                int counterIndex = 0;
                Wildcard union = null;
                int unionMax = 0;
                for (Particle baseParticle : baseParticles) {
                    if (baseParticle instanceof ElementWildcard) {
                        ElementWildcard vp = (ElementWildcard) baseParticle;
                        Wildcard v = vp.getWildcard();
                        if (v.overlaps(w)) {
                            if (union == null) {
                                union = v;
                            } else {
                                union = Wildcard.makeUnion(union, v, compiler.getNamePool());
                            }
                            if (union.isInexpressible()) {
                                return "The union of the wildcards in the base type that match a wildcard in the derived type is inexpressible";
                            }
                            if (vp.getMaxOccurs() == -1 || unionMax == -1) {
                                unionMax = -1;
                            } else {
                                unionMax += vp.getMaxOccurs();
                            }

                            if (wp.getMaxOccurs() != -1) {
                                maxCounters[counterIndex] += wp.getMaxOccurs();
                                int baseMax = vp.getMaxOccurs();
                                if (baseMax != -1 && maxCounters[counterIndex] > baseMax) {
                                    return "The derived type allows more occurrences (" + maxCounters[counterIndex] + ") of a wildcard "
                                        + "than the base type allows (" + baseMax + ")";
                                }
                            }
                            if (v.compareStrength(w) > 0) {
                                return "A wildcard in the derived type has a less strict processContents than the" +
                                    " corresponding wildcard in the base type";
                            }
                        }
                        if (w.isSubset(v, getConfiguration().getNamePool())) {
                            minCounters[counterIndex] += wp.getMinOccurs();
                        }
                        counterIndex++;
                    } else if (baseParticle instanceof ElementParticle) {
                        // test Wild069: if the wildcard in the derived type matches an element particle in the
                        // base type for which there is no corresponding element particle in the derived type, and
                        // if the wildcard imposes fewer constraints than the element particle, then the type
                        // derivation is not valid.
                        ElementParticle ep = (ElementParticle) baseParticle;
                        if (wp.getWildcard().matches(ep.getTargetComponentName(), true, getConfiguration(), null)) {
                            boolean found = false;
                            for (Particle derivedParticle : derivedParticles) {
                                if (derivedParticle instanceof ElementParticle &&
                                    derivedParticle.getTargetComponentName().equals(ep.getTargetComponentName())) {
                                    found = true;
                                }
                            }
                            if (!found && ep.getDeclaration().getType() != AnyType.getInstance()) {
                                return "The derived type contains a wildcard which matches elements named " +
                                    Err.wrap(ep.getDisplayName(), Err.ELEMENT) +
                                    " but which allows element content that the element particle for " +
                                    Err.wrap(ep.getDisplayName(), Err.ELEMENT) + " in the base type does not allow";
                                // TODO: if the wildcard is strict or lax and there is a global element whose type
                                // is consistent with the local element declaration in the base type, then all is
                                // well and the error is spurious.
                            }
                        }
                    }
                }

                if (unionMax != -1) {
                    if (unionMax == 0) {
                        return "A wildcard in the derived type allows elements that are not permitted by the base type";
                    } else if (wp.getMaxOccurs() == -1) {
                        return "A wildcard in the derived type allows unlimited occurrences, but the base type imposes a limit";
                    } else if (wp.getMaxOccurs() > unionMax) {
                        return "A wildcard in the derived type allows more occurrences (" + wp.getMaxOccurs() +
                            ") than the maximum allowed by the base type (" + unionMax + ")";
                    }
                }
                if (!w.isSubset(union, compiler.getNamePool())) {
                    return "A wildcard in the derived type allows elements that do not match any wildcard in the base type";
                }

            } else {
                return "?A type derived by restriction from <xs:all> must only contain element particles and " +
                    "wildcard particles in its content model";
            }
        for (int i = 0; i < baseParticles.size(); i++) {
            Particle p = baseParticles.get(i);
            if (p instanceof ElementParticle) {
                int counterIndex = counterMap.get(((ElementParticle)p).getDeclaration().getFingerprint());
                if (minCounters[counterIndex] < p.getMinOccurs()) {
                    String clarkName = p.getTargetComponentName().getDisplayName();
                    if (minCounters[counterIndex] == 0) {
                        if (maxCounters[counterIndex] == 0) {
                            return "Element " + Err.wrap(clarkName, Err.ELEMENT) +
                                    " is mandatory in the base type, but is not permitted in the derived type";
                        } else {
                            return "Element " + Err.wrap(clarkName, Err.ELEMENT) +
                                    " is mandatory in the base type, but is not mandatory in the derived type";
                        }
                    } else {
                        return "Element " + Err.wrap(clarkName, Err.ELEMENT) +
                                " allows a smaller number of occurrences (" + minCounters[i] +
                                ") in the derived type than the minimum required by the base type (" +
                                p.getMinOccurs() + ")";
                    }
                }
            }
        }
        if (wildcards != null) {
            for (int i = 0; i < wildcards.size(); i++) {
                if (minCounters[i] < wildcards.get(i).getMinOccurs()) {
                    if (minCounters[i] == 0) {
                        return "A wildcard is mandatory in the base type, but is not permitted in the derived type";
                    } else {
                        return "A wildcard in the base type allows a " +
                                "smaller number of occurrences (" + minCounters[i] +
                                ") in the derived type than the minimum required by the base type (" +
                                wildcards.get(i).getMinOccurs() + ")";
                    }
                }
            }
        }
        return null;
    }

    /**
     * This method takes a list of particles and turns it into a flattened list in which there are
     * no model group particles, and each element or wildcard particle has a computed minimum and
     * maximum cardinality representing the smallest and largest number of times it can appear. The
     * ordering of particles in the returned list is irrelevant (and it may contain several with the
     * same name); the only important thing is the computed cardinalities, which are obtained by
     * multiplying out the occurrence counts on each level of repeating group
     *
     * @param particles the list of particles to be flattened
     * @param min       the minimum number of times this list occurs
     * @param max       the maximum number of times this list occurs
     * @return the flattened particle list
     */

    private List<Particle> flattenParticleList(List<Particle> particles, int min, int max) throws MissingComponentException {
        if (min == 1 && max == 1) {
            // optimization
            boolean alreadyFlat = true;
            for (Particle p : particles) {
                if (p instanceof ModelGroupParticle) {
                    alreadyFlat = false;
                    break;
                }
            }
            if (alreadyFlat) {
                return particles;
            }
        }
        List<Particle> p2 = new ArrayList<Particle>();
        for (Particle p : particles) {
            if (p instanceof ModelGroupParticle) {
                ModelGroup group = (ModelGroup) p.getTarget();
                int newMin = min * p.getMinOccurs();
                if (group instanceof ChoiceModelGroup) {
                    newMin = 0;
                }
                int newMax;
                if (max == -1 || p.getMaxOccurs() == -1) {
                    newMax = -1;
                } else {
                    newMax = max * p.getMaxOccurs();
                }
                List<Particle> mp = flattenParticleList(group.getSimplifiedContentModel(), newMin, newMax);
                p2.addAll(mp);
            } else if (min == 1 && max == 1) {
                p2.add(p);
            } else if (p instanceof ElementParticle) {
                ElementParticle e2 = ((ElementParticle) p).copy();
                e2.setMinOccurs(p.getMinOccurs() * min);
                e2.setMaxOccurs(max == -1 || p.getMaxOccurs() == -1 ? -1 : p.getMaxOccurs() * max);
                p2.add(e2);
            } else if (p instanceof ElementWildcard) {
                ElementWildcard e2 = ((ElementWildcard) p).copy();
                e2.setMinOccurs(p.getMinOccurs() * min);
                e2.setMaxOccurs(max == -1 || p.getMaxOccurs() == -1 ? -1 : p.getMaxOccurs() * max);
                p2.add(e2);
            }
        }
        return p2;

    }
}

// Copyright (c) 2004-2023 Saxonica Limited

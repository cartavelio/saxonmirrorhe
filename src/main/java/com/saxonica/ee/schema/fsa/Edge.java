package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.*;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.jiter.MonoIterator;
import net.sf.saxon.type.*;

import java.util.Iterator;

/**
 * Internal class to represent the data associated with a transition:
 * the element that triggers the transition, and the new state that results
 * from the transition.
 */

public class Edge implements Iterable<Edge> {
    private Term term;
    private Particle particle; // this is the particle to which the transition is attributed for the purpose
    // of evaluating the UPA constraint. In the case of an element that is permitted
    // by virtue of participation in a substitution group, this is the particle for which
    // the element is substituting.
    private AutomatonState targetState;
    //private int countFrom = -1;  // -1 = not counting; 0 = count from zero; 1 = count from 1

    /**
     * Set the state resulting from this transition
     *
     * @param targetState the state at the end of this edge
     */

    public void setTargetState(AutomatonState targetState) {
        this.targetState = targetState;
    }

    /**
     * Get the (fixed) state that is the target of this edge
     *
     * @return the state at the end of this edge
     */

    public AutomatonState getTargetState() {
        return targetState;
    }

    /**
     * Get the dynamic state that results from a transition using this edge,
     * updating any counters that need to be updated
     *
     * @param previousState the previous dynamic state before making this transition
     * @return the dynamic state that results from applying this transition
     * @throws ValidationException if this transition is not possible because the occurrence limits are violated
     */

    public State makeTransition(State previousState) throws ValidationException {
        if (previousState instanceof CountingState) {
            if (((CountingState) previousState).getUnderlyingState() == targetState) {
                return ((CountingState) previousState).incrementCounter();
            } else {
                ((CountingState) previousState).testMinimumReached();
            }
        }

        if (targetState.requiresCounter()) {
            return new CountingState(targetState, 1);
        } else {
            return targetState;
        }

    }

    /**
     * Set the element or wildcard particle causing this transition
     *
     * @param term the term that causes this transition
     */

    public void setTerm(Term term) {
        this.term = term;
    }

    /**
     * Get the element or wildcard particle causing this transition
     *
     * @return the term that causes this transition
     */

    public Term getTerm() {
        return term;
    }

    /**
     * Set the element or wildcard particle causing this transition
     *
     * @param particle the particle that causes this transition
     */

    public void setParticle(Particle particle) {
        this.particle = particle;
    }

    /**
     * Get the element or wildcard particle causing this transition
     *
     * @return the particle that causes this transition if known, otherwise null
     */

    public Particle getParticle() {
        return particle;
    }

    /**
     * Say that this edge is a counting edge. Such an edge will always point back to the original
     * state, and has the effect of updating the counters held in that state
     * @param countFrom -1 if not counting, 0 if counting from zero, 1 if counting from one
     */

//    public void setCounting(int countFrom) {
//        this.countFrom = countFrom;
//    }

    /**
     * Ask whether this edge is a counting edge. Such an edge will always point back to the original
     * state, and has the effect of updating the counters held in that state
     * @return true if this is a counting edge
     */

//    public boolean isCounting() {
//        return countFrom >= 0;
//    }

    /**
     * Get the counter origin for this edge, always zero or one if this is a counting edge
     * @return the counter origin
     */

//    public int getCounterOrigin() {
//        return countFrom;
//    }

    /**
     * Test whether edges in two different finite state machines match each other. This is called
     * while establishing whether one type subsumes another.
     * <p>From Thompson &amp; Tobin 2003: an edge BE matches another edge DE iff one of the following three
     * conditions holds: (1) they are labelled as element declarations with the same expanded name;
     * (2) BE is labelled with a wildcard and DE is labelled with an element declaration in a namespace
     * allowed by the wildcard; (3) BE and DE are both wildcards and DE's label is an intensional subset
     * of BE's. In case (3) we add the rule, not in Thompson and Tobin, that the processContents of DE must
     * be at least as strong as the processContents of BE.</p>
     * <p>It is also necessary that the types of B and D are compatible, and that their nillability
     * is compatible, and that their value constraints are compatible, and that the disallowed
     * substitutions of D is a superset of those of B.</p>
     *
     * @param de       the edge in a finite state machine representing a derived type, and which corresponds
     *                 to this edge in the finite state machine for the base type
     * @param compiler user for error reporting
     * @return null if the edges match, or a string explaining the reason for the non-match if not.
     */

    /*@Nullable*/
    public String matches(Edge de, SchemaCompiler compiler) throws MissingComponentException {

        // If the two particles are references to the same element declaration, then all is well
        if (term == de.term) {
            return null;
        }

        // If the two particles are references to different element declarations with the same
        // element name, then they need to be compatible
        if ((term instanceof ElementDecl) && (de.term instanceof ElementDecl) &&
                ((ElementDecl) term).getFingerprint() == ((ElementDecl) de.term).getFingerprint()) {
            ElementDecl base = (ElementDecl) term;
            ElementDecl sub = (ElementDecl) de.term;
            SchemaType baseType = base.getType();
            SchemaType subType = sub.getType();

            // (a) If base is not nillable, then sub must not be nillable

            if (sub.isNillable() && !base.isNillable()) {
                return "The element in the derived type is nillable but the corresponding element in the base type is not";
            }

            // (b) If base has a fixed value constraint, then sub must have a fixed value constraint with
            // the same value

            if (base.getFixedValue() != null) {
                if (sub.getFixedValue() == null) {
                    return "The element in the base type has a fixed value, but the corresponding element in derived type does not";
                }
                if (!SimpleTypeComparison.getInstance().equal(sub.getFixedValue(), base.getFixedValue())) {
                    return "The element in the derived type has a different fixed value from the corresponding element in the base type";
                }
            }

            // (c) DE's type definition must be the same as BE's, or be derived by restriction from it
            boolean elementTypeOK = true;
            String reason = null;

            if (subType.isSimpleType() /*&& baseType.isSimpleType()*/) {
                reason = UserSimpleType.isTypeDerivationOK(((SimpleType) subType), baseType, 0);
                elementTypeOK = (reason == null);
            } else {
                // TODO: invoke Type Derivation OK (Complex)
                while (!baseType.isSameType(subType)) {
                    if (subType.getDerivationMethod() != Derivation.DERIVATION_RESTRICTION) {
                        elementTypeOK = false;
                        break;
                    }
                    subType = subType.getBaseType();
                    if (subType == null) {
                        elementTypeOK = false;
                        break;
                    }
                }
            }
            if (!elementTypeOK) {
                return "The type of the element in the restricted content model is not validly derived from the " +
                        "type of the corresponding element in the base content model" +
                        (reason == null ? "" : ". " + reason);
            }

            // (d) If B has a fixed value constraint, D must have a fixed value constraint with the same value
            if (base.getFixedValue() != null &&
                    !SimpleTypeComparison.getInstance().equal(base.getFixedValue(), sub.getFixedValue())) {
                return "The fixed value of the element in the base content model is not reflected in the corresponding " +
                        "element of the restricted content model";
            }

            // (e) DE's identity constraints must be a superset of BE's identity constraints
            // Currently we only do this check partially
            int baseUniqueCount = 0;
            int baseKeyCount = 0;
            int baseKeyRefCount = 0;
            int derivedUniqueCount = 0;
            int derivedKeyCount = 0;
            int derivedKeyRefCount = 0;
            for (IdentityConstraintReference icr : base.getIdentityConstraints()) {
                IdentityConstraint ic = icr.getTarget();
                if (ic instanceof Unique) {
                    baseUniqueCount++;
                } else if (ic instanceof Key) {
                    baseKeyCount++;
                } else if (ic instanceof KeyRef) {
                    baseKeyRefCount++;
                }
            }
            for (IdentityConstraintReference icr : sub.getIdentityConstraints()) {
                IdentityConstraint ic = icr.getTarget();
                if (ic instanceof Unique) {
                    derivedUniqueCount++;
                } else if (ic instanceof Key) {
                    derivedKeyCount++;
                } else if (ic instanceof KeyRef) {
                    derivedKeyRefCount++;
                }
            }
            if (baseUniqueCount > derivedUniqueCount) {
                return "Element in base content model has a uniqueness constraint that is not present" +
                        " in the corresponding element of the restricted content model";
            }
            if (baseKeyCount > derivedKeyCount) {
                return "Element in base content model has a key constraint that is not present" +
                        " in the corresponding element of the restricted content model";
            }
            if (baseKeyRefCount > derivedKeyRefCount) {
                return "Element in base content model has a keyref constraint that is not present" +
                        " in the corresponding element of the restricted content model";
            }
            if (baseUniqueCount > 0 || baseKeyCount > 0 || baseKeyRefCount > 0) {
                compiler.warning("Saxon is not able to verify that the identity constraints defined on " +
                        "element " + base.getDisplayName() + " are compatible between the base type and the derived type",
                                 SaxonErrorCode.SXSD1009, sub);
            }

            // (f) If (DE) ends in a final state then BE must also do so
            // (this condition was omitted from the T+T 2003 algorithm)
            if (de.getTargetState().isFinalState() &&
                    !(getTargetState().isFinalState() || getTargetState().isConditionallyFinalState())) {
                return "Not final";
            }

            // (g) Check that the disallowed substitutions (block) of B are a superset of those
            // of D
            if ((base.getBlock() & ~sub.getBlock()) != 0) {
                return "The disallowed substitutions of the element in the restricted content model (@block) must be " +
                        "a superset of those in the base content model";
            }

            // (h) The type tables of B and D must either both be absent, or must be equivalent
            // (see spec bug 12185)
            if (!base.hasSameTypeTable(sub)) {
                return "The type table of the element in the restricted content model must be the same as " +
                        "the type table of the element in the base content model";
            }

            return null;
        }

        // The following code implements the additional tests described in the section
        // "Another problem with wildcards" in the Thompson & Tobin paper: namely, it
        // checks that where the candidate subtype uses an element particle in a position
        // where the base type uses a wildcard, then the element particle is consistent
        // with the global element declarations available in the schema: otherwise validation
        // against the local element could succeed where validation against the wildcard fails,
        // for lack of a suitable global element declaration.

        // This code rejects many schemas in the test suite that other processors consider valid
        // (for example ctG001, ctG007-011, particlesJa001-4. For the moment, therefore, this code
        // generates warnings.

        if ((term instanceof Wildcard) && (de.term instanceof ElementDecl) &&
                ((Wildcard) term).matches(((ElementDecl) de.term).getTargetNamespace())) {
            String mode = ((Wildcard) term).getProcessContents();
            final PreparedSchema schema = compiler.getPreparedSchema();
            if (mode.equals("skip")) {
                return null;
            } else if (mode.equals("strict")) {
                ElementDecl een = (ElementDecl)schema.getElementDecl(((ElementDecl) de.term).getComponentName());
                if (een == null) {
                    compiler.warning("There is no global element declaration named " +
                            Err.wrap(((ElementDecl) de.term).getDisplayName()) +
                            ", so the strict wildcard in the base type can never be satisfied",
                                     SaxonErrorCode.SXSD1003, (ElementDecl) de.term);
                    return null;
                }
                SchemaType eent = een.getType();
                SchemaType et = ((ElementDecl) de.term).getType();
                while (et != null) {
                    if (et == eent) {
                        return null;
                    }
                    if (et.getDerivationMethod() != Derivation.DERIVATION_RESTRICTION) {
                        break;
                    }
                    et = et.getBaseType();
                }
                compiler.warning("The type of the local element declaration named " +
                        Err.wrap(((ElementDecl) de.term).getDisplayName()) +
                        " is not derived by restriction from that of the global element" +
                        " with the same name, so the strict wildcard in the base type can never be satisfied",
                                 SaxonErrorCode.SXSD1011, (ElementDecl) de.term);
                return null;

            } else if (mode.equals("lax")) {
                ElementDecl een = (ElementDecl)schema.getElementDecl(((ElementDecl) de.term).getComponentName());
                if (een == null) {
                    return null;
                }
                SchemaType eent = een.getType();
                SchemaType et = ((ElementDecl) de.term).getType();
                while (et != null) {
                    if (et == eent) {
                        return null;
                    }
                    if (et.getDerivationMethod() != Derivation.DERIVATION_RESTRICTION) {
                        break;
                    }
                    et = et.getBaseType();
                }
                compiler.warning("The type of the local element declaration named " +
                        Err.wrap(((ElementDecl) de.term).getDisplayName()) +
                        " is not derived by restriction from that of the global element" +
                        " with the same name, so the lax wildcard in the base type can never be satisfied"
                        , SaxonErrorCode.SXSD1011, (ElementDecl) de.term);
                return null;
            }
            return null;
        }
        if ((term instanceof Wildcard) && (de.term instanceof Wildcard)) {
            if (!((Wildcard) de.term).isSubset((Wildcard) term, compiler.getNamePool())) {
                return "The xs:any wildcard in the restricted type allows namespaces that are " +
                        "not permitted by the wildcard in the base type";
            }
            if (((Wildcard) term).compareStrength((Wildcard) de.term) > 0) {
                return "The xs:any wildcard in the restricted type has a processContents that is less " +
                        "strict than the processContents in the base type";
            }
            return null;
        }
        return "No match";
    }

    @Override
    public Iterator<Edge> iterator() {
        return new MonoIterator<>(this);
    }

    /**
     * Serialize this Edge as part of the serialization of a schema component model
     *
     * @param serializer object that formats the model output
     * @throws XPathException if a serialization error occur
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("edge");
        serializer.emitAttribute("term", serializer.getId((SchemaComponent) term, false));
        serializer.emitAttribute("to", targetState.getStateNumber() + "");
        serializer.endElement();
    }

    /**
     * Special singular Term identifying a transition to be used when the maxOccurs value of
     * a counting state is reached. This enables content models like a(2,2)a to be treated as non-ambiguous.
     * See test case modelGroups mg005.
     */

    public static class MaxOccursTerm implements Term {

        private MaxOccursTerm() {
        }

        private static final MaxOccursTerm THE_INSTANCE = new MaxOccursTerm();

        public static MaxOccursTerm getInstance() {
            return THE_INSTANCE;
        }
    }

    public static class MaxOccursParticle extends ModelGroupParticle {

        public MaxOccursParticle() {
            super(null, null);
        }

        private static final MaxOccursParticle THE_INSTANCE = new MaxOccursParticle();

        public static MaxOccursParticle getInstance() {
            return THE_INSTANCE;
        }

        @Override
        public Term getTerm() {
            return MaxOccursTerm.getInstance();
        }
    }

    /**
     * Special singular Term identifying a transition to be used when the maxOccurs value of
     * a counting state is reached. This enables content models like a(2,2)a to be treated as non-ambiguous.
     * See test case modelGroups mg005.
     */

    public static final MaxOccursParticle MAX_OCCURS_PARTICLE = new MaxOccursParticle();

}

// Copyright (c) 2004-2023 Saxonica Limited

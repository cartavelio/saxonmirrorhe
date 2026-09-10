package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.*;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.tree.jiter.MonoIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRangeSet;
import net.sf.saxon.z.IntSet;

import java.util.*;

/**
 * Class representing a finite state machine, as a collection of states with transitions between them.
 * <p>The finite state machine allows states with counters. The static machine created by the schema compiler
 * contains states of type AutomatonState; these may have a minOccurs and maxOccurs value. When such a state
 * is entered at validation time, a CountingState is dynamically created that contains a pointer to the fixed
 * AutomatonState plus an integer counter. Any transition that goes from the state to itself causes a new
 * CountingState to be created with an incremented value of the counter (provided it does not exceed the maxOccurs());
 * any transition to a different state causes the counter to be checked against the minOccurs value.</p>
 */

public class FiniteStateMachine {

    private int statesUsed = 0;
    private AutomatonState initialState;
    private AutomatonState[] states = new AutomatonState[20];
    private final Set<TermParticlePair> alphabet = new HashSet<>(20);
    // the set of elementDecl and elementWildcard terms used in this machine
    private Wildcard openContentWildcard = null;
    private boolean openContentInterleaved = false;
    private final UserComplexType complexType; // the complex type implemented by this finite state machine

    /**
     * Create a finite state machine
     *
     * @param type         the type whose content model is implemented by this finite state machine
     * @param determinized true if this machine is determinized
     */

    public FiniteStateMachine(UserComplexType type, boolean determinized) {
        this.complexType = type;
        if (determinized) {
            initialState = new DeterminizedState(this);
        } else {
            initialState = new NonDeterminizedState(this);
        }
    }

    /**
     * Allocate a unique number to a state, and index the state from the FSM
     *
     * @param state the state whose number is to be allocated
     */

    void allocateStateNumber(AutomatonState state) {
        if (statesUsed > states.length - 1) {
            states = Arrays.copyOf(states, statesUsed*2);
        }
        states[statesUsed] = state;
        state.stateNumber = statesUsed++;
    }

    public UserComplexType getComplexType() {
        return complexType;
    }

    /**
     * Determine the number of states in this finite state machine
     *
     * @return the number of states
     */

    public int getNumberOfStates() {
        return statesUsed;
    }

    /**
     * Get the initial state of this finite state machine
     *
     * @return the initial state
     */

    public AutomatonState getInitialState() {
        return initialState;
    }

    /**
     * Set the initial state of this finite state machine
     *
     * @param initialState the initial state
     */

    public void setInitialState(AutomatonState initialState) {
        this.initialState = initialState;
    }

    /**
     * Get the state with a given unique state number
     *
     * @param number the number of the state
     * @return the state with this unique state number
     */

    public AutomatonState getState(int number) {
        return states[number];
    }

    /**
     * Set the open content wildcard for this machine
     *
     * @param wildcard    the open content wildcard (may be null)
     * @param interleaved true if open content may be interleaved, false if it must be suffixed
     */

    public void setOpenContentWildcard(Wildcard wildcard, boolean interleaved) {
        openContentWildcard = wildcard;
        openContentInterleaved = interleaved;
    }

    /**
     * Static method to translate a particle to a Finite State Automaton, returning
     * the start state of the FSA. This precisely follows Henry Thompson's and Richard Tobin's algorithm,
     * <a href="http://www.ltg.ed.ac.uk/~ht/XML_Europe_2003.html">published here</a>
     *
     * @param compiler    user for error reporting
     * @param particle    the particle to be compiled, which is either an ElementDeclaration,
     *                    a WildCard, a choice, or a sequence
     * @param endState    the State representing the final state of the automaton
     * @param subjectType the complex type to which this particle belongs
     * @param machine     the finite state machine to which the state belongs
     * @return the initial state of the new Automaton
     * @throws net.sf.saxon.type.SchemaException
     *          if XSD constraints are violated
     * @throws MissingComponentException
     *          if unresolved references in the schema are found
     */

    public static NonDeterminizedState compileParticle(SchemaCompiler compiler,
                                                       Particle particle,
                                                       NonDeterminizedState endState,
                                                       UserComplexType subjectType,
                                                       FiniteStateMachine machine)
            throws SchemaException, MissingComponentException {

        NonDeterminizedState n = endState;
        int max = particle.getMaxOccurs();
        int min = particle.getMinOccurs();

        // If the particle is an element or wildcard particle,
        // and the minOccurs or maxOccurs are not 0, 1, or infinity, use a counting state.
        if ((particle.getTerm() instanceof ElementDecl || particle.getTerm() instanceof Wildcard)
                && (min > 1 || max > 1)
                && (min == max || !particle.isVulnerable())) {
            NonDeterminizedState t = new NonDeterminizedState(machine);
            compileCountingTerm(particle, t, min, max, subjectType, compiler, machine);
            if (min == max) {
                // If min == max the conditions for leaving the counting state are unambiguous, so we define
                // a transition to a new state that is made when max is reached. This enables content
                // models like A(2,2)A(1,5) to be compiled successfully. It also allows a particle with
                // min=max to form part of a repeated sequence or choice group.
                Edge leavingEdge = new Edge();
                leavingEdge.setTerm(Edge.MaxOccursTerm.getInstance());
                leavingEdge.setTargetState(n);
                t.setMaxTransition(leavingEdge);
                t.setFinalState(n.isFinalState());
                machine.alphabet.add(new TermParticlePair(Edge.MaxOccursTerm.getInstance(), Edge.MAX_OCCURS_PARTICLE));
            } else {
                t.addLambdaTransition(n);
            }
            NonDeterminizedState start = compileTerm(compiler, particle, t, subjectType, machine);
            if (min == 0) {
                start.addLambdaTransition(n);
            }
            return start;
        } else {
            // Make some unpleasant adjustments to the requested min and max values
            // to stop the algorithms blowing the heap size.
            int[] limits = compiler.getOccurrenceLimits();
            if (max > limits[1]) {
                compiler.warning("maxOccurs value of " + max + " treated as unbounded", SaxonErrorCode.SXSD1012, subjectType);
                max = Particle.UNBOUNDED;
            }
            if (min > limits[0]) {
                compiler.warning("minOccurs value of " + min + " treated as " + limits[0], SaxonErrorCode.SXSD1012, subjectType);
                min = limits[0];
            }
        }

        if (max == Particle.UNBOUNDED) {
            NonDeterminizedState t = new NonDeterminizedState(machine);
            NonDeterminizedState b = compileTerm(compiler, particle, t, subjectType, machine);
            b.addLambdaTransition(n);
            t.addLambdaTransition(b);
            n = b;
        } else if (particle.getTerm() instanceof AllModelGroup) {
            // Path not used
        } else {
            int limit = max - min;
            for (int i = 0; i < limit; i++) {
                NonDeterminizedState b = compileTerm(compiler, particle, n, subjectType, machine);
                b.addLambdaTransition(endState);
                n = b;
            }
        }
        int count = min;
        for (int i = 0; i < count; i++) {
            n = compileTerm(compiler, particle, n, subjectType, machine);
        }
        return n;
    }

    /**
     * Compile a term, again following the Thompson and Tobin algorithm.
     *
     * @param compiler    user for error reporting
     * @param particle    the particle containing the term to be compiled, which is either an ElementDeclaration,
     *                    a WildCard, a choice, or a sequence
     * @param endState    the State representing the final state of the automaton
     * @param subjectType the complex type to which this term belongs
     * @param machine     the finite state machine to which this state belongs
     * @return the initial state of the new Automaton
     * @throws net.sf.saxon.type.SchemaException
     *          if an XSD constraint is violated
     * @throws MissingComponentException
     *          if unresolved links
     *          exist in the scheam
     */

    private static NonDeterminizedState compileTerm(SchemaCompiler compiler,
                                                    Particle particle,
                                                    NonDeterminizedState endState,
                                                    UserComplexType subjectType,
                                                    FiniteStateMachine machine)
            throws SchemaException, MissingComponentException {
        PreparedSchema schema  = compiler.getPreparedSchema();
        Term term = particle.getTerm();
        if (term instanceof Wildcard) {
            machine.alphabet.add(new TermParticlePair(term, particle));
            NonDeterminizedState b = new NonDeterminizedState(machine);
            Edge edge = new Edge();
            edge.setTerm(term);
            edge.setParticle(particle);
            edge.setTargetState(endState);
            b.addWildcardTransition(compiler, edge);
            return b;
        } else if (term instanceof ElementDecl) {
            machine.alphabet.add(new TermParticlePair(term, particle));
            NonDeterminizedState b = new NonDeterminizedState(machine);

            // A transition for each element in the substitution group is added even if it is an
            // abstract element, even though the transition will never occur, so that UPA can be detected.

            Edge edge = new Edge();
            edge.setTerm(term);
            edge.setParticle(particle);
            edge.setTargetState(endState);
            b.addSpecificTransition(edge, compiler);

            int fp = ((ElementDecl) term).getFingerprint();

            for (ElementDecl elem : schema.getSubstitutionGroupMembers(((ElementDecl) term))) {
                if (elem.getFingerprint() != fp) {
                    edge = new Edge();
                    edge.setTerm(elem);
                    machine.alphabet.add(new TermParticlePair(elem, particle));
                    edge.setParticle(particle);
                    edge.setTargetState(endState);
                    b.addSpecificTransition(edge, compiler);
                }
                elem.registerComplexTypeUsingThisElement(subjectType);
            }
            return b;
        } else if (term instanceof ChoiceModelGroup || term instanceof AllModelGroup) {
            NonDeterminizedState b = new NonDeterminizedState(machine);
            for (Particle p : ((ModelGroup) term).getParticles()) {
                NonDeterminizedState c = compileParticle(compiler, p, endState, subjectType, machine);
                b.addLambdaTransition(c);
            }
            return b;
        } else {
            if (term instanceof SequenceModelGroup || term instanceof ModelGroupDefinition) {
                NonDeterminizedState n = endState;
                List<Particle> reversedParticles = new ArrayList<>(10);
                for (Particle p : ((ModelGroup) term).getParticles()) {
                    // need to handle the particles in reverse order
                    reversedParticles.add(0, p);
                }
                for (Particle p : reversedParticles) {
                    n = compileParticle(compiler, p, n, subjectType, machine);
                }
                return n;
            } else {
                throw new IllegalStateException("Unknown type of term " + term.getClass());
            }
        }

        // Note, we could distribute this code over the various classes that implement
        // Term, making it a polymorphic method; however, the algorithm is clearer if the
        // code is all in one place.
    }

    /**
     * Compile a term, again following the Thompson and Tobin algorithm.
     *
     * @param particle      the particle containing the term to be compiled, which is either an ElementDeclaration,
     *                      a WildCard, a choice, or a sequence
     * @param countingState the State representing the final state of the automaton
     * @param min           the minimum number of occurrences of the term
     * @param max           the maximum number of occurrences of the term (-1 means unbounded)
     * @param subjectType   the complex type to which this term belongs
     * @param compiler      the schema compiler
     * @param machine       the finite state machine to which this state belongs @return the initial state of the new Automaton
     * @throws net.sf.saxon.type.SchemaException
     *          if an XSD violation is discovered
     * @throws MissingComponentException
     *          if unresolved references are found in the schema
     */

    private static void compileCountingTerm(
            Particle particle,   //must be an ElementDecl or a Wildcard
            NonDeterminizedState countingState,
            int min,
            int max,
            UserComplexType subjectType,
            SchemaCompiler compiler,
            FiniteStateMachine machine)
            throws SchemaException, MissingComponentException {

        PreparedSchema schema = compiler.getPreparedSchema();
        Term term = particle.getTerm();
        machine.alphabet.add(new TermParticlePair(term, particle));

        // A transition for each element in the substitution group is added even if it is an
        // abstract element, even though the transition will never occur, so that UPA can be detected.

        Edge edge = new Edge();
        edge.setTerm(term);
        edge.setParticle(particle);
        edge.setTargetState(countingState);
        countingState.setLimits(min, max);

        if (term instanceof ElementDecl) {
            countingState.addSpecificTransition(edge, compiler);
            int fp = ((ElementDecl) term).getFingerprint();

            for (ElementDecl elem : schema.getSubstitutionGroupMembers(((ElementDecl) term))) {
                if (elem.getFingerprint() != fp /*&& !elem.isAbstract() */) {
                    machine.alphabet.add(new TermParticlePair(elem, particle));
                    edge = new Edge();
                    edge.setParticle(particle);
                    edge.setTerm(elem);
                    edge.setTargetState(countingState);
                    countingState.addSpecificTransition(edge, compiler);
                }
                elem.registerComplexTypeUsingThisElement(subjectType);
            }
        } else {
            countingState.addWildcardTransition(compiler, edge);
        }
    }

    /**
     * Display the finite state machine. This is a diagnostic method for
     * internal use.
     *
     * @param err the output destination
     */

    public void display(net.sf.saxon.lib.Logger err) {
        err.info("Initial state: " + initialState.stateNumber);
        for (int i = statesUsed - 1; i >= 0; i--) {
            // display backwards because we tend to allocate states in that order
            states[i].displayState(err);
        }
    }

    /**
     * Test whether one finite state machine subsumes another FSM: that is, whether for each path through
     * the second FSM, there is a corresponding path through the first FSM.  The algorithm
     * used is as published by Thompson and Tobin, XML Europe 2003.
     *
     * @param base     the first FSM (representing the base type)
     * @param sub      the other FSM (representing the type that is derived by restriction, validly or otherwise)
     * @param compiler used for error reporting
     * @return null indicating that the first machine does indeed subsume the other; or a string indicating
     *         why it doesn't.
     */

    // Note: this algorithm doesn't cover the case where a wildcard in the subtype (such as <xs:any ns="a b">) is
    // covered by two separate wildcards in the supertype. See Thompson and Tobin for discussion of this case.
    public static String subsumesMachine(FiniteStateMachine base, FiniteStateMachine sub, SchemaCompiler compiler) {

        List<State[]> unprocessed = new ArrayList<>(30);
        List<State[]> processed = new ArrayList<>(30);
        Set<State> finalDerivedStates = new HashSet<>(30);

        if (sub.initialState.isFinalState() && sub.initialState.getMinOccurs() <= 0) {
            finalDerivedStates.add(sub.initialState);
            // The following case would be caught anyway, but we handle it specially to get diagnostics
            if (!(base.initialState.isFinalState() && sub.initialState.getMinOccurs() <= 0)) {
                return "The derived type permits empty content, but the base type does not";
            }
        }

        // "Initialize SS to be a set containing the unprocessed pair <B.initialState, D.initialState>"
        // Unprocessed states in SS are represented by the list "unprocessed", processed states by the
        // list "processed".

        State[] initialPair = {base.getInitialState(), sub.getInitialState()};
        unprocessed.add(initialPair);

        // Choose an unprocessed pair P

        while (!unprocessed.isEmpty()) {
            State[] p = unprocessed.get(0);

            State baseState = p[0];
            State derivedState = p[1];

            // for each edge DE leaving P.derived, if there is an edge leaving P.base which matches DE,
            // then add a pair <BE.end, DE.end> to SS (unless it's already there), otherwise fail

            for (Edge derivedEdge : derivedState.getEdges()) {
                State newDerivedState;
                try {
                    newDerivedState = derivedEdge.makeTransition(derivedState);
                } catch (ValidationException e) {
                    throw new AssertionError("Invalid derived transition");
                }
                if (newDerivedState.isFinalState()) {
                    finalDerivedStates.add(newDerivedState);
                }
                Edge baseEdge = null;
                if (derivedEdge.getTerm() instanceof ElementDecl) {
                    baseEdge = baseState.getTransition(((ElementDecl) derivedEdge.getTerm()).getFingerprint(),
                            base.complexType, compiler.getPreparedSchema());
                    String s = "No match";
                    if (baseEdge != null) {
                        try {
                            s = baseEdge.matches(derivedEdge, compiler);
                        } catch (MissingComponentException e) {
                            return "Cannot prove the restriction is valid, because there are missing schema components. " + e.getMessage();
                        }
                    }
                    if (s != null) {
                        switch (s) {
                            case "No match":
                                return "The restricted type allows element " +
                                        ((ElementDecl) derivedEdge.getTerm()).getDisplayName() +
                                        " where the base type does not";
                            case "Not final":
                                return "The restricted type allows the content to finish after element " +
                                        ((ElementDecl) derivedEdge.getTerm()).getDisplayName() +
                                        " where the base type does not";
                            default:
                                return "The definition of element " +
                                        ((ElementDecl) derivedEdge.getTerm()).getDisplayName() +
                                        " differs between the restricted type and the base type. " + s;
                        }
                    }
                } else {
                    // derived particle is a wildcard: see if it matches a wildcard in the base type
                    Edge[] baseEdges = baseState.getWildcardEdges();
                    boolean foundWildcard = false;
                    String s = null;
                    for (Edge baseEdge1 : baseEdges) {
                        baseEdge = baseEdge1;
                        try {
                            s = baseEdge.matches(derivedEdge, compiler);
                        } catch (MissingComponentException e) {
                            return "Cannot prove the restriction is valid, because there are missing schema components. " + e.getMessage();
                        }
                        if (s == null) {
                            foundWildcard = true;
                            break;
                        }
                    }
                    if (!foundWildcard) {
                        return "The restricted type allows xs:any content that is not allowed by the base type. " + s;
                    }
                }

                // Add the corresponding end states to the unprocessed list unless already in one of the lists
                boolean foundExistingPair = false;
                State[] end;
                try {
                    end = new State[]{baseEdge.makeTransition(baseState), newDerivedState};
                } catch (ValidationException e) {
                    return "Occurrence limits on the restricted type exceed those on base type";
                }
                for (State[] anUnprocessed : unprocessed) {
                    if (anUnprocessed[0].equals(end[0]) && anUnprocessed[1].equals(end[1])) {
                        foundExistingPair = true;
                        break;
                    }
                }
                for (State[] aProcessed : processed) {
                    if (aProcessed[0].equals(end[0]) && aProcessed[1].equals(end[1])) {
                        foundExistingPair = true;
                        break;
                    }
                }
                if (!foundExistingPair) {
                    unprocessed.add(end);
                }
            }

            // Mark P as processed, and repeat until no unprocessed pairs are left

            unprocessed.remove(0);
            processed.add(p);
        }

        // Check that for every end-state in D, every processed pair in SS which leads to this end-state pairs it
        // with an end-state of B, otherwise fail

        for (State e : finalDerivedStates) {
            Iterator<State[]> processedPairIterator = processed.iterator();
            boolean unfound = false;
            while (processedPairIterator.hasNext()) {
                State[] pair = processedPairIterator.next();
                if (pair[1].equals(e) && !pair[0].isFinalState()) {
                    unfound = true;
                    break;
                }
            }
            if (unfound) {
                return "The restricted type allows a final state that is not allowed by the base type";
            }
        }
        return null;
    }

    /**
     * Determinize the finite state machine: that is, ensure that there are no ambiguous
     * transitions from this state. When presented with an input token, the
     * FSA can then examine the specific transitions from this state and either
     * choose one of them, or throw an error.
     * <p>This is a revised implementation, based more closely on Aho and Ullman p93,
     * to try to eliminate the rare problems that occur when a state has two transitions
     * for the same symbol. (Given UPA, this should occur only with nested loops, e.g.
     * (a{10,11}){1,3} which allows a sequence of 10, 20, 30, 11, 22, or 33 a's.)</p>
     *
     * @param compiler the schema compiler
     * @param nfsa     the finite state machine to be determinized
     * @return the new deterministic finite state machine
     * @throws net.sf.saxon.type.SchemaException
     *          if an XSD constraint is violated
     */

    public static FiniteStateMachine determinize(SchemaCompiler compiler, FiniteStateMachine nfsa) throws SchemaException {

        FiniteStateMachine dfsa = new FiniteStateMachine(nfsa.complexType, true);
        dfsa.setOpenContentWildcard(nfsa.openContentWildcard, nfsa.openContentInterleaved);
        LinkedList<AutomatonState> unmarkedStatesOfD = new LinkedList<>();
        HashMap<IntSet, AutomatonState> statesOfD = new HashMap<>(20);
                // the states of D, indexed by their provenance
        DeterminizedState initialD = (DeterminizedState) dfsa.getInitialState();
        initialD.setProvenance(lambdaClosure(new MonoIterator<>(nfsa.getInitialState())));
        initialD.setFinalState(containsFinalState(nfsa, initialD.getProvenance()));
        initialD.setLimits(nfsa.getInitialState().getMinOccurs(), nfsa.getInitialState().getMaxOccurs());
        unmarkedStatesOfD.add(initialD);
        statesOfD.put(initialD.getProvenance(), initialD);

        // while there is an unmarked state x of D do
        while (!unmarkedStatesOfD.isEmpty()) {
            DeterminizedState x = (DeterminizedState) unmarkedStatesOfD.pollFirst();
            // mark x
            //unmarkedStatesOfD.remove(x);
            // for each input symbol a do
            for (TermParticlePair pair : nfsa.alphabet) {
                Particle particle = pair.particle;
                Term a = pair.term;
                // let T be the set of states to which there is a transition on a
                Set<AutomatonState> T = x.reachableStates(pair, nfsa);
                //int countOrigin = x.getCounterOrigin(a, nfsa);
                if (!T.isEmpty()) {
                    // y := lambda-closure(T)
                    IntSet key = lambdaClosure(T.iterator());
                    DeterminizedState dy = (DeterminizedState) statesOfD.get(key);
                    // if y has not yet been added to the set of states of D
                    if (dy == null) {
                        dy = new DeterminizedState(dfsa);
                        dy.setProvenance(key);
                        dy.setFinalState(containsFinalState(nfsa, key));
                        int[] limits = getOccurrenceLimits(nfsa, key);
                        dy.setLimits(limits[0], limits[1]);
                        // make y an unmarked state of D
                        unmarkedStatesOfD.add(dy);
                        statesOfD.put(key, dy);
                    }
                    // add a transition from x to y labelled a if not already present
                    x.addEdge(compiler, a, particle, dy);
                }
            }
        }
        // clear out unwanted working data to free some memory
        for (int i = dfsa.getNumberOfStates() - 1; i >= 0; i--) {
            ((DeterminizedState) dfsa.getState(i)).setProvenance(null);
        }
        return dfsa;
    }

    /**
     * Determine whether one of the states in a set is a final state
     *
     * @param machine the Finite State Machine
     * @param states  the set of states to be examined
     * @return true if one or more states in the supplied set of states is a final state
     */

    private static boolean containsFinalState(FiniteStateMachine machine, IntSet states) {
        for (IntIterator i = states.iterator(); i.hasNext(); ) {
            final int state = i.next();
            if (machine.getState(state).isConditionallyFinalState()) {
                return true;
            }
        }
        return false;
    }

    private static int[] getOccurrenceLimits(FiniteStateMachine machine, IntSet states) {
        for (IntIterator i = states.iterator(); i.hasNext(); ) {
            final int state = i.next();
            AutomatonState aState = machine.getState(state);
            if (aState.getMinOccurs() != -1) {
                return new int[]{aState.getMinOccurs(), aState.getMaxOccurs()};
            }
        }
        return new int[]{-1, -1};
    }

    /**
     * Get the open content wildcard, if any
     *
     * @return the open content wildcard for this finite state machine, if one is defined
     */

    /*@Nullable*/
    public Wildcard getOpenContentWildcard() {
        return openContentWildcard;
    }

    /**
     * Ask whether the open content wildcard for this finite state machine (assuming there is one)
     * permits interleaved content (as opposed to suffixed content only)
     *
     * @return true if the open content wildcard permits interleaved content.
     */

    public boolean isOpenContentInterleaved() {
        return openContentInterleaved;
    }

    /**
     * Compute the lambda-closure of a set of states: that is, the set of states
     * reachable from an initial set of states using lambda transitions alone.
     * See Aho and Ullman p92 (they call it epsilon-CLOSURE).
     *
     * @param T_iter iterator over the initial set of states whose closure is required (a Set whose
     *               members are AutomatonState objects in the original NFSA)
     * @return a set of integers, the identifiers of the states reachable from
     *         states in T via lambda transitions alone.
     */

    private static IntSet lambdaClosure(Iterator<AutomatonState> T_iter) {
        Stack<AutomatonState> stack = new Stack<>();
        IntRangeSet result = new IntRangeSet();
        while (T_iter.hasNext()) {
            final NonDeterminizedState state = (NonDeterminizedState) T_iter.next();
            result.add(state.stateNumber);
            stack.push(state);
        }
        while (!stack.isEmpty()) {
            NonDeterminizedState s = (NonDeterminizedState) stack.pop();
            List<AutomatonState> transitions = s.getLambdaTransitions();
            if (transitions != null) {
                for (AutomatonState t : transitions) {
                    if (result.add(t.stateNumber)) {
                        stack.push(t);
                    }
                }
            }
        }
        return result;
    }

    /**
     * Output a representation of this finite state machine
     *
     * @param serializer the schema model serializer to which this representation is to be written
     * @throws XPathException if any error occurs during serialization
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("finiteStateMachine");
        serializer.emitAttribute("initialState", initialState.getStateNumber() + "");
        for (int s = 0; s < statesUsed; s++) {
            states[s].serialize(serializer);
        }
        serializer.endElement();
    }

    protected static class TermParticlePair {
        public Term term;
        public Particle particle;

        TermParticlePair(Term t, Particle p) {
            term = t;
            particle = p;
        }

        @Override
        public boolean equals(Object obj) {
            return obj instanceof TermParticlePair &&
                    term == ((TermParticlePair) obj).term &&
                    particle == ((TermParticlePair) obj).particle;
        }

        @Override
        public int hashCode() {
            return term.hashCode() ^ particle.hashCode();
        }
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
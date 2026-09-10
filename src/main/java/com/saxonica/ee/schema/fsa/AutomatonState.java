package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.om.CodedName;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.jiter.ConcatenatingIterable;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.z.IntHashMap;

import java.util.*;

/**
 * Defines a state of a finite state machine used when validating an instance against
 * a complex type. Each State knows about the possible
 * transitions to other states, and knows whether it is a final state. The initial
 * state is referenced from the FiniteStateMachine object, which is itself a property
 * of the UserComplexType against which validation is being performed.
 * <p>This class is designed for internal use only.</p>
 */

public abstract class AutomatonState implements State {

    int stateNumber;

    private Iterable<Edge> specificTransitions = null;

    protected Edge[] wildcardEdges;
    // It's unusual to have more than one wildcard, so we make this array the exact size required

    private Edge maxTransition;

    private boolean _isFinalState = false;

    // A state can hold a counter; logically this represents a collection of N different states, one for each
    // possible value of the counter. The counter is not part of this object, because the FSM is immutable. But
    // this object holds the min and max values of the counter. The counter is incremented when a counting Edge
    // is traversed from this state back to this state; it is reset to zero when this state is reached via a
    // non-counting edge. A transition out of this state via a non-counting edge is not valid unless the counter
    // value is between min and max inclusive

    private int min = -1;
    private int max = -1;

    public static final Edge[] EMPTY_EDGE_ARRAY = new Edge[0];

    /**
     * Create a new state
     *
     * @param machine the finite state machine to which this State belongs
     */

    public AutomatonState(FiniteStateMachine machine) {
        machine.allocateStateNumber(this);
    }

    /**
     * Test whether this state is a final state
     *
     * @return true if this is a final state
     */

    @Override
    public boolean isFinalState() {
        return _isFinalState && min < 1;
    }

    /**
     * Test whether this state is a final state assuming the counter has reached its minimum value
     *
     * @return true if this state is a final state when the minimum number of occurrences is reached
     */

    public boolean isConditionallyFinalState() {
        return _isFinalState;
    }

    /**
     * Set this state to be (or not to be) a final state
     *
     * @param finalState true if this state is final
     */

    public void setFinalState(boolean finalState) {
        this._isFinalState = finalState;
    }

    /**
     * Get the state number
     *
     * @return the state number
     */

    public int getStateNumber() {
        return stateNumber;
    }

    /**
     * Add a specific transition from this state to another state. This
     * transition accepts a specific element name only.
     *
     * @param edge     the transition to be added
     * @param compiler the schema compiler
     * @throws SchemaException if there are two transitions for the same element
     *                         declaration, that is, if the content model is ambiguous
     */

    public void addSpecificTransition(Edge edge, SchemaCompiler compiler) throws SchemaException {
//        System.err.println("State " + this + " - Add transition from " + this +
//                           " via " + ((ElementDecl)edge.getTerm()).getName() + " to " + edge.getTargetState());

        ElementDecl decl = (ElementDecl) edge.getTerm();
        int key = decl.getFingerprint();

        Edge existing = getSpecificTransition(key);
        if (existing != null) {
            if (existing.getTerm() == decl) {
                if (existing.getParticle() != null && edge.getParticle() != null && existing.getParticle() != edge.getParticle()) {
                    String message = "Ambiguous content model: element particle " +
                            Err.wrap(decl.getDisplayName(), Err.ELEMENT) +
                            " appears in more than one place in the content model";
                    String one = termParticleMessage(decl, existing.getParticle());
                    String two = termParticleMessage(decl, edge.getParticle());
                    if (!one.equals(two)) {
                        message += ", once " + one + ", and once " + two;
                    }
                    throw new SchemaException(message);
                }
            } else {
                throw new SchemaException("Ambiguous content model, element " +
                        Err.wrap(decl.getDisplayName(), Err.ELEMENT) +
                        " appears more than once");
            }
        }

        if (wildcardEdges != null && compiler.getLanguageVersion() == Configuration.XSD10) {
            NamespaceUri uri = decl.getTargetNamespace();
            for (Edge wedge : wildcardEdges) {
                if (((Wildcard) wedge.getTerm()).matches(uri)) {
                    throw new SchemaException("Ambiguous content model, element " +
                            Err.wrap(decl.getDisplayName(), Err.ELEMENT) +
                            " appears in its own right, and also matches an <xs:any> wildcard");
                }
            }
        }


        if (specificTransitions == null) {
            specificTransitions = edge;
        } else if (specificTransitions instanceof Edge) {
            List<Edge> list = new ArrayList<>(4);
            list.add((Edge) specificTransitions);
            list.add(edge);
            specificTransitions = list;
        } else  {
            List<Edge> list = (List<Edge>) specificTransitions;
            list.add(edge);
        }

    }

    private String termParticleMessage(ElementDecl term, Particle particle) {
        try {
            if (((ElementDecl) particle.getTerm()).getFingerprint() == term.getFingerprint()) {
                return "in its own right";
            } else {
                return "as a member of the substitution group of element " +
                        Err.wrap(((ElementDecl) particle.getTerm()).getDisplayName(), Err.ELEMENT);
            }
        } catch (MissingComponentException e) {
            return "indeterminably (" + e.getMessage() + ")";
        }
    }

    /**
     * Add a wildcard transition from this state to another state.
     *
     * @param compiler   the schema compiler
     * @param transition the edge representing the transition
     * @throws net.sf.saxon.type.SchemaException
     *          if the transition cannot be added because
     *          it would generate an inconsistency
     */

    public void addWildcardTransition(SchemaCompiler compiler, Edge transition) throws SchemaException {
//        System.err.println("State " + this + " - Add wildcard transition " + transition);
        if (wildcardEdges == null) {
            wildcardEdges = EMPTY_EDGE_ARRAY;
        } else {
            Wildcard thiscard = (Wildcard) transition.getTerm();
            for (Edge other : wildcardEdges) {
                if (thiscard.overlaps(((Wildcard) other.getTerm()))) {
                    throw new SchemaException(
                            "Ambiguous content model, there are two <xs:any> wildcards " +
                                    "that match overlapping sets of namespaces");

                }
            }
        }

        Wildcard transitionWildcard = (Wildcard) transition.getTerm();
        if (specificTransitions != null && compiler.getLanguageVersion() == Configuration.XSD10) {
            for (Edge other : iterateSpecificTransitions()) {
                if (transitionWildcard.matches(((ElementDecl) other.getTerm()).getTargetNamespace())) {
                    throw new SchemaException(
                            "Ambiguous content model, element " +
                                    Err.wrap(((ElementDecl) other.getTerm()).getDisplayName(), Err.ELEMENT) +
                                    " appears in its own right, and also matches an <xs:any> wildcard");

                }
            }
        }

        // This code assumes that having more than one wildcard is rather unusual
        wildcardEdges = Arrays.copyOf(wildcardEdges, wildcardEdges.length + 1);
        wildcardEdges[wildcardEdges.length - 1] = transition;
    }

    /**
     * Set the counter limits
     *
     * @param min the minimum number of occurrences of counting edges before a non-counting edge is used
     * @param max the maximum number of occurrences of counting edges before a non-counting edge is used;
     *            the value -1 means unbounded
     */

    public void setLimits(int min, int max) {
        this.min = min;
        this.max = max;
    }

    /**
     * Get the minOccurs limit
     *
     * @return the minOccurs value. This will be -1 for a non-counting state
     */

    int getMinOccurs() {
        return min;
    }

    /**
     * Get the maxOccurs limit
     *
     * @return the maxOccurs value. For a counting state, -1 indicates unbounded
     */

    int getMaxOccurs() {
        return max;
    }

    /**
     * Ask whether this is a counting state
     *
     * @return true if this state requires a counter to be maintained
     */

    public boolean requiresCounter() {
        return min != -1;
    }

    /**
     * Find the edge representing the transition from this state to another state, that is
     * triggered by a given input token
     *
     * @param token  the input token, specifically the fingerprint of an element name
     * @param type   the complex type we are validating against
     * @param schema
     * @return the Edge representing the selected transition of the finite state machine,
     * or null if this token is not accepted when in this State (which implies a validation error).
     */

    @Override
    public Edge getTransition(int token, UserComplexType type, PreparedSchema schema) {
        // assert determinized;

        // First see if there is a specific transition for this token
        Edge sedge = getSpecificTransition(token);
        if (sedge != null) {
            return sedge;
        }

        // If not, see if there is a wildcard that matches this token
        Configuration config = type.getConfiguration();
        if (wildcardEdges != null && wildcardEdges.length > 0) {
            NodeName name = new CodedName(token, "", config.getNamePool());
            for (final Edge wedge : wildcardEdges) {
                if (((Wildcard) wedge.getTerm()).matches(name, true, schema, type)) {
                    return wedge;
                }
            }
        }
        // Otherwise, we're stuck
        return null;
    }

    /**
     * Get a specific transition if there is one
     *
     * @param token the input token, specifically the fingerprint of an element name
     * @return the Edge representing the selected transition of the finite state machine,
     *         or null if this token is not accepted when in this State (which implies a validation error).
     */

    private Edge getSpecificTransition(int token) {
        if (specificTransitions != null) {
            for (Edge edge : specificTransitions) {
                if (((ElementDecl) edge.getTerm()).getFingerprint() == token) {
                    return edge;
                }
            }
        }
        return null;
    }

    /**
     * Get the edge from this state for a given term if there is one
     *
     * @param t the term in question: an Element declaration or wildcard
     * @return the selected Edge, or null if there is no Edge for this term
     */

    Edge getEdge(Term t) {
        if (t instanceof ElementDecl && specificTransitions != null) {
            Edge e = getSpecificTransition(((ElementDecl) t).getFingerprint());
            if (e != null && e.getTerm() == t) {
                return e;
            } else {
                return null;
            }
        }
        if (t instanceof Wildcard && wildcardEdges != null) {
            // We only match wildcards by identity
            for (Edge wildcardEdge : wildcardEdges) {
                if (wildcardEdge.getTerm() == t) {
                    return wildcardEdge;
                }
            }
        }
        return null;
    }

    /**
     * Add an edge (either a specific transition or a wildcard transition)
     *
     * @param compiler the schema compiler
     * @param t        the term (an element declaration or wildcard)
     * @param p        the particle to which the element causing this transition is to be attributed for UPA purposes
     * @param s        the target state reached when this term appears in the input
     * @throws net.sf.saxon.type.SchemaException
     *          if the edge is inconsistent with existing edges
     */

    void addEdge(SchemaCompiler compiler, Term t, Particle p, AutomatonState s) throws SchemaException {
        Edge e = new Edge();
        e.setTerm(t);
        e.setParticle(p);
        e.setTargetState(s);
        if (t instanceof ElementDecl) {
            addSpecificTransition(e, compiler);
        } else if (t instanceof Wildcard) {
            addWildcardTransition(compiler, e);
        } else if (t instanceof Edge.MaxOccursTerm) {
            setMaxTransition(e);
        } else {
            throw new IllegalArgumentException("Term must be an ElementDecl or Wildcard");
        }
    }

    /**
     * List the allowed elements in this state as a string, for use in error messages
     *
     * @return the list of permitted elements as a string containing comma-separated element names
     */

    @Override
    public String listAllowedElements() {
        List<String> allowed = new ArrayList<>(10);
        getAllowedElements(allowed);
        if (allowed.isEmpty()) {
            return "No further elements are allowed at this position";
        } else {
            StringBuilder sb = new StringBuilder(allowed.size() * 30);
            sb.append("Expected");
            if (allowed.size() > 1) {
                sb.append(" one of");
            }
            sb.append(": ");
            Iterator<String> iter = allowed.iterator();
            while (iter.hasNext()) {
                String s = iter.next();
                sb.append(s);
                if (iter.hasNext()) {
                    sb.append(", ");
                }
            }
            return sb.toString();
        }
    }

    /**
     * Get the elements that are allowed to appear in this state. This information
     * is used in error messages.
     *
     * @param list a list which will be updated to contain names of elements in displayable form, or symbols
     *             denoting sets of elements, e.g. ##other. The caller should supply an empty list.
     */

    private void getAllowedElements(List<String> list) {
        if (specificTransitions != null) {
            for (Edge edge : iterateSpecificTransitions()) {
                ElementDecl decl = (ElementDecl) edge.getTerm();
                String name = decl.getComponentName().getClarkName();
                list.add(name);
            }
        }
        if (wildcardEdges != null) {
            for (Edge wildcardEdge : wildcardEdges) {
                Wildcard w = (Wildcard) (wildcardEdge).getTerm();
                list.add(w.toString());
            }
        }

    }

    /**
     * Get an iterator over the Edge objects representing the specific element transitions from this state
     *
     * @return an iterator whose returned items are Edge objects
     */

    private Iterable<Edge> iterateSpecificTransitions() {
        if (specificTransitions == null) {
            return Collections.emptyList();
        } else if (specificTransitions instanceof Edge) {
            return specificTransitions;
        } else if (specificTransitionsIsList()) {
            //noinspection unchecked
            return (List<Edge>) specificTransitions;
        } else {
            //noinspection unchecked
            return ((IntHashMap<Edge>) specificTransitions).valueSet();
        }
    }

    @CSharpReplaceBody(code="return specificTransitions is List<Edge>;")
    private boolean specificTransitionsIsList() {
        return specificTransitions instanceof List;
    }

    /**
     * Get an iterator over the wildcard transitions (edges) allowed from this state.
     *
     * @return an Iterator over the edges that represent element wildcards. The objects
     *         returned by this Iterator are {@link Edge} objects.
     */

    @Override
    public Edge[] getWildcardEdges() {
        return wildcardEdges == null ?
                EMPTY_EDGE_ARRAY :
                wildcardEdges;
    }

    /**
     * Get an iterator over all the transitions (edges) allowed from this state
     *
     * @return an Iterator over all the edges from this state. The objects
     *         returned by this Iterator are {@link Edge} objects.
     */

    /*@NotNull*/
    @Override
    public Iterable<Edge> getEdges() {
        if (specificTransitions == null) {
            if (wildcardEdges == null) {
                return Collections.emptyList();
            } else {
                return Arrays.asList(wildcardEdges);
            }
        } else {
            if (wildcardEdges == null) {
                return iterateSpecificTransitions();
            } else {
                return new ConcatenatingIterable<>(
                        iterateSpecificTransitions(),
                        Arrays.asList(wildcardEdges));
            }
        }
    }

    /**
     * Display the finite state machine reachable from this state. This is a diagnostic method for
     * internal use. Output is written to System.err. At present, wildcard transitions are not shown.
     *
     * @param map a Hashmap mapping states already processed to the integer values used to represent that state.
     *            The method should be called initially with an empty map.
     */

    public void display(HashMap<AutomatonState, Integer> map) {
        if (map.isEmpty()) {
            System.err.println("====== FINITE STATE MACHINE ======");
        }
        int key = map.size();
        map.put(this, key);

        for (Edge edge : getEdges()) {
            String symbol;
            if (edge.getTerm() instanceof ElementDecl) {
                ElementDecl decl = (ElementDecl) edge.getTerm();
                symbol = decl.getDisplayName();
            } else {
                Wildcard card = (Wildcard) edge.getTerm();
                symbol = card.toString();
            }
            AutomatonState target = edge.getTargetState();
            int endKey = map.getOrDefault(target, Integer.MIN_VALUE);
            if (endKey == Integer.MIN_VALUE) {
                endKey = map.size();
                System.err.println(key + " " + symbol +
                        " -> " + endKey + finalMarker(target));
                target.display(map);
            } else {
                System.err.println(key + " " + symbol +
                        " -> " + endKey + finalMarker(target));
            }
        }

    }

    private static String finalMarker(AutomatonState state) {
        String finalFlag = "";
        if (state.isFinalState()) {
            finalFlag = "(F)";
        } else if (state.isConditionallyFinalState()) {
            finalFlag = "(F?)";
        }
        return finalFlag;
    }

    /**
     * Display the finite state machine reachable from this state. This is a diagnostic method for
     * internal use. Output is written to System.err. At present, wildcard transitions are not shown.
     *
     * @param err the output destination destination
     */

    public void displayState(Logger err) {
        err.info("State " + stateNumber + finalMarker(this));
        displayLambdaTransitions();

        for (Edge edge : getEdges()) {
            String symbol;
            if (edge.getTerm() instanceof ElementDecl) {
                ElementDecl decl = (ElementDecl) edge.getTerm();
                symbol = decl.getDisplayName();
            } else {
                Wildcard card = (Wildcard) edge.getTerm();
                symbol = card.toString();
            }
            AutomatonState target = edge.getTargetState();
            String counting = requiresCounter() && (target == this)
                    ? " (min=" + getMinOccurs() + ", max=" + getMaxOccurs() + ")"
                    : "";
            err.info("    " + symbol + counting +
                    " -> " + target.stateNumber + finalMarker(target));
        }
        if (maxTransition != null) {
            AutomatonState target = maxTransition.getTargetState();
            err.info("    onMax -> " + target.getStateNumber() + finalMarker(target));
        }

    }

    /**
     * Display the lambda transitions available from this state
     */

    public void displayLambdaTransitions() {
        // do nothing (overridden in subclass)
    }

    /**
     * A counting state may have a special Edge that is used when the maxOccurs value is reached.
     * This method sets this edge.
     *
     * @param edge the transition to be used when the maxOccurs value is reached
     */

    public void setMaxTransition(Edge edge) {
        maxTransition = edge;
    }

    /**
     * Get the transition to be used when the maxOccurs value is reached for a counting state.
     *
     * @return the max transition if there is one, else null.
     */

    public Edge getMaxTransition() {
        return maxTransition;
    }


    /**
     * Serialize this state as part of the serialization of a schema component model
     *
     * @param serializer used to output the model
     * @throws XPathException if any failure occurs
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("state");
        serializer.emitAttribute("nr", stateNumber + "");
        if (_isFinalState) {
            serializer.emitAttribute("final", "true");
        }
        if (requiresCounter()) {
            serializer.emitAttribute("minOccurs", "" + getMinOccurs());
            serializer.emitAttribute("maxOccurs", getMaxOccurs() == -1 ? "unbounded" : "" + getMaxOccurs());
            if (maxTransition != null) {
                serializer.emitAttribute("afterMax", maxTransition.getTargetState().getStateNumber() + "");
            }
        }
        for (Edge edge : iterateSpecificTransitions()) {
            edge.serialize(serializer);
        }
        if (wildcardEdges != null) {
            for (Edge edge : wildcardEdges) {
                edge.serialize(serializer);
            }
        }

        serializer.endElement();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited See Terms and Conditions.
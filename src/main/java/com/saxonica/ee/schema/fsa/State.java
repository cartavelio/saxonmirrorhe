package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.UserComplexType;

/**
 * A State is a dynamic state of the finite state machine. It may either be a static (non-counting) AutomatonState,
 * or a dynamically-created CountingState, which represents the combination of an AutomatonState and a counter value
 */
public interface State {

    /**
     * Find the edge representing the transition from this state to another state, that is
     * triggered by a given input token
     *
     * @param token the input token, specifically the fingerprint of an element name
     * @param type  the complex type whose content model we are validating against
     * @return the Edge representing the selected transition of the finite state machine,
     *         or null if this token is not accepted when in this State (which implies a validation error).
     */

    Edge getTransition(int token, UserComplexType type);

    /**
     * Ask whether this state is a valid final state, that is, no further transitions are needed
     * to complete a successful path through the finite state machine
     * @return true if this is a valid final state
     */

    boolean isFinalState();

    /**
     * Construct a human-readable message explaining what elements are permitted
     * as transitions from this state
     * @return A message indicating what elements may come next
     */

    CharSequence listAllowedElements();

    /**
     * Get an array of the wildcard transitions (edges) allowed from this state.
     *
     * @return an array containing all the edges that represent element wildcards.
     */

    Edge[] getWildcardEdges();

    /**
     * Get an iterator over all the transitions (edges) allowed from this state
     *
     * @return an Iterator over all the edges from this state. The objects
     *         returned by this Iterator are {@link Edge} objects.
     */

    /*@NotNull*/
    Iterable<Edge> getEdges();
}

// Copyright (c) 2008-2023 Saxonica Limited


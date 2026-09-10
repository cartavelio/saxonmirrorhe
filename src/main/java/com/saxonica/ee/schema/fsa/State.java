package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.UserComplexType;

import java.io.Serializable;
import java.util.Iterator;

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

    public Edge getTransition(int token, UserComplexType type);

    boolean isFinalState();

    CharSequence listAllowedElements();

    /**
     * Get an array of the wildcard transitions (edges) allowed from this state.
     *
     * @return an array containing all the edges that represent element wildcards.
     */

    public Edge[] getWildcardEdges();

    /**
     * Get an iterator over all the transitions (edges) allowed from this state
     *
     * @return an Iterator over all the edges from this state. The objects
     *         returned by this Iterator are {@link Edge} objects.
     */

    /*@NotNull*/
    public Iterator<Edge> getEdges();
}

// Copyright (c) 2008 Saxonica Limited.


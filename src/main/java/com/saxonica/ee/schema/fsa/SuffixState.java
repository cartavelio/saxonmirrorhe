package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.UserComplexType;

import java.util.Collections;

/**
 * The state of a finite state machine that is processing "suffix" open content. In this state
 * no terms from the original content model are accepted.
 */
public class SuffixState implements State {

    private SuffixState() {
    }

    public static SuffixState getInstance() {
        return THE_INSTANCE;
    }

    private static final SuffixState THE_INSTANCE = new SuffixState();

    /**
     * Find the edge representing the transition from this state to another state, that is
     * triggered by a given input token
     *
     * @param token the input token, specifically the fingerprint of an element name
     * @param type  the complex type we are validating against
     * @return the Edge representing the selected transition of the finite state machine,
     *         or null if this token is not accepted when in this State (which implies a validation error).
     */

    /*@Nullable*/
    @Override
    public Edge getTransition(int token, UserComplexType type) {
        return null;
    }

    @Override
    public boolean isFinalState() {
        return true;
    }

    @Override
    public CharSequence listAllowedElements() {
        return "";
    }

    /**
     * Get all the wildcard transitions (edges) allowed from this state.
     *
     * @return an array of the edges that represent element wildcards. The objects
     *         returned by this Iterator are {@link com.saxonica.ee.schema.fsa.Edge} objects.
     */

    @Override
    public Edge[] getWildcardEdges() {
        return new Edge[0];
    }

    /**
     * Get an iterator over all the transitions (edges) allowed from this state
     *
     * @return an Iterator over all the edges from this state. The objects
     *         returned by this Iterator are {@link com.saxonica.ee.schema.fsa.Edge} objects.
     */

    /*@NotNull*/
    @Override
    public Iterable<Edge> getEdges() {
        return Collections.emptyList();
    }
}

// Copyright (c) 2009-2023 Saxonica Limited




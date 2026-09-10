package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.UserComplexType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.type.ValidationFailure;

import java.util.ArrayList;
import java.util.List;

/**
 * A counting state is a dynamic object representing the combination of an AutomatonState and a counter value
 */

public class CountingState implements State {

    private final AutomatonState automatonState;
    private final int counter;

    public CountingState(AutomatonState as, int startFrom) {
        automatonState = as;
        counter = startFrom;
    }

    public State incrementCounter() throws ValidationException {
        int max = automatonState.getMaxOccurs();
        if (max != -1 && (counter + 1 == max)) {
            Edge maxTransition = automatonState.getMaxTransition();
            if (maxTransition != null) {
                // For an example, see test modelGroups mgZ005 
                return maxTransition.getTargetState();
            }
        } else if (max != -1 && (counter == max)) {
            ValidationFailure failure = new ValidationFailure(
                    "Too many occurrences of repeating element: " +
                    "found " + (counter + 1) + " occurrences but maxOccurs is " + max);
            failure.setConstraintReference(1, "cvc-particle", "2.2");
            throw failure.makeException();
        }
        return new CountingState(automatonState, counter + 1);
    }

    public void testMinimumReached() throws ValidationException {
        int min = automatonState.getMinOccurs();
        if (counter < min) {
            ValidationFailure failure = new ValidationFailure("Too few occurrences of repeating element: " +
                    "found " + counter + " occurrences but minOccurs is " + min);
            failure.setConstraintReference(1, "cvc-particle", "2.1");
            throw failure.makeException();
        }
    }


    /*@Nullable*/
    @Override
    public Edge getTransition(int token, UserComplexType type) {
        return automatonState.getTransition(token, type);
    }


    @Override
    public boolean isFinalState() {
        return automatonState.isConditionallyFinalState() && counter >= automatonState.getMinOccurs();
    }

    public AutomatonState getUnderlyingState() {
        return automatonState;
    }

    @Override
    public CharSequence listAllowedElements() {
        return automatonState.listAllowedElements();
    }

    /**
     * Get an iterator over the wildcard transitions (edges) allowed from this state.
     *
     * @return an Iterator over the edges that represent element wildcards. The objects
     *         returned by this Iterator are {@link Edge} objects.
     */

    @Override
    public Edge[] getWildcardEdges() {
        Edge[] baseEdges = automatonState.getWildcardEdges();
        if (counter < automatonState.getMinOccurs()) {
            // if we haven't had the minimum number of occurrences of the repeating term,
            // disallow any edge that doesn't lead back to the same state
            int keep = 0;
            for (Edge baseEdge1 : baseEdges) {
                if (baseEdge1.getTargetState() == automatonState) {
                    keep++;
                }
            }
            if (keep == baseEdges.length) {
                return baseEdges;
            } else {
                Edge[] newEdges = new Edge[keep];
                int j = 0;
                for (Edge baseEdge : baseEdges) {
                    if (baseEdge.getTargetState() == automatonState) {
                        newEdges[j++] = baseEdge;
                    }
                }
                return newEdges;
            }
        }
        if (automatonState.getMaxOccurs() != -1 && counter >= automatonState.getMaxOccurs()) {
            // if we've had the maximum number of occurrences of the repeating term,
            // exclude the repeating term from the list
            int keep = 0;
            for (Edge baseEdge : baseEdges) {
                if (baseEdge.getTargetState() != automatonState) {
                    keep++;
                }
            }
            if (keep == baseEdges.length) {
                return baseEdges;
            } else {
                Edge[] newEdges = new Edge[keep];
                int j = 0;
                for (Edge baseEdge : baseEdges) {
                    if (baseEdge.getTargetState() != automatonState) {
                        newEdges[j++] = baseEdge;
                    }
                }
                return newEdges;
            }
        }
        return baseEdges;
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
        Iterable<Edge> baseEdges = automatonState.getEdges();
        List<Edge> newEdges = new ArrayList<>();
        for (Edge baseEdge : baseEdges) {
            if (baseEdge.getTargetState() == automatonState) {
                // this is a counting edge, returning to the current state with an incremented counter.
                // include it only if there is a maxOccurs and the counter has not yet reached maxOccurs.
                // If maxOccurs is unbounded, we don't include this edge because it's not needed for subsumption
                // checking. For performance reasons, treat any maxOccurs greater than 1000 as unbounded.
                if (automatonState.getMaxOccurs() != -1 &&
                        automatonState.getMaxOccurs() < 1000 && counter < automatonState.getMaxOccurs()) {
                    newEdges.add(baseEdge);
                }
            } else {
                // this transition moves to a new state. Include it only if the counter has reached minOccurs
                if (counter >= automatonState.getMinOccurs()) {
                    newEdges.add(baseEdge);
                }
            }
        }
        return newEdges;
    }

    public boolean equals(Object obj) {
        return obj instanceof CountingState &&
                ((CountingState) obj).automatonState == automatonState &&
                ((CountingState) obj).counter == counter;
    }


    @Override
    public int hashCode() {
        return automatonState.hashCode() ^ counter;
    }
}

// Copyright (c) 2008-2022 Saxonica Limited



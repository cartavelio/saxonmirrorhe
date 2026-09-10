package com.saxonica.ee.schema.fsa;



import java.util.ArrayList;
import java.util.List;

/**
 * A state in the non-determinized finite state machine
 */
public class NonDeterminizedState extends AutomatonState {


    private transient List<AutomatonState> lambdaTransitions = null;
    // There will be no lambda transitions once the FSM is determinized

    /**
     * Create a new state in a non-determinized finite state machine
     *
     * @param machine the finite state machine to which this state belongs
     */

    public NonDeterminizedState(FiniteStateMachine machine) {
        super(machine);
    }

    /**
     * Add a lambda transition from this state to another state. This
     * is a transition that can take place when no symbol is read.
     *
     * @param newState the state that results from this event
     */

    public void addLambdaTransition(AutomatonState newState) {
        // If there is a lambda transition to a final state, then this
        // state is also final
        if (newState.isFinalState()) {
            setFinalState(true);
        }
        if (lambdaTransitions == null) {
            lambdaTransitions = new ArrayList<AutomatonState>(3);
        }
        if (!lambdaTransitions.contains(newState)) {
            lambdaTransitions.add(newState);
        }
    }

    /**
     * Get the lambda transitions from this state
     *
     * @return the set of lambda transitions; a null return value is equivalent to an empty list
     */

    /*@Nullable*/
    public List<AutomatonState> getLambdaTransitions() {
        return lambdaTransitions;
    }

    @Override
    public void displayLambdaTransitions() {
        if (lambdaTransitions != null) {
            StringBuilder sb = new StringBuilder(64);
            sb.append("   lambda transitions -> ");
            for (int i = 0; i < lambdaTransitions.size(); i++) {
                sb.append(lambdaTransitions.get(i).stateNumber +
                        (i < lambdaTransitions.size() - 1 ? ", " : ""));
            }
            System.err.println(sb);
        }
    }

}

// Copyright (c) 2006-2023 Saxonica Limited


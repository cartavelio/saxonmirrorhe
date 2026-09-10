package com.saxonica.ee.schema.fsa;

import com.saxonica.ee.schema.Term;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntSet;

import java.util.HashSet;
import java.util.Set;

/**
 * A state in a determinized finite state machine
 */
public class DeterminizedState extends AutomatonState {

    private transient IntSet provenance = null;
    // in a DFSA, provenance represents the set of states in the corresponding NFA

    /**
     * Create a new state
     *
     * @param machine the finite state machine to which this state belongs
     */

    public DeterminizedState(FiniteStateMachine machine) {
        super(machine);
    }

    /**
     * Set the provenance of a state in a determinized finite state machine. This is the set of
     * corresponding states in the non-determinized FSA
     *
     * @param provenance the set of states in the non-deterministic FSA to which this state in the
     *                   deterministic FSA corresponds. May be set to null when the information is no longer needed
     *                   (specifically, once construction is complete).
     */

    public void setProvenance(/*@Nullable*/ IntSet provenance) {
        this.provenance = provenance;
    }

    /**
     * Get the provenance of a state in a determinized finite state machine. This is the set of
     * corresponding states in the non-determinized FSA
     *
     * @return the set of states in the non-deterministic FSA to which this state in the
     *         may be null is ininitially null indicating unknown or not relevant.
     */

    /*@Nullable*/
    public IntSet getProvenance() {
        return provenance;
    }

    /**
     * For a state in the DFSA, get the set of terms for which there are transitions in the
     * corresponding states of the original NFSA
     *
     * @param nfsa a reference to the original non-deterministic finite state machine
     * @return a Set whose members are Term objects (ElementDecl or Wildcard)
     */

    Set<Term> originalTerms(FiniteStateMachine nfsa) {
        Set<Term> terms = new HashSet<>(10);
        for (IntIterator p = provenance.iterator(); p.hasNext(); ) {
            NonDeterminizedState orig = (NonDeterminizedState) nfsa.getState(p.next());
            for (Edge e : orig.getEdges()) {
                terms.add(e.getTerm());
            }
        }
        return terms;
    }


    /**
     * For a state in the DFSA (specifically, for this AutomatonState),
     * get the set of states in the original NFSA that are reachable
     * using a given term
     *
     * @param pair the given term/particle pair
     * @param nfsa the original non-deterministic finite state machine
     * @return a Set whose members are AutomatonState objects in the NFSA
     */

    Set<AutomatonState> reachableStates(FiniteStateMachine.TermParticlePair pair, FiniteStateMachine nfsa) {
        Set<AutomatonState> result = new HashSet<AutomatonState>(5);
        for (IntIterator p = provenance.iterator(); p.hasNext(); ) {
            NonDeterminizedState n = (NonDeterminizedState) nfsa.getState(p.next());
            if (pair.term instanceof Edge.MaxOccursTerm) {
                Edge onmax = n.getMaxTransition();
                if (onmax != null) {
                    result.add(onmax.getTargetState());
                }
            } else {
                Edge e = n.getEdge(pair.term);
                if (e != null && e.getParticle() == pair.particle) {
                    result.add(e.getTargetState());
                }
            }
        }
        return result;
    }

}

// Copyright (c) 2006-2022 Saxonica Limited
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class DeterminizedState : Saxon.Eej.ee.schema.fsa.AutomatonState {
        private Saxon.Hej.z.IntSet provenance = null;
        public DeterminizedState(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) : base (machine) {
        }
        public void setProvenance(Saxon.Hej.z.IntSet provenance) {
            this.provenance = provenance;
        }
        public Saxon.Hej.z.IntSet getProvenance() {
            return provenance;
        }
        internal System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Term> originalTerms(Saxon.Eej.ee.schema.fsa.FiniteStateMachine nfsa) {
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Term> terms = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Term>(10);
            for (Saxon.Hej.z.IntIterator p = provenance.iterator(); p.hasNext(); ) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState orig = (Saxon.Eej.ee.schema.fsa.NonDeterminizedState)(nfsa.getState(p.next()));
                foreach (Saxon.Eej.ee.schema.fsa.Edge e in orig.getEdges()) {
                    terms.Add(e.getTerm());
                }
            }
            return terms;
        }
        internal System.Collections.Generic.ISet<Saxon.Eej.ee.schema.fsa.AutomatonState> reachableStates(Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair pair, Saxon.Eej.ee.schema.fsa.FiniteStateMachine nfsa) {
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.fsa.AutomatonState> result = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.fsa.AutomatonState>(5);
            for (Saxon.Hej.z.IntIterator p = provenance.iterator(); p.hasNext(); ) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState n = (Saxon.Eej.ee.schema.fsa.NonDeterminizedState)(nfsa.getState(p.next()));
                if ((pair.term is Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm)) {
                    Saxon.Eej.ee.schema.fsa.Edge onmax = n.getMaxTransition();
                    if ((onmax != null)) {
                        result.Add(onmax.getTargetState());
                    }
                } else {
                    Saxon.Eej.ee.schema.fsa.Edge e = n.getEdge(pair.term);
                    if (((e != null) && (e.getParticle() == pair.particle))) {
                        result.Add(e.getTargetState());
                    }
                }
            }
            return result;
        }
    
    }
}
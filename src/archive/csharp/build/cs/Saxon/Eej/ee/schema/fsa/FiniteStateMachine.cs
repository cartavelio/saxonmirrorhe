using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class FiniteStateMachine {
        private int statesUsed = 0;
        private Saxon.Eej.ee.schema.fsa.AutomatonState initialState;
        private Saxon.Eej.ee.schema.fsa.AutomatonState[] states = new Saxon.Eej.ee.schema.fsa.AutomatonState[20];
        private readonly System.Collections.Generic.ISet<Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair> alphabet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair>(20);
        private Saxon.Eej.ee.schema.Wildcard openContentWildcard = null;
        private bool openContentInterleaved = false;
        private readonly Saxon.Eej.ee.schema.UserComplexType complexType;
        public FiniteStateMachine(Saxon.Eej.ee.schema.UserComplexType type, bool determinized)  {
            this.complexType = type;
            if (determinized) {
                initialState = new Saxon.Eej.ee.schema.fsa.DeterminizedState(this);
            } else {
                initialState = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(this);
            }
        }
        internal void allocateStateNumber(Saxon.Eej.ee.schema.fsa.AutomatonState state) {
            if ((statesUsed>(states.Length-1))) {
                states = Saxon.Ejava.util.Arrays.copyOf(states, (statesUsed*2));
            }
            states[statesUsed] = state;
            state.stateNumber = statesUsed++;
        }
        public Saxon.Eej.ee.schema.UserComplexType getComplexType() {
            return complexType;
        }
        public int getNumberOfStates() {
            return statesUsed;
        }
        public Saxon.Eej.ee.schema.fsa.AutomatonState getInitialState() {
            return initialState;
        }
        public void setInitialState(Saxon.Eej.ee.schema.fsa.AutomatonState initialState) {
            this.initialState = initialState;
        }
        public Saxon.Eej.ee.schema.fsa.AutomatonState getState(int number) {
            return states[number];
        }
        public void setOpenContentWildcard(Saxon.Eej.ee.schema.Wildcard wildcard, bool interleaved) {
            openContentWildcard = wildcard;
            openContentInterleaved = interleaved;
        }
        public static Saxon.Eej.ee.schema.fsa.NonDeterminizedState compileParticle(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.Particle particle, Saxon.Eej.ee.schema.fsa.NonDeterminizedState endState, Saxon.Eej.ee.schema.UserComplexType subjectType, Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) {
            Saxon.Eej.ee.schema.fsa.NonDeterminizedState n = endState;
            int max = particle.getMaxOccurs();
            int min = particle.getMinOccurs();
            if ((((((particle.getTerm() is Saxon.Eej.ee.schema.ElementDecl) || (particle.getTerm() is Saxon.Eej.ee.schema.Wildcard))) && (((min>1) || (max>1)))) && (((min == max) || !(particle.isVulnerable()))))) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState t = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
                compileCountingTerm(particle, t, min, max, subjectType, compiler, machine);
                if ((min == max)) {
                    Saxon.Eej.ee.schema.fsa.Edge leavingEdge = new Saxon.Eej.ee.schema.fsa.Edge();
                    leavingEdge.setTerm(Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm.getInstance());
                    leavingEdge.setTargetState(n);
                    t.setMaxTransition(leavingEdge);
                    t.setFinalState(n.isFinalState());
                    machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm.getInstance(), Saxon.Eej.ee.schema.fsa.Edge.MAX_OCCURS_PARTICLE));
                } else {
                    t.addLambdaTransition(n);
                }
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState start = compileTerm(compiler, particle, t, subjectType, machine);
                if ((min == 0)) {
                    start.addLambdaTransition(n);
                }
                return start;
            } else {
                int[] limits = compiler.getOccurrenceLimits();
                if ((max>limits[1])) {
                    compiler.warning((("maxOccurs value of "+max)+" treated as unbounded"), Saxon.Hej.trans.SaxonErrorCode.SXSD1012, subjectType);
                    max = Saxon.Eej.ee.schema.Particle.UNBOUNDED;
                }
                if ((min>limits[0])) {
                    compiler.warning(((("minOccurs value of "+min)+" treated as ")+limits[0]), Saxon.Hej.trans.SaxonErrorCode.SXSD1012, subjectType);
                    min = limits[0];
                }
            }
            if ((max == Saxon.Eej.ee.schema.Particle.UNBOUNDED)) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState t = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState b = compileTerm(compiler, particle, t, subjectType, machine);
                b.addLambdaTransition(n);
                t.addLambdaTransition(b);
                n = b;
            } else {
                if ((particle.getTerm() is Saxon.Eej.ee.schema.AllModelGroup)) {
                } else {
                    int limit = (max-min);
                    for (int i = 0; (i<limit); i++) {
                        Saxon.Eej.ee.schema.fsa.NonDeterminizedState b = compileTerm(compiler, particle, n, subjectType, machine);
                        b.addLambdaTransition(endState);
                        n = b;
                    }
                }
            }
            int count = min;
            for (int i = 0; (i<count); i++) {
                n = compileTerm(compiler, particle, n, subjectType, machine);
            }
            return n;
        }
        private static Saxon.Eej.ee.schema.fsa.NonDeterminizedState compileTerm(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.Particle particle, Saxon.Eej.ee.schema.fsa.NonDeterminizedState endState, Saxon.Eej.ee.schema.UserComplexType subjectType, Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) {
            Saxon.Eej.ee.schema.Term term = particle.getTerm();
            if ((term is Saxon.Eej.ee.schema.Wildcard)) {
                machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(term, particle));
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState b = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
                Saxon.Eej.ee.schema.fsa.Edge edge = new Saxon.Eej.ee.schema.fsa.Edge();
                edge.setTerm(term);
                edge.setParticle(particle);
                edge.setTargetState(endState);
                b.addWildcardTransition(compiler, edge);
                return b;
            } else {
                if ((term is Saxon.Eej.ee.schema.ElementDecl)) {
                    machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(term, particle));
                    Saxon.Eej.ee.schema.fsa.NonDeterminizedState b = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
                    Saxon.Eej.ee.schema.fsa.Edge edge = new Saxon.Eej.ee.schema.fsa.Edge();
                    edge.setTerm(term);
                    edge.setParticle(particle);
                    edge.setTargetState(endState);
                    b.addSpecificTransition(edge, compiler);
                    int fp = ((Saxon.Eej.ee.schema.ElementDecl)(term)).getFingerprint();
                    foreach (Saxon.Eej.ee.schema.ElementDecl elem in ((Saxon.Eej.ee.schema.ElementDecl)(term)).getSubstitutionGroupMembers()) {
                        if ((elem.getFingerprint() != fp)) {
                            edge = new Saxon.Eej.ee.schema.fsa.Edge();
                            edge.setTerm(elem);
                            machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(elem, particle));
                            edge.setParticle(particle);
                            edge.setTargetState(endState);
                            b.addSpecificTransition(edge, compiler);
                        }
                        elem.registerComplexTypeUsingThisElement(subjectType);
                    }
                    return b;
                } else {
                    if (((term is Saxon.Eej.ee.schema.ChoiceModelGroup) || (term is Saxon.Eej.ee.schema.AllModelGroup))) {
                        Saxon.Eej.ee.schema.fsa.NonDeterminizedState b = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
                        foreach (Saxon.Eej.ee.schema.Particle p in ((Saxon.Eej.ee.schema.ModelGroup)(term)).getParticles()) {
                            Saxon.Eej.ee.schema.fsa.NonDeterminizedState c = compileParticle(compiler, p, endState, subjectType, machine);
                            b.addLambdaTransition(c);
                        }
                        return b;
                    } else {
                        if (((term is Saxon.Eej.ee.schema.SequenceModelGroup) || (term is Saxon.Eej.ee.schema.ModelGroupDefinition))) {
                            Saxon.Eej.ee.schema.fsa.NonDeterminizedState n = endState;
                            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> reversedParticles = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>(10);
                            foreach (Saxon.Eej.ee.schema.Particle p in ((Saxon.Eej.ee.schema.ModelGroup)(term)).getParticles()) {
                                reversedParticles.Insert(0, p);
                            }
                            foreach (Saxon.Eej.ee.schema.Particle p in reversedParticles) {
                                n = compileParticle(compiler, p, n, subjectType, machine);
                            }
                            return n;
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException(("Unknown type of term "+term.GetType()));
                        }
                    }
                }
            }
        }
        private static void compileCountingTerm(Saxon.Eej.ee.schema.Particle particle, Saxon.Eej.ee.schema.fsa.NonDeterminizedState countingState, int min, int max, Saxon.Eej.ee.schema.UserComplexType subjectType, Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) {
            Saxon.Eej.ee.schema.Term term = particle.getTerm();
            machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(term, particle));
            Saxon.Eej.ee.schema.fsa.Edge edge = new Saxon.Eej.ee.schema.fsa.Edge();
            edge.setTerm(term);
            edge.setParticle(particle);
            edge.setTargetState(countingState);
            countingState.setLimits(min, max);
            if ((term is Saxon.Eej.ee.schema.ElementDecl)) {
                countingState.addSpecificTransition(edge, compiler);
                int fp = ((Saxon.Eej.ee.schema.ElementDecl)(term)).getFingerprint();
                foreach (Saxon.Eej.ee.schema.ElementDecl elem in ((Saxon.Eej.ee.schema.ElementDecl)(term)).getSubstitutionGroupMembers()) {
                    if ((elem.getFingerprint() != fp)) {
                        machine.alphabet.Add(new Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair(elem, particle));
                        edge = new Saxon.Eej.ee.schema.fsa.Edge();
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
        public void display(Saxon.Hej.lib.Logger err) {
            err.info(("Initial state: "+initialState.stateNumber));
            for (int i = (statesUsed-1); (i>=0); i--) {
                states[i].displayState(err);
            }
        }
        public static string subsumesMachine(Saxon.Eej.ee.schema.fsa.FiniteStateMachine @base, Saxon.Eej.ee.schema.fsa.FiniteStateMachine sub, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.State[]> unprocessed = new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.State[]>(30);
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.State[]> processed = new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.State[]>(30);
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.fsa.State> finalDerivedStates = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.fsa.State>(30);
            if ((sub.initialState.isFinalState() && (sub.initialState.getMinOccurs()<=0))) {
                finalDerivedStates.Add(sub.initialState);
                if (!(((@base.initialState.isFinalState() && (sub.initialState.getMinOccurs()<=0))))) {
                    return "The derived type permits empty content, but the base type does not";
                }
            }
            Saxon.Eej.ee.schema.fsa.State[] initialPair = {@base.getInitialState(), sub.getInitialState()};
            unprocessed.Add(initialPair);
            while (!(unprocessed.Count==0)) {
                Saxon.Eej.ee.schema.fsa.State[] p = unprocessed[0];
                Saxon.Eej.ee.schema.fsa.State baseState = p[0];
                Saxon.Eej.ee.schema.fsa.State derivedState = p[1];
                foreach (Saxon.Eej.ee.schema.fsa.Edge derivedEdge in derivedState.getEdges()) {
                    Saxon.Eej.ee.schema.fsa.State newDerivedState;
                    try {
                        newDerivedState = derivedEdge.makeTransition(derivedState);
                    } catch (Saxon.Hej.type.ValidationException e) {
                        throw new Saxon.Ejava.lang.AssertionError("Invalid derived transition");
                    }
                    if (newDerivedState.isFinalState()) {
                        finalDerivedStates.Add(newDerivedState);
                    }
                    Saxon.Eej.ee.schema.fsa.Edge baseEdge = null;
                    if ((derivedEdge.getTerm() is Saxon.Eej.ee.schema.ElementDecl)) {
                        baseEdge = baseState.getTransition(((Saxon.Eej.ee.schema.ElementDecl)(derivedEdge.getTerm())).getFingerprint(), @base.complexType);
                        string s = "No match";
                        if ((baseEdge != null)) {
                            try {
                                s = baseEdge.matches(derivedEdge, compiler);
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                return ("Cannot prove the restriction is valid, because there are missing schema components. "+e.Message);
                            }
                        }
                        if ((s != null)) {
                            switch (s) {
                              case "No match":
                                return (("The restricted type allows element "+((Saxon.Eej.ee.schema.ElementDecl)(derivedEdge.getTerm())).getDisplayName())+" where the base type does not");
                              case "Not final":
                                return (("The restricted type allows the content to finish after element "+((Saxon.Eej.ee.schema.ElementDecl)(derivedEdge.getTerm())).getDisplayName())+" where the base type does not");
                              default:
                                return ((("The definition of element "+((Saxon.Eej.ee.schema.ElementDecl)(derivedEdge.getTerm())).getDisplayName())+" differs between the restricted type and the base type. ")+s);
                            }
                        }
                    } else {
                        Saxon.Eej.ee.schema.fsa.Edge[] baseEdges = baseState.getWildcardEdges();
                        bool foundWildcard = false;
                        string s = null;
                        foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge1 in baseEdges) {
                            baseEdge = baseEdge1;
                            try {
                                s = baseEdge.matches(derivedEdge, compiler);
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                return ("Cannot prove the restriction is valid, because there are missing schema components. "+e.Message);
                            }
                            if ((s == null)) {
                                foundWildcard = true;
                                break ;
                            }
                        }
                        if (!(foundWildcard)) {
                            return ("The restricted type allows xs:any content that is not allowed by the base type. "+s);
                        }
                    }
                    bool foundExistingPair = false;
                    Saxon.Eej.ee.schema.fsa.State[] end;
                    try {
                        end = new Saxon.Eej.ee.schema.fsa.State[]{baseEdge.makeTransition(baseState), newDerivedState};
                    } catch (Saxon.Hej.type.ValidationException e) {
                        return "Occurrence limits on the restricted type exceed those on base type";
                    }
                    foreach (Saxon.Eej.ee.schema.fsa.State[] anUnprocessed in unprocessed) {
                        if ((anUnprocessed[0].Equals(end[0]) && anUnprocessed[1].Equals(end[1]))) {
                            foundExistingPair = true;
                            break ;
                        }
                    }
                    foreach (Saxon.Eej.ee.schema.fsa.State[] aProcessed in processed) {
                        if ((aProcessed[0].Equals(end[0]) && aProcessed[1].Equals(end[1]))) {
                            foundExistingPair = true;
                            break ;
                        }
                    }
                    if (!(foundExistingPair)) {
                        unprocessed.Add(end);
                    }
                }
                Saxon.Impl.Helpers.ListUtils.Remove(unprocessed, 0);
                processed.Add(p);
            }
            foreach (Saxon.Eej.ee.schema.fsa.State e in finalDerivedStates) {
                System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.fsa.State[]> processedPairIterator = processed.GetEnumerator();
                bool unfound = false;
                while (processedPairIterator.MoveNext() /*UNSAFE CONVERSION*/) {
                    Saxon.Eej.ee.schema.fsa.State[] pair = processedPairIterator.Current;
                    if ((pair[1].Equals(e) && !(pair[0].isFinalState()))) {
                        unfound = true;
                        break ;
                    }
                }
                if (unfound) {
                    return "The restricted type allows a final state that is not allowed by the base type";
                }
            }
            return null;
        }
        public static Saxon.Eej.ee.schema.fsa.FiniteStateMachine determinize(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.fsa.FiniteStateMachine nfsa) {
            Saxon.Eej.ee.schema.fsa.FiniteStateMachine dfsa = new Saxon.Eej.ee.schema.fsa.FiniteStateMachine(nfsa.complexType, true);
            dfsa.setOpenContentWildcard(nfsa.openContentWildcard, nfsa.openContentInterleaved);
            System.Collections.Generic.LinkedList<Saxon.Eej.ee.schema.fsa.AutomatonState> unmarkedStatesOfD = new System.Collections.Generic.LinkedList<Saxon.Eej.ee.schema.fsa.AutomatonState>();
            System.Collections.Generic.Dictionary<Saxon.Hej.z.IntSet,Saxon.Eej.ee.schema.fsa.AutomatonState> statesOfD = new System.Collections.Generic.Dictionary<Saxon.Hej.z.IntSet, Saxon.Eej.ee.schema.fsa.AutomatonState>(20);
            Saxon.Eej.ee.schema.fsa.DeterminizedState initialD = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(dfsa.getInitialState());
            initialD.setProvenance(lambdaClosure(new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Eej.ee.schema.fsa.AutomatonState>(nfsa.getInitialState())));
            initialD.setFinalState(containsFinalState(nfsa, initialD.getProvenance()));
            initialD.setLimits(nfsa.getInitialState().getMinOccurs(), nfsa.getInitialState().getMaxOccurs());
            unmarkedStatesOfD.AddLast(initialD);
            Saxon.Impl.Helpers.MapUtils.Put(statesOfD, initialD.getProvenance(), initialD);
            while (!(unmarkedStatesOfD.Count==0)) {
                Saxon.Eej.ee.schema.fsa.DeterminizedState x = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(Saxon.Impl.Helpers.LinkedListUtils.PollFirst(unmarkedStatesOfD));
                foreach (Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair pair in nfsa.alphabet) {
                    Saxon.Eej.ee.schema.Particle particle = pair.particle;
                    Saxon.Eej.ee.schema.Term a = pair.term;
                    System.Collections.Generic.ISet<Saxon.Eej.ee.schema.fsa.AutomatonState> T = x.reachableStates(pair, nfsa);
                    if (!(T.Count==0)) {
                        Saxon.Hej.z.IntSet key = lambdaClosure(T.GetEnumerator());
                        Saxon.Eej.ee.schema.fsa.DeterminizedState dy = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(Saxon.Impl.Helpers.MapUtils.Get(statesOfD, key));
                        if ((dy == null)) {
                            dy = new Saxon.Eej.ee.schema.fsa.DeterminizedState(dfsa);
                            dy.setProvenance(key);
                            dy.setFinalState(containsFinalState(nfsa, key));
                            int[] limits = getOccurrenceLimits(nfsa, key);
                            dy.setLimits(limits[0], limits[1]);
                            unmarkedStatesOfD.AddLast(dy);
                            Saxon.Impl.Helpers.MapUtils.Put(statesOfD, key, dy);
                        }
                        x.addEdge(compiler, a, particle, dy);
                    }
                }
            }
            for (int i = (dfsa.getNumberOfStates()-1); (i>=0); i--) {
                ((Saxon.Eej.ee.schema.fsa.DeterminizedState)(dfsa.getState(i))).setProvenance(null);
            }
            return dfsa;
        }
        private static bool containsFinalState(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine, Saxon.Hej.z.IntSet states) {
            for (Saxon.Hej.z.IntIterator i = states.iterator(); i.hasNext(); ) {
                int state = i.next();
                if (machine.getState(state).isConditionallyFinalState()) {
                    return true;
                }
            }
            return false;
        }
        private static int[] getOccurrenceLimits(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine, Saxon.Hej.z.IntSet states) {
            for (Saxon.Hej.z.IntIterator i = states.iterator(); i.hasNext(); ) {
                int state = i.next();
                Saxon.Eej.ee.schema.fsa.AutomatonState aState = machine.getState(state);
                if ((aState.getMinOccurs() != -1)) {
                    return new int[]{aState.getMinOccurs(), aState.getMaxOccurs()};
                }
            }
            return new int[]{-1, -1};
        }
        public Saxon.Eej.ee.schema.Wildcard getOpenContentWildcard() {
            return openContentWildcard;
        }
        public bool isOpenContentInterleaved() {
            return openContentInterleaved;
        }
        private static Saxon.Hej.z.IntSet lambdaClosure(System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.fsa.AutomatonState> T_iter) {
            System.Collections.Generic.Stack<Saxon.Eej.ee.schema.fsa.AutomatonState> stack = new System.Collections.Generic.Stack<Saxon.Eej.ee.schema.fsa.AutomatonState>();
            Saxon.Hej.z.IntRangeSet result = new Saxon.Hej.z.IntRangeSet();
            while (T_iter.MoveNext() /*UNSAFE CONVERSION*/) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState state = (Saxon.Eej.ee.schema.fsa.NonDeterminizedState)(T_iter.Current);
                result.add(state.stateNumber);
                stack.Push(state);
            }
            while (!((stack.Count==0))) {
                Saxon.Eej.ee.schema.fsa.NonDeterminizedState s = (Saxon.Eej.ee.schema.fsa.NonDeterminizedState)(stack.Pop());
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.AutomatonState> transitions = s.getLambdaTransitions();
                if ((transitions != null)) {
                    foreach (Saxon.Eej.ee.schema.fsa.AutomatonState t in transitions) {
                        if (result.add(t.stateNumber)) {
                            stack.Push(t);
                        }
                    }
                }
            }
            return result;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("finiteStateMachine");
            serializer.emitAttribute("initialState", (initialState.getStateNumber()+""));
            for (int s = 0; (s<statesUsed); s++) {
                states[s].serialize(serializer);
            }
            serializer.endElement();
        }
        protected internal class TermParticlePair {
            public Saxon.Eej.ee.schema.Term term;
            public Saxon.Eej.ee.schema.Particle particle;
            internal TermParticlePair(Saxon.Eej.ee.schema.Term t, Saxon.Eej.ee.schema.Particle p)  {
                term = t;
                particle = p;
            }
            public override bool Equals(System.Object obj) {
                return (((obj is Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair) && (term == ((Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair)(obj)).term)) && (particle == ((Saxon.Eej.ee.schema.fsa.FiniteStateMachine.TermParticlePair)(obj)).particle));
            }
            public override int GetHashCode() {
                return (term.GetHashCode()^particle.GetHashCode());
            }
        
        }
    
    }
}
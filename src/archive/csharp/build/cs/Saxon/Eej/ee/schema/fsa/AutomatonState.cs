using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public abstract class AutomatonState : Saxon.Eej.ee.schema.fsa.State {
        internal int stateNumber;
        private System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> specificTransitions = null;
        protected internal Saxon.Eej.ee.schema.fsa.Edge[] wildcardEdges;
        private Saxon.Eej.ee.schema.fsa.Edge maxTransition;
        private bool _isFinalState = false;
        private int min = -1;
        private int max = -1;
        public static readonly Saxon.Eej.ee.schema.fsa.Edge[] EMPTY_EDGE_ARRAY = new Saxon.Eej.ee.schema.fsa.Edge[0];
        public AutomatonState(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine)  {
            machine.allocateStateNumber(this);
        }
        public bool isFinalState() {
            return (_isFinalState && (min<1));
        }
        public bool isConditionallyFinalState() {
            return _isFinalState;
        }
        public void setFinalState(bool finalState) {
            this._isFinalState = finalState;
        }
        public int getStateNumber() {
            return stateNumber;
        }
        public void addSpecificTransition(Saxon.Eej.ee.schema.fsa.Edge edge, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(edge.getTerm());
            int key = decl.getFingerprint();
            Saxon.Eej.ee.schema.fsa.Edge existing = getSpecificTransition(key);
            if ((existing != null)) {
                if ((existing.getTerm() == decl)) {
                    if ((((existing.getParticle() != null) && (edge.getParticle() != null)) && (existing.getParticle() != edge.getParticle()))) {
                        string message = (("Ambiguous content model: element particle "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in more than one place in the content model");
                        string one = termParticleMessage(decl, existing.getParticle());
                        string two = termParticleMessage(decl, edge.getParticle());
                        if (!(one.Equals(two))) {
                            message += (((", once "+one)+", and once ")+two);
                        }
                        throw new Saxon.Hej.type.SchemaException(message);
                    }
                } else {
                    throw new Saxon.Hej.type.SchemaException((("Ambiguous content model, element "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears more than once"));
                }
            }
            if (((wildcardEdges != null) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                string uri = decl.getTargetNamespace();
                foreach (Saxon.Eej.ee.schema.fsa.Edge wedge in wildcardEdges) {
                    if (((Saxon.Eej.ee.schema.Wildcard)(wedge.getTerm())).matches(uri)) {
                        throw new Saxon.Hej.type.SchemaException((("Ambiguous content model, element "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in its own right, and also matches an <xs:any> wildcard"));
                    }
                }
            }
            if ((specificTransitions == null)) {
                specificTransitions = edge;
            } else {
                if ((specificTransitions is Saxon.Eej.ee.schema.fsa.Edge)) {
                    System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.Edge> list = new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.Edge>(4);
                    list.Add((Saxon.Eej.ee.schema.fsa.Edge)(specificTransitions));
                    list.Add(edge);
                    specificTransitions = list;
                } else {
                    System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.Edge> list = (System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.Edge>)(specificTransitions);
                    list.Add(edge);
                }
            }
        }
        private string termParticleMessage(Saxon.Eej.ee.schema.ElementDecl term, Saxon.Eej.ee.schema.Particle particle) {
            try {
                if ((((Saxon.Eej.ee.schema.ElementDecl)(particle.getTerm())).getFingerprint() == term.getFingerprint())) {
                    return "in its own right";
                } else {
                    return ("as a member of the substitution group of element "+Saxon.Hej.trans.Err.wrap(((Saxon.Eej.ee.schema.ElementDecl)(particle.getTerm())).getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return (("indeterminably ("+e.Message)+")");
            }
        }
        public void addWildcardTransition(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.fsa.Edge transition) {
            if ((wildcardEdges == null)) {
                wildcardEdges = EMPTY_EDGE_ARRAY;
            } else {
                Saxon.Eej.ee.schema.Wildcard thiscard = (Saxon.Eej.ee.schema.Wildcard)(transition.getTerm());
                foreach (Saxon.Eej.ee.schema.fsa.Edge other in wildcardEdges) {
                    if (thiscard.overlaps(((Saxon.Eej.ee.schema.Wildcard)(other.getTerm())))) {
                        throw new Saxon.Hej.type.SchemaException(("Ambiguous content model, there are two <xs:any> wildcards "+"that match overlapping sets of namespaces"));
                    }
                }
            }
            Saxon.Eej.ee.schema.Wildcard transitionWildcard = (Saxon.Eej.ee.schema.Wildcard)(transition.getTerm());
            if (((specificTransitions != null) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge other in iterateSpecificTransitions()) {
                    if (transitionWildcard.matches(((Saxon.Eej.ee.schema.ElementDecl)(other.getTerm())).getTargetNamespace())) {
                        throw new Saxon.Hej.type.SchemaException((("Ambiguous content model, element "+Saxon.Hej.trans.Err.wrap(((Saxon.Eej.ee.schema.ElementDecl)(other.getTerm())).getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in its own right, and also matches an <xs:any> wildcard"));
                    }
                }
            }
            wildcardEdges = Saxon.Ejava.util.Arrays.copyOf(wildcardEdges, (wildcardEdges.Length+1));
            wildcardEdges[(wildcardEdges.Length-1)] = transition;
        }
        public void setLimits(int min, int max) {
            this.min = min;
            this.max = max;
        }
        internal int getMinOccurs() {
            return min;
        }
        internal int getMaxOccurs() {
            return max;
        }
        public bool requiresCounter() {
            return (min != -1);
        }
        public Saxon.Eej.ee.schema.fsa.Edge getTransition(int token, Saxon.Eej.ee.schema.UserComplexType type) {
            Saxon.Eej.ee.schema.fsa.Edge sedge = getSpecificTransition(token);
            if ((sedge != null)) {
                return sedge;
            }
            Saxon.Hej.Configuration config = type.getConfiguration();
            if (((wildcardEdges != null) && (wildcardEdges.Length>0))) {
                Saxon.Hej.om.NodeName name = new Saxon.Hej.om.CodedName(token, "", config.getNamePool());
                foreach (Saxon.Eej.ee.schema.fsa.Edge wedge in wildcardEdges) {
                    if (((Saxon.Eej.ee.schema.Wildcard)(wedge.getTerm())).matches(name, true, config, type)) {
                        return wedge;
                    }
                }
            }
            return null;
        }
        private Saxon.Eej.ee.schema.fsa.Edge getSpecificTransition(int token) {
            if ((specificTransitions != null)) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge edge in specificTransitions) {
                    if ((((Saxon.Eej.ee.schema.ElementDecl)(edge.getTerm())).getFingerprint() == token)) {
                        return edge;
                    }
                }
            }
            return null;
        }
        internal Saxon.Eej.ee.schema.fsa.Edge getEdge(Saxon.Eej.ee.schema.Term t) {
            if (((t is Saxon.Eej.ee.schema.ElementDecl) && (specificTransitions != null))) {
                Saxon.Eej.ee.schema.fsa.Edge e = getSpecificTransition(((Saxon.Eej.ee.schema.ElementDecl)(t)).getFingerprint());
                if (((e != null) && (e.getTerm() == t))) {
                    return e;
                } else {
                    return null;
                }
            }
            if (((t is Saxon.Eej.ee.schema.Wildcard) && (wildcardEdges != null))) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge wildcardEdge in wildcardEdges) {
                    if ((wildcardEdge.getTerm() == t)) {
                        return wildcardEdge;
                    }
                }
            }
            return null;
        }
        internal void addEdge(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Eej.ee.schema.Term t, Saxon.Eej.ee.schema.Particle p, Saxon.Eej.ee.schema.fsa.AutomatonState s) {
            Saxon.Eej.ee.schema.fsa.Edge e = new Saxon.Eej.ee.schema.fsa.Edge();
            e.setTerm(t);
            e.setParticle(p);
            e.setTargetState(s);
            if ((t is Saxon.Eej.ee.schema.ElementDecl)) {
                addSpecificTransition(e, compiler);
            } else {
                if ((t is Saxon.Eej.ee.schema.Wildcard)) {
                    addWildcardTransition(compiler, e);
                } else {
                    if ((t is Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm)) {
                        setMaxTransition(e);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("Term must be an ElementDecl or Wildcard");
                    }
                }
            }
        }
        public string listAllowedElements() {
            System.Collections.Generic.IList<string> allowed = new System.Collections.Generic.List<string>(10);
            getAllowedElements(allowed);
            if (allowed.Count==0) {
                return "No further elements are allowed at this position";
            } else {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((allowed.Count*30));
                sb.append("Expected");
                if ((allowed.Count>1)) {
                    sb.append(" one of");
                }
                sb.append(": ");
                System.Collections.Generic.IEnumerator<string> iter = allowed.GetEnumerator();
                while (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                    string s = iter.Current;
                    sb.append(s);
                    if (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                        sb.append(", ");
                    }
                }
                return sb.ToString();
            }
        }
        private void getAllowedElements(System.Collections.Generic.IList<string> list) {
            if ((specificTransitions != null)) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge edge in iterateSpecificTransitions()) {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(edge.getTerm());
                    string name = decl.getComponentName().getClarkName();
                    list.Add(name);
                }
            }
            if ((wildcardEdges != null)) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge wildcardEdge in wildcardEdges) {
                    Saxon.Eej.ee.schema.Wildcard w = (Saxon.Eej.ee.schema.Wildcard)((wildcardEdge).getTerm());
                    list.Add(w.ToString());
                }
            }
        }
        private System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> iterateSpecificTransitions() {
            if ((specificTransitions == null)) {
                return new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.Edge>();
            } else {
                if ((specificTransitions is Saxon.Eej.ee.schema.fsa.Edge)) {
                    return specificTransitions;
                } else {
                    if (specificTransitionsIsList()) {
                        return (System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.Edge>)(specificTransitions);
                    } else {
                        return ((Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.fsa.Edge>)(specificTransitions)).valueSet();
                    }
                }
            }
        }
        private bool specificTransitionsIsList() {
            return specificTransitions is List<Edge>;
        }
        public Saxon.Eej.ee.schema.fsa.Edge[] getWildcardEdges() {
            return ((wildcardEdges == null) ? EMPTY_EDGE_ARRAY : wildcardEdges);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> getEdges() {
            if ((specificTransitions == null)) {
                if ((wildcardEdges == null)) {
                    return new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.Edge>();
                } else {
                    return Saxon.Ejava.util.Arrays.asList(wildcardEdges);
                }
            } else {
                if ((wildcardEdges == null)) {
                    return iterateSpecificTransitions();
                } else {
                    return new Saxon.Hej.tree.jiter.ConcatenatingIterable<Saxon.Eej.ee.schema.fsa.Edge>(iterateSpecificTransitions(), Saxon.Ejava.util.Arrays.asList(wildcardEdges));
                }
            }
        }
        public void display(System.Collections.Generic.Dictionary<Saxon.Eej.ee.schema.fsa.AutomatonState,int> map) {
            if (map.Count==0) {
                System.Console.Error.WriteLine("====== FINITE STATE MACHINE ======");
            }
            int key = map.Count;
            Saxon.Impl.Helpers.MapUtils.Put(map, this, key);
            foreach (Saxon.Eej.ee.schema.fsa.Edge edge in getEdges()) {
                string symbol;
                if ((edge.getTerm() is Saxon.Eej.ee.schema.ElementDecl)) {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(edge.getTerm());
                    symbol = decl.getDisplayName();
                } else {
                    Saxon.Eej.ee.schema.Wildcard card = (Saxon.Eej.ee.schema.Wildcard)(edge.getTerm());
                    symbol = card.ToString();
                }
                Saxon.Eej.ee.schema.fsa.AutomatonState target = edge.getTargetState();
                int endKey = Saxon.Impl.Helpers.MapUtils.GetOrDefault(map, target, System.Int32.MinValue);
                if ((endKey == System.Int32.MinValue)) {
                    endKey = map.Count;
                    System.Console.Error.WriteLine((((((key+" ")+symbol)+" -> ")+endKey)+finalMarker(target)));
                    target.display(map);
                } else {
                    System.Console.Error.WriteLine((((((key+" ")+symbol)+" -> ")+endKey)+finalMarker(target)));
                }
            }
        }
        private static string finalMarker(Saxon.Eej.ee.schema.fsa.AutomatonState state) {
            string finalFlag = "";
            if (state.isFinalState()) {
                finalFlag = "(F)";
            } else {
                if (state.isConditionallyFinalState()) {
                    finalFlag = "(F?)";
                }
            }
            return finalFlag;
        }
        public void displayState(Saxon.Hej.lib.Logger err) {
            err.info((("State "+stateNumber)+finalMarker(this)));
            displayLambdaTransitions();
            foreach (Saxon.Eej.ee.schema.fsa.Edge edge in getEdges()) {
                string symbol;
                if ((edge.getTerm() is Saxon.Eej.ee.schema.ElementDecl)) {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(edge.getTerm());
                    symbol = decl.getDisplayName();
                } else {
                    Saxon.Eej.ee.schema.Wildcard card = (Saxon.Eej.ee.schema.Wildcard)(edge.getTerm());
                    symbol = card.ToString();
                }
                Saxon.Eej.ee.schema.fsa.AutomatonState target = edge.getTargetState();
                string counting = ((requiresCounter() && ((target == this))) ? ((((" (min="+getMinOccurs())+", max=")+getMaxOccurs())+")") : "");
                err.info(((((("    "+symbol)+counting)+" -> ")+target.stateNumber)+finalMarker(target)));
            }
            if ((maxTransition != null)) {
                Saxon.Eej.ee.schema.fsa.AutomatonState target = maxTransition.getTargetState();
                err.info((("    onMax -> "+target.getStateNumber())+finalMarker(target)));
            }
        }
        public virtual void displayLambdaTransitions() {
        }
        public void setMaxTransition(Saxon.Eej.ee.schema.fsa.Edge edge) {
            maxTransition = edge;
        }
        public Saxon.Eej.ee.schema.fsa.Edge getMaxTransition() {
            return maxTransition;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("state");
            serializer.emitAttribute("nr", (stateNumber+""));
            if (_isFinalState) {
                serializer.emitAttribute("final", "true");
            }
            if (requiresCounter()) {
                serializer.emitAttribute("minOccurs", (""+getMinOccurs()));
                serializer.emitAttribute("maxOccurs", ((getMaxOccurs() == -1) ? "unbounded" : (""+getMaxOccurs())));
                if ((maxTransition != null)) {
                    serializer.emitAttribute("afterMax", (maxTransition.getTargetState().getStateNumber()+""));
                }
            }
            foreach (Saxon.Eej.ee.schema.fsa.Edge edge in iterateSpecificTransitions()) {
                edge.serialize(serializer);
            }
            if ((wildcardEdges != null)) {
                foreach (Saxon.Eej.ee.schema.fsa.Edge edge in wildcardEdges) {
                    edge.serialize(serializer);
                }
            }
            serializer.endElement();
        }
    
    }
}
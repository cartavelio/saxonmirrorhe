using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class CountingState : Saxon.Eej.ee.schema.fsa.State {
        private readonly Saxon.Eej.ee.schema.fsa.AutomatonState automatonState;
        private readonly int counter;
        public CountingState(Saxon.Eej.ee.schema.fsa.AutomatonState @as, int startFrom)  {
            automatonState = @as;
            counter = startFrom;
        }
        public Saxon.Eej.ee.schema.fsa.State incrementCounter() {
            int max = automatonState.getMaxOccurs();
            if (((max != -1) && (((counter+1) == max)))) {
                Saxon.Eej.ee.schema.fsa.Edge maxTransition = automatonState.getMaxTransition();
                if ((maxTransition != null)) {
                    return maxTransition.getTargetState();
                }
            } else {
                if (((max != -1) && ((counter == max)))) {
                    Saxon.Hej.type.ValidationFailure failure = new Saxon.Hej.type.ValidationFailure((((("Too many occurrences of repeating element: "+"found ")+((counter+1)))+" occurrences but maxOccurs is ")+max));
                    failure.setConstraintReference(1, "cvc-particle", "2.2");
                    throw failure.makeException();
                }
            }
            return new Saxon.Eej.ee.schema.fsa.CountingState(automatonState, (counter+1));
        }
        public void testMinimumReached() {
            int min = automatonState.getMinOccurs();
            if ((counter<min)) {
                Saxon.Hej.type.ValidationFailure failure = new Saxon.Hej.type.ValidationFailure((((("Too few occurrences of repeating element: "+"found ")+counter)+" occurrences but minOccurs is ")+min));
                failure.setConstraintReference(1, "cvc-particle", "2.1");
                throw failure.makeException();
            }
        }
        public Saxon.Eej.ee.schema.fsa.Edge getTransition(int token, Saxon.Eej.ee.schema.UserComplexType type) {
            return automatonState.getTransition(token, type);
        }
        public bool isFinalState() {
            return (automatonState.isConditionallyFinalState() && (counter>=automatonState.getMinOccurs()));
        }
        public Saxon.Eej.ee.schema.fsa.AutomatonState getUnderlyingState() {
            return automatonState;
        }
        public string listAllowedElements() {
            return ((/*1430*/string)automatonState.listAllowedElements());
        }
        public Saxon.Eej.ee.schema.fsa.Edge[] getWildcardEdges() {
            Saxon.Eej.ee.schema.fsa.Edge[] baseEdges = automatonState.getWildcardEdges();
            if ((counter<automatonState.getMinOccurs())) {
                int keep = 0;
                foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge1 in baseEdges) {
                    if ((baseEdge1.getTargetState() == automatonState)) {
                        keep++;
                    }
                }
                if ((keep == baseEdges.Length)) {
                    return baseEdges;
                } else {
                    Saxon.Eej.ee.schema.fsa.Edge[] newEdges = new Saxon.Eej.ee.schema.fsa.Edge[keep];
                    int j = 0;
                    foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge in baseEdges) {
                        if ((baseEdge.getTargetState() == automatonState)) {
                            newEdges[j++] = baseEdge;
                        }
                    }
                    return newEdges;
                }
            }
            if (((automatonState.getMaxOccurs() != -1) && (counter>=automatonState.getMaxOccurs()))) {
                int keep = 0;
                foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge in baseEdges) {
                    if ((baseEdge.getTargetState() != automatonState)) {
                        keep++;
                    }
                }
                if ((keep == baseEdges.Length)) {
                    return baseEdges;
                } else {
                    Saxon.Eej.ee.schema.fsa.Edge[] newEdges = new Saxon.Eej.ee.schema.fsa.Edge[keep];
                    int j = 0;
                    foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge in baseEdges) {
                        if ((baseEdge.getTargetState() != automatonState)) {
                            newEdges[j++] = baseEdge;
                        }
                    }
                    return newEdges;
                }
            }
            return baseEdges;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> getEdges() {
            System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> baseEdges = automatonState.getEdges();
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.Edge> newEdges = new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.Edge>();
            foreach (Saxon.Eej.ee.schema.fsa.Edge baseEdge in baseEdges) {
                if ((baseEdge.getTargetState() == automatonState)) {
                    if ((((automatonState.getMaxOccurs() != -1) && (automatonState.getMaxOccurs()<1000)) && (counter<automatonState.getMaxOccurs()))) {
                        newEdges.Add(baseEdge);
                    }
                } else {
                    if ((counter>=automatonState.getMinOccurs())) {
                        newEdges.Add(baseEdge);
                    }
                }
            }
            return newEdges;
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Eej.ee.schema.fsa.CountingState) && (((Saxon.Eej.ee.schema.fsa.CountingState)(obj)).automatonState == automatonState)) && (((Saxon.Eej.ee.schema.fsa.CountingState)(obj)).counter == counter));
        }
        public override int GetHashCode() {
            return (automatonState.GetHashCode()^counter);
        }
    
    }
}
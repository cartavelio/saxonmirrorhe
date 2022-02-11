using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class NonDeterminizedState : Saxon.Eej.ee.schema.fsa.AutomatonState {
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.AutomatonState> lambdaTransitions = null;
        public NonDeterminizedState(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) : base (machine) {
        }
        public void addLambdaTransition(Saxon.Eej.ee.schema.fsa.AutomatonState newState) {
            if (newState.isFinalState()) {
                setFinalState(true);
            }
            if ((lambdaTransitions == null)) {
                lambdaTransitions = new System.Collections.Generic.List<Saxon.Eej.ee.schema.fsa.AutomatonState>(3);
            }
            if (!(lambdaTransitions.Contains(newState))) {
                lambdaTransitions.Add(newState);
            }
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.fsa.AutomatonState> getLambdaTransitions() {
            return lambdaTransitions;
        }
        public override void displayLambdaTransitions() {
            if ((lambdaTransitions != null)) {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
                sb.append("   lambda transitions -> ");
                for (int i = 0; (i<lambdaTransitions.Count); i++) {
                    sb.append((lambdaTransitions[i].stateNumber+(((i<(lambdaTransitions.Count-1)) ? ", " : ""))));
                }
                System.Console.Error.WriteLine(sb);
            }
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class RuleEE : Saxon.Hej.trans.rules.Rule {
        private System.Collections.Generic.IList<Saxon.Eej.ee.trans.Precondition> preconditions = new System.Collections.Generic.List<Saxon.Eej.ee.trans.Precondition>();
        public RuleEE()  {
        }
        public RuleEE(Saxon.Hej.pattern.Pattern p, Saxon.Hej.trans.rules.RuleTarget o, int prec, int min, double prio, int seq, int part) : base (p, o, prec, min, prio, seq, part) {
        }
        public override Saxon.Hej.trans.rules.Rule copy(bool copyChain) {
            Saxon.Eej.ee.trans.RuleEE r2 = new Saxon.Eej.ee.trans.RuleEE();
            r2.copyFrom(this, copyChain);
            return r2;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.trans.Precondition> getPreconditions() {
            return preconditions;
        }
        public void addPrecondition(Saxon.Eej.ee.trans.Precondition condition) {
            if (preconditions.Count==0) {
                preconditions = new System.Collections.Generic.List<Saxon.Eej.ee.trans.Precondition>();
            }
            preconditions.Add(condition);
        }
        public bool checkPreconditions(Saxon.Eej.ee.trans.PreconditionMatcher state) {
            foreach (Saxon.Eej.ee.trans.Precondition condition in preconditions) {
                if (!(condition.isSatisfied(state))) {
                    return false;
                }
            }
            return true;
        }
        public override void exportOtherProperties(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
    
    }
}
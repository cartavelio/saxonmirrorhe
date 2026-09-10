using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class PreconditionMatcher : Saxon.Hej.trans.rules.RuleSearchState {
        private readonly Saxon.Hej.expr.XPathContext context;
        private int[] preconditionState;
        private System.Object[] characteristics;
        public PreconditionMatcher(Saxon.Hej.trans.rules.RuleChain chain, Saxon.Hej.expr.XPathContext context) : base () {
            this.context = context;
            if ((chain.optimizationData != null)) {
                Saxon.Eej.ee.trans.ModeEE.RuleChainOptimizationData data = (Saxon.Eej.ee.trans.ModeEE.RuleChainOptimizationData)(chain.optimizationData);
                preconditionState = new int[data.preconditions.Count];
                characteristics = new System.Object[data.characteristics.Count];
            }
        }
        public int getPreconditionState(int slot) {
            return preconditionState[slot];
        }
        public void setPreconditionState(int slot, bool value) {
            preconditionState[slot] = (value ? Saxon.Eej.ee.trans.Precondition.PRECONDITION_TRUE : Saxon.Eej.ee.trans.Precondition.PRECONDITION_FALSE);
        }
        public System.Object obtainCharacteristic(Saxon.Eej.ee.trans.Characteristic chic) {
            System.Object val = characteristics[chic.getSlotNumber()];
            if ((val == null)) {
                val = chic.evaluate(context);
                characteristics[chic.getSlotNumber()] = val;
            }
            return val;
        }
    
    }
}
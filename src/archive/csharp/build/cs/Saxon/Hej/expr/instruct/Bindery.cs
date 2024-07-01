using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class Bindery {
        private Saxon.Hej.expr.instruct.SlotManager slotManager;
        private Saxon.Hej.om.GroundedValue[] globals;
        public Bindery(Saxon.Hej.expr.PackageData pack)  {
            this.slotManager = pack.getGlobalSlotManager();
            allocateGlobals(slotManager);
        }
        private void allocateGlobals(Saxon.Hej.expr.instruct.SlotManager map) {
            int n = (map.getNumberOfVariables()+1);
            globals = new Saxon.Hej.om.GroundedValue[n];
            for (int i = 0; (i<n); i++) {
                globals[i] = null;
            }
        }
        public void setGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable binding, Saxon.Hej.om.GroundedValue value) {
            globals[binding.getBinderySlotNumber()] = value;
        }
        public Saxon.Hej.om.GroundedValue saveGlobalVariableValue(Saxon.Hej.expr.instruct.GlobalVariable binding, Saxon.Hej.om.GroundedValue value) { lock(this) {
            int slot = binding.getBinderySlotNumber();
            if ((globals[slot] != null)) {
                return globals[slot];
            } else {
                globals[slot] = value;
                return value;
            }
        }}
        public void setGlobalVariableValue(int slot, Saxon.Hej.om.GroundedValue value) {
            globals[slot] = value;
        }
        public Saxon.Hej.om.GroundedValue getGlobalVariableValue(Saxon.Hej.expr.instruct.GlobalVariable binding) {
            return globals[binding.getBinderySlotNumber()];
        }
        public Saxon.Hej.om.GroundedValue getGlobalVariable(int slot) {
            return globals[slot];
        }
        public Saxon.Hej.om.GroundedValue[] getGlobalVariables() {
            return globals;
        }
        public class FailureValue : Saxon.Hej.value.ObjectValue<Saxon.Hej.trans.XPathException> {
            public FailureValue(Saxon.Hej.trans.XPathException err) : base (err) {
            }
        
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class Bindery {
        private Saxon.Hej.om.GroundedValue[] globals;
        private long[] busy;
        public Bindery(Saxon.Hej.expr.PackageData pack)  {
            allocateGlobals(pack.getGlobalSlotManager());
        }
        private void allocateGlobals(Saxon.Hej.expr.instruct.SlotManager map) {
            int n = (map.getNumberOfVariables()+1);
            globals = new Saxon.Hej.om.GroundedValue[n];
            busy = new long[n];
            for (int i = 0; (i<n); i++) {
                globals[i] = null;
                busy[i] = -(1L);
            }
        }
        public void setGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable binding, Saxon.Hej.om.GroundedValue value) {
            globals[binding.getBinderySlotNumber()] = value;
        }
        public bool setExecuting(Saxon.Hej.expr.instruct.GlobalVariable binding) {
            long thisThread = Saxon.Ejava.lang.Thread.currentThread().getId();
            int slot = binding.getBinderySlotNumber();
            long busyThread = busy[slot];
            if ((busyThread != -(1L))) {
                if ((busyThread == thisThread)) {
                    throw new Saxon.Hej.trans.XPathException.Circularity(("Circular definition of variable "+binding.getVariableQName().getDisplayName()));
                } else {
                    for (int i = 0; (i<10); i++) {
                        try {
                            Saxon.Ejava.lang.Thread.sleep((20*i));
                        } catch (System.Threading.ThreadInterruptedException e) {
                        }
                        if ((busy[slot] == -(1L))) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            busy[slot] = thisThread;
            return true;
        }
        public void setNotExecuting(Saxon.Hej.expr.instruct.GlobalVariable binding) {
            int slot = binding.getBinderySlotNumber();
            busy[slot] = -(1L);
        }
        public Saxon.Hej.om.GroundedValue saveGlobalVariableValue(Saxon.Hej.expr.instruct.GlobalVariable binding, Saxon.Hej.om.GroundedValue value) { lock(this) {
            int slot = binding.getBinderySlotNumber();
            if ((globals[slot] != null)) {
                return globals[slot];
            } else {
                busy[slot] = -(1L);
                globals[slot] = value;
                return value;
            }
        }}
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
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class SlotManager {
        public static Saxon.Hej.expr.instruct.SlotManager EMPTY = new Saxon.Hej.expr.instruct.SlotManager(0);
        private readonly System.Collections.Generic.List<Saxon.Hej.om.StructuredQName> variableMap;
        private int numberOfVariables = 0;
        public SlotManager()  {
            numberOfVariables = 0;
            variableMap = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
        }
        public SlotManager(int n)  {
            numberOfVariables = n;
            variableMap = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>(n);
        }
        public int getNumberOfVariables() {
            return numberOfVariables;
        }
        public void setNumberOfVariables(int numberOfVariables) {
            this.numberOfVariables = numberOfVariables;
            Saxon.Impl.Helpers.ListUtils.TrimToSize(variableMap);
        }
        public int allocateSlotNumber(Saxon.Hej.om.StructuredQName qName) {
            variableMap.Add(qName);
            return numberOfVariables++;
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> getVariableMap() {
            return variableMap;
        }
        public virtual void showStackFrame(Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.Logger logger) {
        }
    
    }
}
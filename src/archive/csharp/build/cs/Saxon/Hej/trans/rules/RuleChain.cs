using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class RuleChain {
        private Saxon.Hej.trans.rules.Rule _head;
        public System.Object optimizationData;
        public RuleChain()  {
            _head = null;
        }
        public RuleChain(Saxon.Hej.trans.rules.Rule head)  {
            this._head = head;
        }
        public Saxon.Hej.trans.rules.Rule head() {
            return _head;
        }
        public void setHead(Saxon.Hej.trans.rules.Rule head) {
            this._head = head;
        }
        public int getLength() {
            int i = 0;
            Saxon.Hej.trans.rules.Rule r = head();
            while ((r != null)) {
                i++;
                r = r.getNext();
            }
            return i;
        }
        public bool hasOptimizationData() {
            return (optimizationData != null);
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class RuleSearchState {
        private readonly static Saxon.Hej.trans.rules.RuleSearchState THE_INSTANCE = new Saxon.Hej.trans.rules.RuleSearchState();
        public static Saxon.Hej.trans.rules.RuleSearchState getInstance() {
            return THE_INSTANCE;
        }
        protected internal RuleSearchState()  {
        }
        public virtual bool checkPreconditions(Saxon.Hej.trans.rules.Rule rule) {
            return true;
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public interface RuleTarget {
        void export(Saxon.Hej.trace.ExpressionPresenter presenter);
        void registerRule(Saxon.Hej.trans.rules.Rule rule);
    
    }
}
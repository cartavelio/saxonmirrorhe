using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface TraceListener {
        void setOutputDestination(Saxon.Hej.lib.Logger stream);
        void open(Saxon.Hej.Controller controller);
        void close();
        void enter(Saxon.Hej.trace.Traceable instruction, System.Collections.Generic.IDictionary<string,System.Object> properties, Saxon.Hej.expr.XPathContext context);
        void leave(Saxon.Hej.trace.Traceable instruction);
        void startCurrentItem(Saxon.Hej.om.Item currentItem);
        void endCurrentItem(Saxon.Hej.om.Item currentItem);
        void startRuleSearch();
        void endRuleSearch(System.Object rule, Saxon.Hej.trans.Mode mode, Saxon.Hej.om.Item item);
    
    }
}
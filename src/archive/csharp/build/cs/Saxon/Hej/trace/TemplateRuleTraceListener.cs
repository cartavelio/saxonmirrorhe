using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class TemplateRuleTraceListener {
        private int depth = 0;
        private Saxon.Hej.lib.Logger logger;
        public TemplateRuleTraceListener(Saxon.Hej.lib.Logger logger)  {
            this.logger = logger;
        }
        public void enter(string instName, Saxon.Hej.s9api.Location instLoc, Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.TemplateRule rule) {
            Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
            for (int i = 0; (i<depth); i++) {
                builder.append(' ');
            }
            depth++;
            builder.append(instName).append(" at ").append(Saxon.Hej.trans.Err.show(instLoc));
            builder.append(" to ").append(item.toShortString());
            if (((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getLineNumber() != -1))) {
                builder.append(" at ").append(Saxon.Hej.trans.Err.abbreviateURI(((Saxon.Hej.om.NodeInfo)(item)).getBaseURI())).append("#").append(((Saxon.Hej.om.NodeInfo)(item)).getLineNumber());
            }
            builder.append(" using ");
            if ((rule == null)) {
                builder.append("built-in rule");
            } else {
                builder.append("rule at ").append(Saxon.Hej.trans.Err.show(rule));
            }
            string message = builder.ToString().Replace(" .../", " ");
            logger.info(message);
        }
        public void leave() {
            depth--;
        }
    
    }
}
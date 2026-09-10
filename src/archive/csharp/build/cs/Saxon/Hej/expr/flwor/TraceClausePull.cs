using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class TraceClausePull : Saxon.Hej.expr.flwor.TuplePull {
        private readonly Saxon.Hej.expr.flwor.TuplePull @base;
        private readonly Saxon.Hej.expr.flwor.Clause baseClause;
        private readonly Saxon.Hej.expr.flwor.TraceClause traceClause;
        public TraceClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.TraceClause traceClause, Saxon.Hej.expr.flwor.Clause baseClause)  {
            this.@base = @base;
            this.traceClause = traceClause;
            this.baseClause = baseClause;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            if (controller.isTracing()) {
                Saxon.Hej.expr.flwor.ClauseInfo baseInfo = new Saxon.Hej.expr.flwor.ClauseInfo(baseClause);
                baseInfo.setNamespaceResolver(traceClause.getNamespaceResolver());
                controller.getTraceListener().enter(baseInfo, new System.Collections.Generic.Dictionary<string, System.Object >(), context);
                bool b = @base.nextTuple(context);
                controller.getTraceListener().leave(baseInfo);
                return b;
            } else {
                return @base.nextTuple(context);
            }
        }
        public override void close() {
            @base.close();
        }
    
    }
}
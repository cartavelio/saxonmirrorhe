using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class TraceClausePush : Saxon.Hej.expr.flwor.TuplePush {
        private readonly Saxon.Hej.expr.flwor.TuplePush destination;
        internal Saxon.Hej.expr.flwor.TraceClause traceClause;
        private readonly Saxon.Hej.expr.flwor.Clause baseClause;
        public TraceClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.TraceClause traceClause, Saxon.Hej.expr.flwor.Clause baseClause) : base (outputter) {
            this.destination = destination;
            this.traceClause = traceClause;
            this.baseClause = baseClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            if (controller.isTracing()) {
                Saxon.Hej.expr.flwor.ClauseInfo baseInfo = new Saxon.Hej.expr.flwor.ClauseInfo(baseClause);
                baseInfo.setNamespaceResolver(traceClause.getNamespaceResolver());
                controller.getTraceListener().enter(baseInfo, baseClause.getTraceInfo(), context);
                destination.processTuple(context);
                controller.getTraceListener().leave(baseInfo);
            } else {
                destination.processTuple(context);
            }
        }
        public override void close() {
            destination.close();
        }
    
    }
}
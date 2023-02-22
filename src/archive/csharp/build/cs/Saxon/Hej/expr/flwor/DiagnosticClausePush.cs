using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class DiagnosticClausePush : Saxon.Hej.expr.flwor.TuplePush {
        internal Saxon.Hej.expr.flwor.TuplePush destination;
        internal Saxon.Hej.expr.flwor.DiagnosticClause diagClause;
        internal int tupleSeq = 1;
        public DiagnosticClausePush(Saxon.Hej.@event.Outputter outputter, Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.expr.flwor.DiagnosticClause diagClause) : base (outputter) {
            this.destination = destination;
            this.diagClause = diagClause;
        }
        public override void processTuple(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.SequenceIterator val = /*Delegate.iterate()*/diagClause.getEvaluator()(context);
            Saxon.Hej.lib.Logger @out = context.getController().getTraceFunctionDestination();
            Saxon.Hej.om.Item item;
            int pos = 1;
            while (((item = val.next()) != null)) {
                string label = ((("FLWOR-> "+tupleSeq)+"/")+pos++);
                Saxon.Hej.functions.Trace.traceItem(item, label, @out);
            }
            destination.processTuple(context);
            tupleSeq++;
        }
        public override void close() {
            destination.close();
        }
    
    }
}
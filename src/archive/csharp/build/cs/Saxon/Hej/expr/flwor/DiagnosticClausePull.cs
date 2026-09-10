using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class DiagnosticClausePull : Saxon.Hej.expr.flwor.TuplePull {
        internal Saxon.Hej.expr.flwor.TuplePull @base;
        internal Saxon.Hej.expr.flwor.DiagnosticClause diagClause;
        internal int tupleSeq = 1;
        public DiagnosticClausePull(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.flwor.DiagnosticClause diagClause)  {
            this.@base = @base;
            this.diagClause = diagClause;
        }
        public override bool nextTuple(Saxon.Hej.expr.XPathContext context) {
            if (!(@base.nextTuple(context))) {
                return false;
            }
            Saxon.Hej.om.SequenceIterator val = /*Delegate.iterate()*/diagClause.getEvaluator()(context);
            Saxon.Hej.lib.Logger @out = context.getController().getTraceFunctionDestination();
            Saxon.Hej.om.Item item;
            int pos = 1;
            while (((item = val.next()) != null)) {
                string label = ((("FLWOR<- "+tupleSeq)+"/")+pos++);
                Saxon.Hej.functions.Trace.traceItem(item, label, @out);
            }
            tupleSeq++;
            return true;
        }
        public override void close() {
            @base.close();
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class TraceClause : Saxon.Hej.expr.flwor.Clause {
        private readonly Saxon.Hej.expr.flwor.Clause target;
        private Saxon.Hej.om.NamespaceResolver nsResolver;
        public TraceClause(Saxon.Hej.expr.flwor.FLWORExpression expression, Saxon.Hej.expr.flwor.Clause target)  {
            this.target = target;
            this.nsResolver = expression.getRetainedStaticContext();
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return nsResolver;
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver nsResolver) {
            this.nsResolver = nsResolver;
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.TRACE;
        }
        public override Saxon.Hej.expr.flwor.TraceClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.expr.flwor.TraceClause(flwor, target);
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.TraceClausePull(@base, this, target);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.TraceClausePush(output, destination, this, target);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("trace");
            @out.endElement();
        }
        public override string ToString() {
            return "trace";
        }
    
    }
}
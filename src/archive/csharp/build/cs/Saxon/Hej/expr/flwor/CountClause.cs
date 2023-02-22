using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class CountClause : Saxon.Hej.expr.flwor.Clause {
        private Saxon.Hej.expr.flwor.LocalVariableBinding rangeVariable;
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.COUNT;
        }
        public override Saxon.Hej.expr.flwor.CountClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.CountClause c2 = new Saxon.Hej.expr.flwor.CountClause();
            c2.rangeVariable = rangeVariable.copy();
            c2.setPackageData(getPackageData());
            c2.setLocation(getLocation());
            return c2;
        }
        public void setRangeVariable(Saxon.Hej.expr.flwor.LocalVariableBinding binding) {
            this.rangeVariable = binding;
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding getRangeVariable() {
            return rangeVariable;
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            return new Saxon.Hej.expr.flwor.LocalVariableBinding[]{rangeVariable};
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.CountClausePull(@base, this);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.CountClausePush(output, destination, this);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("count");
            @out.emitAttribute("var", getRangeVariable().getVariableQName());
            @out.endElement();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("count $");
            fsb.append(rangeVariable.getVariableQName().getDisplayName());
            return fsb.ToString();
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class WhereClause : Saxon.Hej.expr.flwor.Clause {
        private readonly Saxon.Hej.expr.Operand predicateOp;
        public WhereClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression predicate)  {
            this.predicateOp = new Saxon.Hej.expr.Operand(flwor, predicate, Saxon.Hej.expr.OperandRole.INSPECT);
        }
        public override void setRepeated(bool repeated) {
            base.setRepeated(repeated);
            if (repeated) {
                this.predicateOp.setOperandRole(Saxon.Hej.expr.OperandRole.REPEAT_INSPECT);
            }
        }
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.WHERE;
        }
        public Saxon.Hej.expr.Expression getPredicate() {
            return predicateOp.getChildExpression();
        }
        public void setPredicate(Saxon.Hej.expr.Expression predicate) {
            predicateOp.setChildExpression(predicate);
        }
        public override Saxon.Hej.expr.flwor.WhereClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.WhereClause w2 = new Saxon.Hej.expr.flwor.WhereClause(flwor, getPredicate().copy(rebindings));
            w2.setLocation(getLocation());
            w2.setPackageData(getPackageData());
            return w2;
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            base.typeCheck(visitor, contextInfo);
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.WhereClausePull(@base, getPredicate());
        }
        public override void gatherVariableReferences(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Binding binding, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references) {
            Saxon.Hej.expr.parser.ExpressionTool.gatherVariableReferences(getPredicate(), binding, references);
        }
        public override void refineVariableType(Saxon.Hej.expr.parser.ExpressionVisitor visitor, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, Saxon.Hej.expr.Expression returnExpr) {
            Saxon.Hej.type.ItemType actualItemType = getPredicate().getItemType();
            foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                @ref.refineVariableType(actualItemType, getPredicate().getCardinality(), ((getPredicate() is Saxon.Hej.expr.Literal) ? ((Saxon.Hej.expr.Literal)(getPredicate())).getGroundedValue() : null), getPredicate().getSpecialProperties());
                Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(returnExpr);
            }
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.WhereClausePush(output, destination, getPredicate());
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(predicateOp);
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            getPredicate().addToPathMap(pathMap, pathMapNodeSet);
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("where");
            getPredicate().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("where ");
            fsb.append(getPredicate().toShortString());
            return fsb.ToString();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("where ");
            fsb.append(getPredicate().ToString());
            return fsb.ToString();
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class DiagnosticClause : Saxon.Hej.expr.flwor.Clause {
        private Saxon.Hej.expr.Operand sequenceOp;
        private Saxon.Hej.expr.elab.PullEvaluator evaluator;
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.DIAG;
        }
        public Saxon.Hej.expr.elab.PullEvaluator getEvaluator() {
            if ((evaluator == null)) {
                evaluator = getSequence().makeElaborator().elaborateForPull();
            }
            return evaluator;
        }
        public override Saxon.Hej.expr.flwor.DiagnosticClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.DiagnosticClause diag2 = new Saxon.Hej.expr.flwor.DiagnosticClause();
            diag2.setLocation(getLocation());
            diag2.setPackageData(getPackageData());
            diag2.initSequence(flwor, getSequence().copy(rebindings));
            return diag2;
        }
        public void initSequence(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression sequence) {
            sequenceOp = new Saxon.Hej.expr.Operand(flwor, sequence, (isRepeated() ? Saxon.Hej.expr.OperandRole.REPEAT_NAVIGATE : Saxon.Hej.expr.OperandRole.NAVIGATE));
        }
        public Saxon.Hej.expr.Expression getSequence() {
            return sequenceOp.getChildExpression();
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            return new Saxon.Hej.expr.flwor.LocalVariableBinding[]{};
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.DiagnosticClausePull(@base, this);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.DiagnosticClausePush(output, destination, this);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(sequenceOp);
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
        }
        public override void gatherVariableReferences(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.Binding binding, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references) {
            Saxon.Hej.expr.parser.ExpressionTool.gatherVariableReferences(getSequence(), binding, references);
        }
        public override void refineVariableType(Saxon.Hej.expr.parser.ExpressionVisitor visitor, System.Collections.Generic.IList<Saxon.Hej.expr.VariableReference> references, Saxon.Hej.expr.Expression returnExpr) {
            Saxon.Hej.expr.Expression seq = getSequence();
            Saxon.Hej.type.ItemType actualItemType = seq.getItemType();
            foreach (Saxon.Hej.expr.VariableReference @ref in references) {
                @ref.refineVariableType(actualItemType, getSequence().getCardinality(), ((seq is Saxon.Hej.expr.Literal) ? ((Saxon.Hej.expr.Literal)(seq)).getGroundedValue() : null), seq.getSpecialProperties());
                Saxon.Hej.expr.parser.ExpressionTool.resetStaticProperties(returnExpr);
            }
        }
        public override void addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet varPath = getSequence().addToPathMap(pathMap, pathMapNodeSet);
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("trace");
            getSequence().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("trace ");
            fsb.append(getSequence().toShortString());
            return fsb.ToString();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("trace ");
            fsb.append(getSequence().ToString());
            return fsb.ToString();
        }
    
    }
}
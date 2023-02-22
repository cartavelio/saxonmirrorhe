using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.flwor {
    public class LetClause : Saxon.Hej.expr.flwor.Clause {
        private Saxon.Hej.expr.flwor.LocalVariableBinding rangeVariable;
        private Saxon.Hej.expr.Operand sequenceOp;
        private Saxon.Hej.expr.elab.SequenceEvaluator variableEvaluator;
        public override Saxon.Hej.expr.flwor.Clause.ClauseName getClauseKey() {
            return /*explicit enum*/ Saxon.Hej.expr.flwor.Clause.ClauseName.LET;
        }
        public Saxon.Hej.expr.elab.SequenceEvaluator getEvaluator() {
            if ((variableEvaluator == null)) {
                variableEvaluator = new Saxon.Hej.expr.elab.LearningEvaluator(getSequence(), getSequence().makeElaborator().lazily(true));
            }
            return variableEvaluator;
        }
        public override Saxon.Hej.expr.flwor.LetClause copy(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.flwor.LetClause let2 = new Saxon.Hej.expr.flwor.LetClause();
            let2.setLocation(getLocation());
            let2.setPackageData(getPackageData());
            let2.rangeVariable = rangeVariable.copy();
            let2.initSequence(flwor, getSequence().copy(rebindings));
            return let2;
        }
        public void initSequence(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.Expression sequence) {
            sequenceOp = new Saxon.Hej.expr.Operand(flwor, sequence, (isRepeated() ? Saxon.Hej.expr.OperandRole.REPEAT_NAVIGATE : Saxon.Hej.expr.OperandRole.NAVIGATE));
        }
        public void setSequence(Saxon.Hej.expr.Expression sequence) {
            sequenceOp.setChildExpression(sequence);
        }
        public Saxon.Hej.expr.Expression getSequence() {
            return sequenceOp.getChildExpression();
        }
        public void setRangeVariable(Saxon.Hej.expr.flwor.LocalVariableBinding binding) {
            this.rangeVariable = binding;
        }
        public Saxon.Hej.expr.flwor.LocalVariableBinding getRangeVariable() {
            return rangeVariable;
        }
        public void evaluateRangeVariable(Saxon.Hej.expr.XPathContext context) {
            if ((variableEvaluator == null)) {
                getEvaluator();
            }
            Saxon.Hej.om.Sequence val = variableEvaluator.evaluate(context);
            context.setLocalVariable(getRangeVariable().getLocalSlotNumber(), val);
        }
        public override Saxon.Hej.expr.flwor.LocalVariableBinding[] getRangeVariables() {
            return new Saxon.Hej.expr.flwor.LocalVariableBinding[]{rangeVariable};
        }
        public override Saxon.Hej.expr.flwor.TuplePull getPullStream(Saxon.Hej.expr.flwor.TuplePull @base, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.LetClausePull(@base, this);
        }
        public override Saxon.Hej.expr.flwor.TuplePush getPushStream(Saxon.Hej.expr.flwor.TuplePush destination, Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.flwor.LetClausePush(output, destination, this);
        }
        public override void processOperands(Saxon.Hej.expr.flwor.OperandProcessor processor) {
            /*Delegate.processOperand()*/processor(sequenceOp);
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, rangeVariable.getVariableQName().getDisplayName(), 0);
            setSequence(Saxon.Hej.expr.parser.TypeChecker.strictTypeCheck(getSequence(), rangeVariable.getRequiredType(), role, visitor.getStaticContext()));
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
            pathMap.registerPathForVariable(rangeVariable, varPath);
        }
        public override void explain(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("let");
            @out.emitAttribute("var", getRangeVariable().getVariableQName());
            @out.emitAttribute("slot", (getRangeVariable().getLocalSlotNumber()+""));
            getSequence().export(@out);
            @out.endElement();
        }
        public override string toShortString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("let $");
            fsb.append(rangeVariable.getVariableQName().getDisplayName());
            fsb.append(" := ");
            fsb.append(getSequence().toShortString());
            return fsb.ToString();
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            fsb.append("let $");
            fsb.append(rangeVariable.getVariableQName().getDisplayName());
            fsb.append(" := ");
            fsb.append(getSequence().ToString());
            return fsb.ToString();
        }
    
    }
}
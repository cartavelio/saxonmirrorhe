using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class LocalParamBlock : Saxon.Hej.expr.instruct.Instruction {
        internal Saxon.Hej.expr.Operand[] operanda;
        public LocalParamBlock(Saxon.Hej.expr.instruct.LocalParam[] @params)  {
            operanda = new Saxon.Hej.expr.Operand[@params.Length];
            for (int i = 0; (i<@params.Length); i++) {
                operanda[i] = new Saxon.Hej.expr.Operand(this, @params[i], Saxon.Hej.expr.OperandRole.NAVIGATE);
            }
        }
        public override string getExpressionName() {
            return "params";
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return Saxon.Ejava.util.Arrays.asList(operanda);
        }
        public int getNumberOfParams() {
            return operanda.Length;
        }
        protected internal override int computeSpecialProperties() {
            return 0;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.LocalParam[] lps2 = new Saxon.Hej.expr.instruct.LocalParam[getNumberOfParams()];
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.expr.instruct.LocalParam oldLps = (Saxon.Hej.expr.instruct.LocalParam)(o.getChildExpression());
                Saxon.Hej.expr.instruct.LocalParam newLps = oldLps.copy(rebindings);
                rebindings.put(oldLps, newLps);
                lps2[i++] = newLps;
            }
            return new Saxon.Hej.expr.instruct.LocalParamBlock(lps2);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.ErrorType.getInstance();
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EMPTY;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("params", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
        public override int getImplementationMethod() {
            return PROCESS_METHOD;
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.LocalParamBlock.LocalParamBlockElaborator();
        }
        public class LocalParamBlockElaborator : Saxon.Hej.expr.elab.PushElaborator {
            internal Saxon.Hej.expr.elab.SequenceEvaluator[] paramEval;
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.LocalParamBlock expr = (Saxon.Hej.expr.instruct.LocalParamBlock)(getExpression());
                paramEval = new Saxon.Hej.expr.elab.SequenceEvaluator[expr.operanda.Length];
                for (int i = 0; (i<expr.operanda.Length); i++) {
                    paramEval[i] = expr.operanda[i].getChildExpression().makeElaborator().eagerly();
                }
                return (@out, context) => {
                    foreach (Saxon.Hej.expr.elab.SequenceEvaluator eagerEvaluator in paramEval) {
                        eagerEvaluator.evaluate(context);
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}
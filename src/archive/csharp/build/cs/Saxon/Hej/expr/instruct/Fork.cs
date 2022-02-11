using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class Fork : Saxon.Hej.expr.instruct.Instruction {
        internal Saxon.Hej.expr.Operand[] operanda;
        public Fork(Saxon.Hej.expr.Operand[] prongs)  {
            operanda = new Saxon.Hej.expr.Operand[prongs.Length];
            for (int i = 0; (i<prongs.Length); i++) {
                operanda[i] = new Saxon.Hej.expr.Operand(this, prongs[i].getChildExpression(), Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            }
        }
        public Fork(Saxon.Hej.expr.Expression[] prongs)  {
            operanda = new Saxon.Hej.expr.Operand[prongs.Length];
            for (int i = 0; (i<prongs.Length); i++) {
                operanda[i] = new Saxon.Hej.expr.Operand(this, prongs[i], Saxon.Hej.expr.OperandRole.SAME_FOCUS_ACTION);
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return Saxon.Ejava.util.Arrays.asList(operanda);
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_FORK;
        }
        public int getSize() {
            return operanda.Length;
        }
        public Saxon.Hej.expr.Expression getProng(int i) {
            return operanda[i].getChildExpression();
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            if ((getSize() == 0)) {
                return Saxon.Hej.type.ErrorType.getInstance();
            }
            Saxon.Hej.type.ItemType t1 = null;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                Saxon.Hej.type.ItemType t2 = o.getChildExpression().getItemType();
                t1 = ((t1 == null) ? t2 : Saxon.Hej.type.Type.getCommonSuperType(t1, t2));
                if ((t1 is Saxon.Hej.type.AnyItemType)) {
                    return t1;
                }
            }
            return t1;
        }
        public override string getStreamerName() {
            return "Fork";
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] e2 = new Saxon.Hej.expr.Expression[getSize()];
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                e2[i++] = o.getChildExpression().copy(rebindings);
            }
            Saxon.Hej.expr.instruct.Fork f2 = new Saxon.Hej.expr.instruct.Fork(e2);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, f2);
            return f2;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().process(output, context);
            }
            return null;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("fork", this);
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                o.getChildExpression().export(@out);
            }
            @out.endElement();
        }
    
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.instruct {
    public class Assign : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.BindingReference {
        private readonly Saxon.Hej.expr.Operand selectOp;
        private Saxon.Hej.expr.instruct.GlobalVariable binding;
        public Assign(Saxon.Hej.expr.Expression select)  {
            selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.NAVIGATE);
        }
        public void setStaticType(Saxon.Hej.value.SequenceType type, Saxon.Hej.om.GroundedValue constantValue, int properties) {
        }
        public void setSelectExpr(Saxon.Hej.expr.Expression select) {
            selectOp.setChildExpression(select);
        }
        public Saxon.Hej.expr.Expression getSelectExpr() {
            return selectOp.getChildExpression();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return selectOp;
        }
        public void fixup(Saxon.Hej.expr.Binding binding) {
            this.binding = (Saxon.Hej.expr.instruct.GlobalVariable)(binding);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, binding.getObjectName().getDisplayName(), 0);
            selectOp.setChildExpression(tc.staticTypeCheck(getSelectExpr(), binding.getRequiredType(), role, visitor));
            return this;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.xsltextn.instruct.Assign a2 = new Saxon.Eej.xsltextn.instruct.Assign(getSelectExpr().copy(rebindings));
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, a2);
            a2.binding = binding;
            return a2;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.SAXON_ASSIGN;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            if ((binding == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("saxon:assign binding has not been fixed up");
            }
            Saxon.Hej.om.GroundedValue value = Saxon.Hej.om.SequenceTool.toGroundedValue(getSelectExpr().iterate(context));
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            controller.getBindery(getPackageData()).setGlobalVariable(binding, value);
            return null;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            if ("JS".Equals(((Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(@out.getOptions())).target)) {
                throw new Saxon.Hej.trans.XPathException("Export failure: Saxon-JS does not implement saxon:assign");
            }
            @out.startElement("assign", this);
            @out.emitAttribute("name", binding.getVariableQName());
            getSelectExpr().export(@out);
            @out.endElement();
        }
    
    }
}
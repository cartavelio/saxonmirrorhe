using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public sealed class LocalParam : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.LocalBinding {
        private Saxon.Hej.expr.Operand conversionOp = null;
        private const int REQUIRED = 4;
        private const int TUNNEL = 8;
        private const int IMPLICITLY_REQUIRED = 16;
        private int properties = 0;
        private Saxon.Hej.expr.Operand selectOp = null;
        private Saxon.Hej.om.StructuredQName variableQName;
        private Saxon.Hej.value.SequenceType requiredType;
        private int slotNumber = -999;
        private int referenceCount = 10;
        public void setSelectExpression(Saxon.Hej.expr.Expression select) {
            if ((select != null)) {
                if ((selectOp == null)) {
                    selectOp = new Saxon.Hej.expr.Operand(this, select, Saxon.Hej.expr.OperandRole.NAVIGATE);
                } else {
                    selectOp.setChildExpression(select);
                }
            } else {
                selectOp = null;
            }
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return ((selectOp == null) ? null : selectOp.getChildExpression());
        }
        public void setRequiredType(Saxon.Hej.value.SequenceType required) {
            requiredType = required;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public void setRequiredParam(bool requiredParam) {
            if (requiredParam) {
                properties |= REQUIRED;
            } else {
                properties &= ~(REQUIRED);
            }
        }
        public void setImplicitlyRequiredParam(bool requiredParam) {
            if (requiredParam) {
                properties |= IMPLICITLY_REQUIRED;
            } else {
                properties &= ~(IMPLICITLY_REQUIRED);
            }
        }
        public void setTunnel(bool tunnel) {
            if (tunnel) {
                properties |= TUNNEL;
            } else {
                properties &= ~(TUNNEL);
            }
        }
        public void setReferenceCount(int refCount) {
            referenceCount = refCount;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EMPTY;
        }
        public bool isAssignable() {
            return false;
        }
        public bool isGlobal() {
            return false;
        }
        public int getLocalSlotNumber() {
            return slotNumber;
        }
        public bool isRequiredParam() {
            return (((properties&REQUIRED)) != 0);
        }
        public bool isImplicitlyRequiredParam() {
            return (((properties&IMPLICITLY_REQUIRED)) != 0);
        }
        public bool isTunnelParam() {
            return (((properties&TUNNEL)) != 0);
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.Expression e2 = base.typeCheck(visitor, contextItemType);
            if ((e2 != this)) {
                return e2;
            }
            checkAgainstRequiredType(visitor);
            return this;
        }
        public void computeEvaluationMode() {
        }
        public override Saxon.Hej.expr.instruct.LocalParam copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.LocalParam p2 = new Saxon.Hej.expr.instruct.LocalParam();
            if ((conversionOp != null)) {
                System.Diagnostics.Debug.Assert((getConversion() != null));
                p2.setConversion(getConversion().copy(rebindings));
            }
            p2.properties = properties;
            if ((selectOp != null)) {
                System.Diagnostics.Debug.Assert((getSelectExpression() != null));
                p2.setSelectExpression(getSelectExpression().copy(rebindings));
            }
            p2.variableQName = variableQName;
            p2.requiredType = requiredType;
            p2.slotNumber = slotNumber;
            p2.referenceCount = referenceCount;
            return p2;
        }
        public void addReference(Saxon.Hej.expr.VariableReference @ref, bool isLoopingReference) {
        }
        public void checkAgainstRequiredType(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
            Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, variableQName.getDisplayName(), 0);
            Saxon.Hej.value.SequenceType r = requiredType;
            Saxon.Hej.expr.Expression select = getSelectExpression();
            if (((r != null) && (select != null))) {
                select = visitor.getConfiguration().getTypeChecker(false).staticTypeCheck(select, requiredType, role, visitor);
            }
        }
        public int getSlotNumber() {
            return slotNumber;
        }
        public void setSlotNumber(int s) {
            slotNumber = s;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName s) {
            variableQName = s;
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            return variableQName;
        }
        public void setConversion(Saxon.Hej.expr.Expression convertor) {
            if ((convertor != null)) {
                if ((conversionOp == null)) {
                    conversionOp = new Saxon.Hej.expr.Operand(this, convertor, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                }
            } else {
                conversionOp = null;
            }
        }
        public Saxon.Hej.expr.Expression getConversion() {
            return ((conversionOp == null) ? null : conversionOp.getChildExpression());
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_PARAM;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(selectOp, conversionOp);
        }
        public Saxon.Hej.value.IntegerValue[] getIntegerBoundsForVariable() {
            return null;
        }
        public Saxon.Hej.om.Sequence evaluateVariable(Saxon.Hej.expr.XPathContext c) {
            return c.evaluateLocalVariable(slotNumber);
        }
        public bool isCompatible(Saxon.Hej.expr.instruct.LocalParam other) {
            return ((getVariableQName().Equals(other.getVariableQName()) && getRequiredType().Equals(other.getRequiredType())) && (isTunnelParam() == other.isTunnelParam()));
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public override bool hasVariableBinding(Saxon.Hej.expr.Binding binding) {
            return (this == binding);
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.ErrorType.getInstance();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        protected internal override int computeSpecialProperties() {
            return Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
        }
        public override bool mayCreateNewNodes() {
            return false;
        }
        public override string getExpressionName() {
            return "param";
        }
        public override string toShortString() {
            return ("$"+getVariableQName().getDisplayName());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("param", this);
            @out.emitAttribute("name", getVariableQName());
            @out.emitAttribute("slot", (""+getSlotNumber()));
            string flags = getFlags();
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            if ((getRequiredType() != Saxon.Hej.value.SequenceType.ANY_SEQUENCE)) {
                @out.emitAttribute("as", getRequiredType().toAlphaCode());
            }
            if ((getSelectExpression() != null)) {
                @out.setChildRole("select");
                getSelectExpression().export(@out);
            }
            Saxon.Hej.expr.Expression conversion = getConversion();
            if ((conversion != null)) {
                @out.setChildRole("conversion");
                conversion.export(@out);
            }
            @out.endElement();
        }
        private string getFlags() {
            string flags = "";
            if (isTunnelParam()) {
                flags += "t";
            }
            if (isRequiredParam()) {
                flags += "r";
            }
            if (isImplicitlyRequiredParam()) {
                flags += "i";
            }
            return flags;
        }
        public void setIndexedVariable() {
        }
        public bool isIndexedVariable() {
            return false;
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.LocalParam.LocalParamElaborator();
        }
        public class LocalParamElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.LocalParam expr = (Saxon.Hej.expr.instruct.LocalParam)(getExpression());
                Saxon.Hej.expr.elab.SequenceEvaluator selectEvaluator;
                if ((expr.getSelectExpression() == null)) {
                    selectEvaluator = null;
                } else {
                    if ((expr.referenceCount == Saxon.Hej.expr.FilterExpression.FILTERED)) {
                        Saxon.Hej.expr.elab.PullEvaluator pullEval = expr.getSelectExpression().makeElaborator().elaborateForPull();
                        selectEvaluator = new Saxon.Hej.expr.elab.IndexedVariableEvaluator(pullEval);
                    } else {
                        Saxon.Hej.expr.Expression select = expr.getSelectExpression();
                        selectEvaluator = new Saxon.Hej.expr.elab.LearningEvaluator(select, select.makeElaborator().lazily(true));
                    }
                }
                Saxon.Hej.expr.elab.SequenceEvaluator conversionEval = (((expr.getConversion() == null)) ? null : expr.getConversion().makeElaborator().eagerly());
                return (@out, context) => {
                    int wasSupplied = context.useLocalParameter(expr.variableQName, expr.slotNumber, expr.isTunnelParam());
                    switch (wasSupplied) {
                      case Saxon.Hej.expr.instruct.ParameterSet.SUPPLIED_AND_CHECKED:
                        break ;
                      case Saxon.Hej.expr.instruct.ParameterSet.SUPPLIED:
                        if ((conversionEval != null)) {
                            context.setLocalVariable(expr.slotNumber, conversionEval.evaluate(context));
                        }
                        break ;
                      case Saxon.Hej.expr.instruct.ParameterSet.NOT_SUPPLIED:
                        if ((expr.isRequiredParam() || expr.isImplicitlyRequiredParam())) {
                            string name = ("$"+expr.getVariableQName().getDisplayName());
                            int suppliedAsTunnel = context.useLocalParameter(expr.variableQName, expr.slotNumber, !(expr.isTunnelParam()));
                            string message = ("No value supplied for required parameter "+name);
                            if (expr.isImplicitlyRequiredParam()) {
                                message += (". A value is required because "+"the default value is not a valid instance of the required type");
                            }
                            if ((suppliedAsTunnel != Saxon.Hej.expr.instruct.ParameterSet.NOT_SUPPLIED)) {
                                if (expr.isTunnelParam()) {
                                    message += ". A non-tunnel parameter with this name was supplied, but a tunnel parameter is required";
                                } else {
                                    message += ". A tunnel parameter with this name was supplied, but a non-tunnel parameter is required";
                                }
                            }
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(message);
                            e.setXPathContext(context);
                            e.setErrorCode("XTDE0700");
                            throw e;
                        }
                        if ((selectEvaluator == null)) {
                            throw new Saxon.Ejava.lang.AssertionError("Internal error: No select expression");
                        } else {
                            int savedOutputState = context.getTemporaryOutputState();
                            context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_WITH_PARAM);
                            Saxon.Hej.om.Sequence result = selectEvaluator.evaluate(context);
                            context.setLocalVariable(expr.slotNumber, result);
                            context.setTemporaryOutputState(savedOutputState);
                            return null;
                        }
                    }
                    return null;
                } ;
            }
        
        }
    
    }
}
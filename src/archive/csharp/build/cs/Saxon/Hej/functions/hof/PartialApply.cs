using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    #pragma warning disable 659
    public class PartialApply : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.expr.Operand baseOp;
        private readonly Saxon.Hej.expr.Operand[] boundArgumentsOp;
        public PartialApply(Saxon.Hej.expr.Expression @base, Saxon.Hej.expr.Expression[] boundArguments)  {
            baseOp = new Saxon.Hej.expr.Operand(this, @base, Saxon.Hej.expr.OperandRole.INSPECT);
            adoptChildExpression(@base);
            boundArgumentsOp = new Saxon.Hej.expr.Operand[boundArguments.Length];
            for (int i = 0; (i<boundArguments.Length); i++) {
                if ((boundArguments[i] != null)) {
                    boundArgumentsOp[i] = new Saxon.Hej.expr.Operand(this, boundArguments[i], Saxon.Hej.expr.OperandRole.NAVIGATE);
                    adoptChildExpression(boundArguments[i]);
                }
            }
        }
        public Saxon.Hej.expr.Expression getBaseExpression() {
            return baseOp.getChildExpression();
        }
        public void setBaseExpression(Saxon.Hej.expr.Expression @base) {
            baseOp.setChildExpression(@base);
        }
        public int getNumberOfPlaceHolders() {
            int n = 0;
            foreach (Saxon.Hej.expr.Operand o in boundArgumentsOp) {
                if ((o == null)) {
                    n++;
                }
            }
            return n;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            Saxon.Hej.type.ItemType baseType = getBaseExpression().getItemType();
            Saxon.Hej.value.SequenceType requiredFunctionType;
            Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[boundArgumentsOp.Length];
            Saxon.Ejava.util.Arrays.fill(argTypes, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            for (int i = 0; (i<boundArgumentsOp.Length); i++) {
                Saxon.Hej.expr.Operand op = boundArgumentsOp[i];
                if ((op != null)) {
                    Saxon.Hej.expr.Expression arg = op.getChildExpression();
                    if (((baseType is Saxon.Hej.type.SpecificFunctionType) && (i<((Saxon.Hej.type.SpecificFunctionType)(baseType)).getArity()))) {
                        Saxon.Hej.expr.parser.RoleDiagnostic argRole = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "saxon:call", i);
                        Saxon.Hej.value.SequenceType requiredArgType = ((Saxon.Hej.type.SpecificFunctionType)(baseType)).getArgumentTypes()[i];
                        argTypes[i] = requiredArgType;
                        Saxon.Hej.expr.Expression a3 = tc.staticTypeCheck(arg, requiredArgType, argRole, visitor);
                        if ((a3 != arg)) {
                            op.setChildExpression(a3);
                        }
                    }
                }
            }
            requiredFunctionType = Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.type.SpecificFunctionType(argTypes, (((baseType is Saxon.Hej.type.AnyFunctionType)) ? ((Saxon.Hej.type.AnyFunctionType)(baseType)).getResultType() : Saxon.Hej.value.SequenceType.ANY_SEQUENCE)), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, "saxon:call", 0);
            setBaseExpression(tc.staticTypeCheck(getBaseExpression(), requiredFunctionType, role, visitor));
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            Saxon.Hej.type.ItemType baseItemType = getBaseExpression().getItemType();
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            if ((baseItemType is Saxon.Hej.type.SpecificFunctionType)) {
                resultType = ((Saxon.Hej.type.SpecificFunctionType)(baseItemType)).getResultType();
            }
            int placeholders = getNumberOfPlaceHolders();
            Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[placeholders];
            if ((baseItemType is Saxon.Hej.type.SpecificFunctionType)) {
                for (int i = 0, j = 0; (i<boundArgumentsOp.Length); i++) {
                    if ((boundArgumentsOp[i] == null)) {
                        argTypes[j++] = ((Saxon.Hej.type.SpecificFunctionType)(baseItemType)).getArgumentTypes()[i];
                    }
                }
            } else {
                Saxon.Ejava.util.Arrays.fill(argTypes, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            }
            return new Saxon.Hej.type.SpecificFunctionType(argTypes, resultType);
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> operanda = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>((boundArgumentsOp.Length+1));
            operanda.Add(baseOp);
            foreach (Saxon.Hej.expr.Operand o in boundArgumentsOp) {
                if ((o != null)) {
                    operanda.Add(o);
                }
            }
            return operanda;
        }
        public int getNumberOfArguments() {
            return boundArgumentsOp.Length;
        }
        public Saxon.Hej.expr.Expression getArgument(int n) {
            Saxon.Hej.expr.Operand o = boundArgumentsOp[n];
            return ((o == null) ? null : o.getChildExpression());
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Hej.functions.hof.PartialApply)))) {
                return false;
            } else {
                Saxon.Hej.functions.hof.PartialApply pa2 = (Saxon.Hej.functions.hof.PartialApply)(other);
                if (!(getBaseExpression().isEqual(pa2.getBaseExpression()))) {
                    return false;
                }
                if ((boundArgumentsOp.Length != pa2.boundArgumentsOp.Length)) {
                    return false;
                }
                for (int i = 0; (i<boundArgumentsOp.Length); i++) {
                    if ((((boundArgumentsOp[i] == null)) != ((pa2.boundArgumentsOp[i] == null)))) {
                        return false;
                    }
                    if (((boundArgumentsOp[i] != null) && !(boundArgumentsOp[i].Equals(pa2.boundArgumentsOp[i])))) {
                        return false;
                    }
                }
                return true;
            }
        }
        protected internal override int computeHashCode() {
            int h = 0x236b92a0;
            int i = 0;
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                h ^= ((o == null) ? i++ : o.getChildExpression().GetHashCode());
            }
            return h;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("partialApply", this);
            getBaseExpression().export(@out);
            foreach (Saxon.Hej.expr.Operand o in boundArgumentsOp) {
                if ((o == null)) {
                    @out.startElement("null", this);
                    @out.endElement();
                } else {
                    o.getChildExpression().export(@out);
                }
            }
            @out.endElement();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.Expression[] boundArgumentsCopy = new Saxon.Hej.expr.Expression[boundArgumentsOp.Length];
            for (int i = 0; (i<boundArgumentsOp.Length); i++) {
                if ((boundArgumentsOp[i] == null)) {
                    boundArgumentsCopy[i] = null;
                } else {
                    boundArgumentsCopy[i] = boundArgumentsOp[i].getChildExpression().copy(rebindings);
                }
            }
            Saxon.Hej.functions.hof.PartialApply exp = new Saxon.Hej.functions.hof.PartialApply(getBaseExpression().copy(rebindings), boundArgumentsCopy);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
            bool par = getBaseExpression().operands().GetEnumerator().MoveNext();
            if (par) {
                buff.append((("("+getBaseExpression().ToString())+")"));
            } else {
                buff.append(getBaseExpression().ToString());
            }
            buff.append("(");
            for (int i = 0; (i<boundArgumentsOp.Length); i++) {
                if ((boundArgumentsOp[i] == null)) {
                    buff.append("?");
                } else {
                    buff.append(boundArgumentsOp[i].getChildExpression().ToString());
                }
                if ((i != (boundArgumentsOp.Length-1))) {
                    buff.append(", ");
                }
            }
            buff.append(")");
            return buff.ToString();
        }
        public override Saxon.Hej.om.Function evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Function f = (Saxon.Hej.om.Function)(getBaseExpression().evaluateItem(context));
            System.Diagnostics.Debug.Assert((f != null));
            Saxon.Hej.om.Sequence[] values = new Saxon.Hej.om.Sequence[boundArgumentsOp.Length];
            for (int i = 0; (i<boundArgumentsOp.Length); i++) {
                if ((boundArgumentsOp[i] == null)) {
                    values[i] = null;
                } else {
                    values[i] = Saxon.Hej.om.SequenceTool.toGroundedValue(boundArgumentsOp[i].getChildExpression().iterate(context));
                }
            }
            return new Saxon.Hej.functions.hof.CurriedFunction(f, values);
        }
        public override string getExpressionName() {
            return "partialApply";
        }
    
    }
}
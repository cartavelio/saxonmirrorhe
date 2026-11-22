package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/** Bind an inline stylesheet function to its originating transformation when it is evaluated. */
public final class InlineFunctionReference extends Expression {
    private final Operand literal;

    public InlineFunctionReference(FunctionLiteral function) {
        literal = new Operand(this, function, OperandRole.INSPECT);
    }

    @Override
    public Iterable<Operand> operands() {
        return literal;
    }

    @Override
    public int getImplementationMethod() {
        return EVALUATE_METHOD;
    }

    @Override
    public int getIntrinsicDependencies() {
        return StaticProperty.DEPENDS_ON_RUNTIME_ENVIRONMENT;
    }

    @Override
    protected int computeCardinality() {
        return StaticProperty.EXACTLY_ONE;
    }

    @Override
    public ItemType getItemType() {
        return literal.getChildExpression().getItemType();
    }

    @Override
    public FunctionItem evaluateItem(XPathContext context) {
        FunctionLiteral expression = (FunctionLiteral) literal.getChildExpression();
        FunctionItem function = expression.getGroundedValue();
        return new UserFunctionReference.BoundUserFunction(function, function.getArity(),
                context.getCurrentComponent(), expression, context.getController());
    }

    @Override
    public Expression copy(RebindingMap rebindings) {
        FunctionLiteral function = (FunctionLiteral) literal.getChildExpression().copy(rebindings);
        InlineFunctionReference copy = new InlineFunctionReference(function);
        ExpressionTool.copyLocationInfo(this, copy);
        return copy;
    }

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        out.startElement("boundInlineFn", this);
        literal.getChildExpression().export(out);
        out.endElement();
    }
}

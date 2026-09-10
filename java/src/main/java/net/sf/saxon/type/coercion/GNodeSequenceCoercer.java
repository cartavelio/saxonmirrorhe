package net.sf.saxon.type.coercion;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

public class GNodeSequenceCoercer extends SequenceCoercer {

    public GNodeSequenceCoercer(Expression sequence, SequenceType requiredType,
                            Supplier<RoleDiagnostic> role, boolean allow40) {
        super(sequence, requiredType, role, allow40);
    }

    @Override
    protected OperandRole getOperandRole() {
        return OperandRole.SAME_FOCUS_ACTION;
    }

    @Override
    public String getExpressionName() {
        return "GNodeCoercer";
    }

    @Override
    public String getStreamerName() {
        return "GNodeCoercer";
    }
}



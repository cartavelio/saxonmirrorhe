package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.BooleanEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.BooleanElaborator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.QuantifiedExpression;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public class QuantifiedExprElaborator extends BooleanElaborator {

    public BooleanEvaluator getBooleanEvaluator() {
        final QuantifiedExpression expr = (QuantifiedExpression) getExpression();
        final PullEvaluator selectEval = Elaborator.make(expr.getSequence()).getPullEvaluator();
        final BooleanEvaluator satisfiesEval = Elaborator.make(expr.getAction()).getBooleanEvaluator();
        final boolean some = expr.getOperator() == Token.SOME;
        final int slot = expr.getLocalSlotNumber();
        return context -> {
            SequenceIterator base = selectEval.eval(context);
            Item it;
            while ((it = base.next()) != null) {
                context.setLocalVariable(slot, it);
                if (some == satisfiesEval.eval(context)) {
                    base.close();
                    return some;
                }
            }
            return !some;
        };
    }

}


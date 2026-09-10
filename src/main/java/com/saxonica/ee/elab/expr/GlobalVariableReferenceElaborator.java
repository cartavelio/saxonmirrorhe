package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.GlobalVariableReference;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

/**
 * Elaborator for a global variable reference, for example {@code $globalVar}.
 */

public class GlobalVariableReferenceElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        GlobalVariableReference varRef = (GlobalVariableReference)getExpression();
        return context -> varRef.evaluateVariable(context).iterate();
    }

    @Override
    public PushEvaluator elaborateForPush() {
        GlobalVariableReference varRef = (GlobalVariableReference) getExpression();
        return (out, context) -> {
            SequenceIterator value = varRef.evaluateVariable(context).iterate();
            Item it;
            while ((it = value.next()) != null) {
                out.append(it);
            }
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        GlobalVariableReference varRef = (GlobalVariableReference) getExpression();
        return context -> varRef.evaluateVariable(context).head();
    }

}


package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

/**
 * Elaborator for a local variable reference, for example {@code $var}.
 */

public class LocalVariableReferenceElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        LocalVariableReference varRef = (LocalVariableReference)getExpression();
        int slot = varRef.getSlotNumber();
        return context -> context.evaluateLocalVariable(slot).iterate();
    }

    @Override
    public PushEvaluator elaborateForPush() {
        LocalVariableReference varRef = (LocalVariableReference) getExpression();
        int slot = varRef.getSlotNumber();
        return (out, context) -> {
            SequenceIterator value = context.evaluateLocalVariable(slot).iterate();
            Item it;
            while ((it = value.next()) != null) {
                out.append(it);
            }
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        LocalVariableReference varRef = (LocalVariableReference) getExpression();
        int slot = varRef.getSlotNumber();
        return context -> context.evaluateLocalVariable(slot).head();
    }

}


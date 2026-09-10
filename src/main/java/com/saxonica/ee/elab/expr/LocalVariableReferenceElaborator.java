package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.Elaborator;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public class LocalVariableReferenceElaborator extends Elaborator {

    @Override
    public PullEvaluator getPullEvaluator() {
        LocalVariableReference varRef = (LocalVariableReference)getExpression();
        int slot = varRef.getSlotNumber();
        return context -> context.evaluateLocalVariable(slot).iterate();
    }

    @Override
    public PushEvaluator getPushEvaluator() {
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
    public ItemEvaluator getItemEvaluator() {
        LocalVariableReference varRef = (LocalVariableReference) getExpression();
        int slot = varRef.getSlotNumber();
        return context -> context.evaluateLocalVariable(slot).head();
    }

}


package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceTool;

/**
 * The FallbackElaborator performs elaboration of expressions for which no more specific
 * elaborator is available. It produces evaluation functions that simply call the
 * direct evaluation interfaces. Note that when a FallbackElaborator is used, the
 * entire contained subtree will be interpreted in the traditional way, with no benefit
 * from elaboration.
 */

public class FallbackElaborator extends Elaborator {

    public FallbackElaborator() {
        
    }

    public FallbackElaborator(Expression expr) {
        super();
        setExpression(expr);
    }

    @Override
    public EagerEvaluator eagerly() {
        return context -> SequenceTool.toGroundedValue(getExpression().iterate(context));
    }


    public PullEvaluator elaborateForPull() {
        return context -> getExpression().iterate(context);
    }

    public PushEvaluator elaborateForPush() {
        return (out, context) -> getExpression().processLeavingTail(out, context);
    }

    public ItemEvaluator elaborateForItem() {
        return context -> getExpression().evaluateItem(context);
    }

    public BooleanEvaluator elaborateForBoolean() {
        return context -> getExpression().effectiveBooleanValue(context);
    }

    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        if (zeroLengthWhenAbsent) {
            return context -> getExpression().evaluateAsString(context);
        } else {
            return context -> {
                Item item = getExpression().evaluateItem(context);
                return item == null ? null : item.getUnicodeStringValue();
            };
        }
    }


}


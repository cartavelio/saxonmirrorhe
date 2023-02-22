package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;

public abstract class PullElaborator extends Elaborator {

    @Override
    public abstract PullEvaluator elaborateForPull();

    @Override
    public PushEvaluator elaborateForPush() {
        PullEvaluator pull = elaborateForPull();
        return (out, context) -> {
            SequenceIterator iter = pull.eval(context);
            Item it;
            while ((it = iter.next()) != null) {
                out.append(it);
            }
            return null;
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        PullEvaluator pull = elaborateForPull();
        return context -> pull.eval(context).next();
    }

    public BooleanEvaluator elaborateForBoolean() {
        PullEvaluator pull = elaborateForPull();
        return context -> ExpressionTool.effectiveBooleanValue(pull.eval(context));
    }

    @Override
    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        ItemEvaluator ie = elaborateForItem();
        return context -> {
            Item item = ie.eval(context);
            return item == null ? handleNullUnicodeString(zeroLengthWhenAbsent) : item.getUnicodeStringValue();
        };
    }


}


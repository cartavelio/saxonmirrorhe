package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.EmptySequence;

public abstract class ItemElaborator extends Elaborator {

    @Override
    public SequenceEvaluator eagerly() {
        boolean maybeEmpty = Cardinality.allowsZero(getExpression().getCardinality());
        ItemEvaluator ie = elaborateForItem();
        if (maybeEmpty) {
            return context -> {
                Item it = ie.eval(context);
                return it == null ? EmptySequence.getInstance() : it;
            };
        } else {
            //noinspection Convert2MethodRef
            return context -> ie.eval(context);
        }
    }

    @Override
    public PullEvaluator elaborateForPull() {
        ItemEvaluator ie = elaborateForItem();
        return context -> SingletonIterator.makeIterator(ie.eval(context));
    }

    @Override
    public PushEvaluator elaborateForPush() {
        ItemEvaluator ie = elaborateForItem();
        return (out, context) -> {
            Item it = ie.eval(context);
            if (it != null) {
                out.append(it);
            }
            return null;
        };
    }

    @Override
    public abstract ItemEvaluator elaborateForItem();

    public BooleanEvaluator elaborateForBoolean() {
        ItemEvaluator ie = elaborateForItem();
        return context -> ExpressionTool.effectiveBooleanValue(ie.eval(context));
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


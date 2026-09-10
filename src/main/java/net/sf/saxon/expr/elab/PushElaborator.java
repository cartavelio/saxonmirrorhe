package net.sf.saxon.expr.elab;

import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.EmptySequence;

public abstract class PushElaborator extends Elaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        Expression expr = getExpression();
        if (Cardinality.allowsMany(expr.getCardinality())) {
            PushEvaluator pushEval = elaborateForPush();
            return context -> {
                Controller controller = context.getController();
                assert controller != null;
                SequenceCollector seq = controller.allocateSequenceOutputter();
                TailCall tc = pushEval.processLeavingTail(new ComplexContentOutputter(seq), context);
                Expression.dispatchTailCall(tc);
                seq.close();
                return seq.iterate();
            };
        } else {
            ItemEvaluator itemEval = elaborateForItem();
            return context -> {
                Item item = itemEval.eval(context);
                return item == null ? EmptyIterator.getInstance() : SingletonIterator.makeIterator(item);
            };
        }
    }

    @Override
    public SequenceEvaluator eagerly() {
        Expression expr = getExpression();
        if (Cardinality.allowsMany(expr.getCardinality())) {
            PushEvaluator pushEval = elaborateForPush();
            return context -> {
                Controller controller = context.getController();
                assert controller != null;
                SequenceCollector seq = controller.allocateSequenceOutputter();
                TailCall tc = pushEval.processLeavingTail(new ComplexContentOutputter(seq), context);
                Expression.dispatchTailCall(tc);
                seq.close();
                return seq.getSequence();
            };
        } else {
            ItemEvaluator itemEval = elaborateForItem();
            return context -> {
                Item item = itemEval.eval(context);
                return item == null ? EmptySequence.getInstance() : item;
            };
        }
    }


    @Override
    public PushEvaluator elaborateForPush() {
        // Must be implemented in a subclass
        throw new UnsupportedOperationException();
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        PushEvaluator pushEval = elaborateForPush();
        return context -> {
            Controller controller = context.getController();
            assert controller != null;
            SequenceCollector seq = controller.allocateSequenceOutputter(1);
            TailCall tc = pushEval.processLeavingTail(new ComplexContentOutputter(seq), context);
            Expression.dispatchTailCall(tc);
            seq.close();
            return seq.getFirstItem();
        };
    }

    public BooleanEvaluator elaborateForBoolean() {
        PullEvaluator pullEval = elaborateForPull();
        return context -> {
            SequenceIterator iter = pullEval.iterate(context);
            return ExpressionTool.effectiveBooleanValue(iter);
        };
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


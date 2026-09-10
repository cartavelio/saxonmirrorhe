package com.saxonica.ee.elab.gen;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.NoDynamicContextException;

public abstract class ComplexNodePushElaborator extends FallbackElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        PushEvaluator pushEval = this.elaborateForPush();
        return context -> {
            Controller controller = context.getController();
            if (controller == null) {
                throw new NoDynamicContextException("No controller available");
            }
            SequenceCollector seq = controller.allocateSequenceOutputter(1);
            TailCall tc = pushEval.process(new ComplexContentOutputter(seq), context);
            Expression.dispatchTailCall(tc);
            seq.close();
            SequenceIterator result = seq.iterate();
            seq.reset();
            return result;
        };
    }

    @Override
    public ItemEvaluator elaborateForItem() {
        PushEvaluator pushEval = this.elaborateForPush();
        return context -> {
            Controller controller = context.getController();
            if (controller == null) {
                throw new NoDynamicContextException("No controller available");
            }
            SequenceCollector seq = controller.allocateSequenceOutputter(1);
            TailCall tc = pushEval.process(new ComplexContentOutputter(seq), context);
            Expression.dispatchTailCall(tc);
            seq.close();
            Item result = seq.getFirstItem();
            seq.reset();
            return result;
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        // Must be implemented in a subclass
        throw new UnsupportedOperationException();
    }



}


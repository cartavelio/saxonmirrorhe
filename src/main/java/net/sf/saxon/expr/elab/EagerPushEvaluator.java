package net.sf.saxon.expr.elab;

import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

public class EagerPushEvaluator implements SequenceEvaluator {

    final PushEvaluator pusher;

    public EagerPushEvaluator(PushEvaluator select) {
        this.pusher = select;
    }

    /**
     * Evaluate a construct to produce a value (which might be a lazily evaluated Sequence)
     *
     * @param context the evaluation context
     * @return a Sequence (not necessarily grounded)
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */
    @Override
    public Sequence evaluate(XPathContext context) throws XPathException {
        try {
            Controller controller = context.getController();
            SequenceCollector seq = controller.allocateSequenceOutputter();
            ComplexContentOutputter out = new ComplexContentOutputter(seq);
            out.open();
            TailCall tail = pusher.processLeavingTail(out, context);
            Expression.dispatchTailCall(tail);
            out.close();
            return seq.getSequence();
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }

}

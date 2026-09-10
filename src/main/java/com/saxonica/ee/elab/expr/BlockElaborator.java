package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AbstractBlockIterator;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

/**
 * Elaborator for a "Block", which is typically either an XPath sequence expression (a, b, c)
 * or an XSLT sequence constructor
 */
public class BlockElaborator extends PullElaborator {

    public PullEvaluator elaborateForPull() {
        final Block expr = (Block) getExpression();
        final Operand[] operanda = expr.getOperanda();
        final int size = operanda.length;
        final PullEvaluator[] actions = new PullEvaluator[size];
        for (int i = 0; i < size; i++) {
            actions[i] = makeElaborator(operanda[i].getChildExpression()).elaborateForPull();
        }
        return context -> new BlockIterator(actions, context);
    }

    private static class BlockIterator extends AbstractBlockIterator {

        private final PullEvaluator[] pullers;
        public BlockIterator(PullEvaluator[] pullers, XPathContext context) {
            this.pullers = pullers;
            init(pullers.length, context);
        }

        @Override
        public SequenceIterator getNthChildIterator(int n) throws XPathException {
            return pullers[n].eval(context);
        }
    }

    @Override
    public PushEvaluator elaborateForPush() {
        final Block expr = (Block) getExpression();
        final Operand[] operanda = expr.getOperanda();
        final int size = operanda.length;
        final PushEvaluator[] actions = new PushEvaluator[size];
        for (int i = 0; i < size; i++) {
            actions[i] = makeElaborator(operanda[i].getChildExpression()).elaborateForPush();
        }
        return (out, context) -> {
            TailCall tail = null;
            for (int i = 0; i < size; i++) {
                while (tail != null) {
                    tail = tail.processLeavingTail();
                }
                tail = actions[i].process(out, context);
            }
            return tail;
        };
    }
}


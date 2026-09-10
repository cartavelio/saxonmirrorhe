package net.sf.saxon.expr.elab;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.VariableReference;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.parser.Evaluators;
import net.sf.saxon.ma.zeno.ZenoSequence;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

public class SharedAppendEvaluator implements SequenceEvaluator {

    private Block.ChainAction[] actions;

    public SharedAppendEvaluator(Block expr) {
        actions = new Block.ChainAction[expr.size()];
        for (int i = 0; i < expr.size(); i++) {
            Expression child = expr.getOperanda()[i].getChildExpression();
            if (child instanceof VariableReference) {
                SequenceEvaluator eval = child.makeElaborator().eagerly();
                actions[i] = (chain, context) -> chain.appendSequence((GroundedValue) eval.evaluate(context));
            } else {
                PullEvaluator pull = child.makeElaborator().elaborateForPull();
                actions[i] = (chain, context) -> {
                    SequenceIterator iter = pull.iterate(context);
                    for (Item item; (item = iter.next()) != null; ) {
                        chain = chain.append(item);
                    }
                    return chain;
                };
            }
        }
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
        ZenoSequence chain = new ZenoSequence();
        for (Block.ChainAction action : actions) {
            chain = action.perform(chain, context);
        }
        return chain;
    }

    /**
     * Get a code identifying the evaluator. Note that the codes are held in SEF files
     * so they should be stable.
     */
    @Override
    public int getEvaluationMethodCode() {
        return Evaluators.SHARED_APPEND_EXPRESSION;
    }
}

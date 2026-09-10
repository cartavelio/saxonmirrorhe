package com.saxonica.ee.parallel;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;

/**
 * This instruction represents an xsl:for-each instruction using more than one thread
 */
public class MultithreadedForEach extends ForEach {


    public MultithreadedForEach(Expression select, Expression action, boolean containsTailCall, Expression threads) {
        super(select, action, false, threads);
        // allowing tail calls would only complicate things
    }

    @Override
    public boolean isMultiThreaded(Configuration config) {
        return true;
    }

    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        Expression e2 = super.optimize(visitor, contextInfo);
        if (e2 != this) {
            return e2;
        }
        Expression e = getParentExpression();
        while (e != null) {
            if (e instanceof LetExpression && ExpressionTool.dependsOnVariable(
                    getAction(), new Binding[]{(LetExpression) e})) {
                ((LetExpression) e).setNeedsEagerEvaluation(true);
            }
            e = e.getParentExpression();
        }
        return this;
    }

    @Override
    public Expression copy(RebindingMap rebindings) {
        MultithreadedForEach result = new MultithreadedForEach(
                getSelect().copy(rebindings), getAction().copy(rebindings), containsTailCall, getThreads());
        result.setInstruction(isInstruction());
        return result;
    }

    /**
     * An implementation of Expression must provide at least one of the methods evaluateItem(), iterate(), or process().
     * This method indicates which of these methods is provided. This implementation provides only the
     * process() method natively.
     */

    @Override
    public int getImplementationMethod() {
        return PROCESS_METHOD;
    }

    /**
     * Evaluate the value of the "threads" attribute, indicating how many threads should be used
     *
     * @param context the dynamic evaluation context
     * @return the number of threads to be used
     * @throws XPathException on an error, for example if the value of "threads" is not an integer
     */

    private int evaluateThreads(XPathContext context) throws XPathException {
        if (threadsOp == null) {
            return 1;
        } else {
            String threadsValue = getThreads().evaluateAsString(context).toString();
            int threads = 0;
            try {
                threads = Integer.parseInt(threadsValue.toString());
            } catch (NumberFormatException e) {
                XPathException err = new XPathException("Value of 'threads' is not an integer (" + threadsValue + ")");
                err.setLocation(getLocation());
                err.setXPathContext(context);
                throw err;
            }
            return threads;
        }
    }

    /**
     * Return an Iterator to iterate over the values of the sequence.
     *
     * @param context supplies the context for evaluation
     * @return a SequenceIterator that can be used to iterate over the result
     *         of the expression
     * @throws net.sf.saxon.trans.XPathException
     *          if any dynamic error occurs evaluating the
     *          expression
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return makeElaborator().elaborateForPull().iterate(context);
    }

    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in explain() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return "multithreadedForEach";
    }


    /**
     * Add the value of the saxon:threads attribute to the explain output
     *
     * @param out the destination for explain output
     */

    @Override
    protected void explainThreads(ExpressionPresenter out) throws XPathException {
        out.setChildRole("threads");
        getThreads().export(out);
    }

    /**
     * Make an elaborator for this expression
     *
     * @return a suitable elaborator
     */

    @Override
    public Elaborator getElaborator() {
        return new MultithreadedForEachElaborator();
    }

    public static class MultithreadedForEachElaborator extends PullElaborator {

        private StringEvaluator threadsEvaluator;

        private void makeThreadsEvaluator(MultithreadedForEach forEach) {
            if (forEach.threadsOp == null) {
                threadsEvaluator = context -> "1";
            } else {
                threadsEvaluator = forEach.getThreads().makeElaborator().elaborateForString(true);
            }
        }

        private int evaluateThreads(MultithreadedForEach forEach, XPathContext context) throws XPathException {
            if (forEach.threadsOp == null) {
                return 1;
            } else {
                String threadsValue = threadsEvaluator.eval(context);
                int threads;
                try {
                    threads = Integer.parseInt(threadsValue);
                } catch (NumberFormatException e) {
                    XPathException err = new XPathException("Value of 'threads' is not an integer (" + threadsValue + ")");
                    err.setLocation(forEach.getLocation());
                    err.setXPathContext(context);
                    throw err;
                }
                return threads;
            }
        }

        @Override
        public PullEvaluator elaborateForPull() {
            MultithreadedForEach forEach = (MultithreadedForEach) getExpression();
            PullEvaluator select = forEach.getSelectExpression().makeElaborator().elaborateForPull();
            PullEvaluator action = forEach.getActionExpression().makeElaborator().elaborateForPull();
            makeThreadsEvaluator(forEach);

            return context -> {
                int threads = evaluateThreads(forEach, context);
                XPathContextMinor c2 = context.newMinorContext();
                c2.trackFocus(select.iterate(context));
                return new MultithreadedContextMappingIterator(action, c2, threads);
            };
        }

        @Override
        public UpdateEvaluator elaborateForUpdate() {
            throw new UnsupportedOperationException("Multi-threading is not supported for updating expressions");
        }
    }
}

// Copyright (c) 2010-2022 Saxonica Limited


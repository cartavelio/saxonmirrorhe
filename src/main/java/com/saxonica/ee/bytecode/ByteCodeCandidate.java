package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CompilerService;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

/**
 * A ByteCodeCandidate is an expression on the expression tree which, if evaluated, returns the value
 * of its single operand. As a side-effect, it counts how many times it has been evaluated, and when
 * a certain threshold is reached, it generates bytecode for its operand. In future evaluations thereafter
 * the expression is evaluated by executing the bytecode rather than by using the interpreter.
 *
 * <p>Note, after the operand of the ByteCodeCandidate is compiled, the ByteCodeCandidate remains
 * on the expression tree. We experimented with removing it from the tree, but run-time structural
 * changes to the expression tree cause problems, for example when evaluating a closure which saves
 * a reference to the tree.</p>
 */
public class ByteCodeCandidate extends UnaryExpression  {

    private final String objectName;
    private long countDown = 100;
    private final ExpressionOwner owner;
    private NamespaceResolver namespaceResolver = null;
    private Expression targetExpression = null;
    private final int id;
    private ByteCodeMonitor byteCodeMonitor;
    private static int nextId = 0;
    private final int requiredEvaluationModes;
    public int compiledMode;

    public ByteCodeCandidate(ExpressionOwner owner, Expression child, String objectName, int requiredEvaluationModes) {
        super(child);
        this.owner = owner;
        this.objectName = objectName;
        this.id = nextId++;
        this.requiredEvaluationModes = requiredEvaluationModes;
        //System.err.println("BC Candidate: " + child);
    }

    public int getId() {
        return id;
    }

    public void setCountDown(int val) {
        countDown = val;
    }

    public long getCountDown() {
        return countDown;
    }

    @Override
    protected OperandRole getOperandRole() {
        return OperandRole.SAME_FOCUS_ACTION;
    }

    public void setByteCodeMonitor(ByteCodeMonitor monitor) {
        this.byteCodeMonitor = monitor;
    }

    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo) throws XPathException {
        if (visitor.isOptimizeForStreaming()) {
            // Bytecode is not used when streaming
            return getBaseExpression().optimize(visitor, contextInfo);
        }
        return super.optimize(visitor, contextInfo);
    }

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        getBaseExpression().export(out);
    }


    /**
     * Determine whether this is a vacuous expression as defined in the XQuery update specification
     *
     * @return true if this expression is vacuous
     */

    @Override
    public boolean isVacuousExpression() {
        return getBaseExpression().isVacuousExpression();
    }

    @Override
    public boolean isUpdatingExpression() {
        return getBaseExpression().isUpdatingExpression();
    }

//    @Override
//    public void evaluatePendingUpdates(XPathContext context, PendingUpdateList pul) throws XPathException {
//        getBaseExpression().evaluatePendingUpdates(context, pul);
//    }

    @Override
    public int getImplementationMethod() {
        return getBaseExpression().getImplementationMethod();
    }


    @Override
    public Expression copy(RebindingMap rebindings) {
        ByteCodeCandidate t = new ByteCodeCandidate(owner, getBaseExpression().copy(rebindings), objectName, requiredEvaluationModes);
        ExpressionTool.copyLocationInfo(this, t);
        t.namespaceResolver = namespaceResolver;
        t.setByteCodeMonitor(byteCodeMonitor);
        return t;
    }

    @Override
    public boolean effectiveBooleanValue(XPathContext context) throws XPathException {
        Expression body = targetExpression(context, Expression.EFFECTIVE_BOOLEAN_VALUE);
        if (byteCodeMonitor != null) {
            long time = System.nanoTime();
            boolean b = body.effectiveBooleanValue(context);
            time = System.nanoTime() - time;
            byteCodeMonitor.logExecution(this, "ebv", time);
            return b;
        } else {
            return body.effectiveBooleanValue(context);
        }
    }

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        Expression body = targetExpression(context, Expression.PROCESS_METHOD);

        try {
            if (byteCodeMonitor != null) {
                long time = System.nanoTime();
                body.process(output, context);
                time = System.nanoTime() - time;
                byteCodeMonitor.logExecution(this, "process", time);
            } else {
                body.process(output, context);
            }

        } catch (XPathException e) {
            e.maybeSetFailingExpression(this);
            e.maybeSetContext(context);
            throw e;
        } catch (UncheckedXPathException e1) {
            XPathException xe = e1.getXPathException();
            xe.maybeSetFailingExpression(this);
            xe.maybeSetContext(context);
            throw xe;
        } catch (Exception e2) {
            Location loc = getLocation();
            String message = "Internal error evaluating byte code candidate "
                    + (loc.getLineNumber() > 0 ? " at line " + loc.getLineNumber() : "")
                    + (loc.getSystemId() != null ? " in module " + loc.getSystemId() : "");
            e2.printStackTrace();
            throw new RuntimeException(message, e2);
        }
    }

//    @Override
//    public TailCall processLeavingTail(Outputter output, XPathContext context) throws XPathException { // bug3838
//        Expression body = targetExpression(context, Expression.PROCESS_METHOD);
//        if (body instanceof TailCallReturner) {
//            if (byteCodeMonitor != null) {
//                long time = System.nanoTime();
//                TailCall tc = ((TailCallReturner) body).processLeavingTail(output, context);
//                // if tc==null, we're returning a tail-call, so no real work has been done, so
//                // there's no point capturing the timing.
//                // Also, if the body returns a tail-call, then we don't want to generate bytecode,
//                // because the bytecode won't be a TailCallReturner. So forcibly suppress bytecode generation.
//                if (tc != null) {
//                    countDown = Integer.MAX_VALUE;
//                    return tc;
//                }
//                time = System.nanoTime() - time;
//                byteCodeMonitor.logExecution(this, "process", time);
//                return null;
//            } else {
//                TailCall tc = ((TailCallReturner) body).processLeavingTail(output, context);
//                if (tc != null) {
//                    // If the body returns a tail-call, then we don't want to generate bytecode,
//                    // because the bytecode won't be a TailCallReturner. So forcibly suppress bytecode generation.
//                    countDown = Integer.MAX_VALUE;
//                }
//                return tc;
//            }
//        } else {
//            process(output, context);
//            return null;
//        }
//    }


    /**
     * Get the operand expression to be evaluated
     * @param context the evaluation context
     * @param evaluationMethods indicates the mode of evaluation, eg iterate, process, or evaluateItem
     * @return the expression to be evaluated (typically either the interpreted or bytecode form of
     * the operand expression)
     */

    private Expression targetExpression(XPathContext context, int evaluationMethods) {
        if (targetExpression != null) {
            return targetExpression;
        }
        if (context instanceof EarlyEvaluationContext) {
            return getBaseExpression();
        }
        if (--countDown <= 0) {
            synchronized(this) {
                if (targetExpression != null) {
                    return targetExpression;
                }
                evaluationMethods |= requiredEvaluationModes;
                Configuration config = context.getConfiguration();
                long startTime = 0;
                if (byteCodeMonitor != null) {
                    startTime = System.nanoTime();
                    config.getLogger().info(
                            "Generating hotspot byte code (" + evaluationMethodName(evaluationMethods) + ") for {" +
                                    getBaseExpression().toShortString() +
                                    "} at " + getBaseExpression().getLocation().getSystemId() +
                                    "#" + getBaseExpression().getLocation().getLineNumber());
                    compiledMode = evaluationMethods;
                }

                Optimizer opt = config.obtainOptimizer();
                HostLanguage lang = getPackageData().getHostLanguage();
                CompilerService compilerService = (CompilerService) config.makeCompilerService(lang);
                Expression body = getBaseExpression();
                Expression temp = opt.compileToByteCode(compilerService, body, body.getExpressionName(), evaluationMethods);

                if (byteCodeMonitor != null) {
                    long totalTime = System.nanoTime() - startTime;
                    byteCodeMonitor.logCompilation(this, totalTime);
                }
                if (temp != null) {
                    targetExpression = temp;
                } else {
                    // compilation failed: don't try again
                    targetExpression = body;
                }
            }
            return targetExpression;
        } else {
            return getBaseExpression();
        }

    }

    private String evaluationMethodName(int method) {
        String s = "";
        if ((method & Expression.EFFECTIVE_BOOLEAN_VALUE) != 0) {
            s += "EBV ";
        }
        if ((method & Expression.EVALUATE_METHOD) != 0) {
            s += "EVAL ";
        }
        if ((method & Expression.ITERATE_METHOD) != 0) {
            s += "ITER ";
        }
        if ((method & Expression.PROCESS_METHOD) != 0) {
            s += "PROC ";
        }
        return s.trim();
    }

    /**
     * Evaluate an expression as a single item. This always returns either a single Item or
     * null (denoting the empty sequence). No conversion is done. This method should not be
     * used unless the static type of the expression is a subtype of "item" or "item?": that is,
     * it should not be called if the expression may return a sequence. There is no guarantee that
     * this condition will be detected.
     *
     * @param context The context in which the expression is to be evaluated
     * @return the node or atomic value that results from evaluating the
     * expression; or null to indicate that the result is an empty
     * sequence
     * @throws net.sf.saxon.trans.XPathException if any dynamic error occurs evaluating the
     *                                           expression
     */

    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        Expression body = targetExpression(context, Expression.EVALUATE_METHOD);
        if (byteCodeMonitor != null) {
            long time = System.nanoTime();
            Item it = body.evaluateItem(context);
            time = System.nanoTime() - time;
            byteCodeMonitor.logExecution(this, "eval", time);
            return it;
        } else {
            return body.evaluateItem(context);
        }
    }

    /**
     * Return an Iterator to iterate over the values of a sequence. The value of every
     * expression can be regarded as a sequence, so this method is supported for all
     * expressions. This default implementation handles iteration for expressions that
     * return singleton values: for non-singleton expressions, the subclass must
     * provide its own implementation.
     *
     * @param context supplies the context for evaluation
     * @return a SequenceIterator that can be used to iterate over the result
     * of the expression
     * @throws net.sf.saxon.trans.XPathException if any dynamic error occurs evaluating the
     *                                           expression
     */

     /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        Expression body = targetExpression(context, Expression.ITERATE_METHOD);
        if (byteCodeMonitor != null) {
            long time = System.nanoTime();
            SequenceIterator iter = body.iterate(context);
            time = System.nanoTime() - time;
            byteCodeMonitor.logExecution(this, "iterate", time);
            return iter;
        } else {
            return body.iterate(context);
        }
    }

    /**
     * Make an elaborator for this expression
     *
     * @return an appropriate {@link Elaborator}
     */
    @Override
    public Elaborator getElaborator() {
        return new ByteCodeCandidateElaborator();
    }

    private static class ByteCodeCandidateElaborator extends PushElaborator {

        Expression interpretedBody;
        PullEvaluator bodyPull;
        PushEvaluator bodyPush;
        ItemEvaluator bodyItem;
        BooleanEvaluator bodyBool;

        @Override
        public PullEvaluator elaborateForPull() {
            ByteCodeCandidate expr = (ByteCodeCandidate) getExpression();
            interpretedBody = expr.getBaseExpression();
            bodyPull = expr.getBaseExpression().makeElaborator().elaborateForPull();
            return context -> {
                Expression body = expr.targetExpression(context, Expression.ITERATE_METHOD);
                if (body != interpretedBody) {
                    bodyPull = body.makeElaborator().elaborateForPull();
                }
                if (expr.byteCodeMonitor != null) {
                    long time = System.nanoTime();
                    SequenceIterator iter = bodyPull.iterate(context);
                    time = System.nanoTime() - time;
                    expr.byteCodeMonitor.logExecution(expr, "iterate", time);
                    return iter;
                } else {
                    return bodyPull.iterate(context);
                }
            };
        }

        @Override
        public PushEvaluator elaborateForPush() {
            ByteCodeCandidate expr = (ByteCodeCandidate) getExpression();
            interpretedBody = expr.getBaseExpression();
            bodyPush = expr.getBaseExpression().makeElaborator().elaborateForPush();
            return (output, context) -> {
                Expression body = expr.targetExpression(context, Expression.PROCESS_METHOD);
                if (body != interpretedBody) {
                    bodyPush = body.makeElaborator().elaborateForPush();
                }
                if (expr.byteCodeMonitor != null) {
                    long time = System.nanoTime();
                    Expression.dispatchTailCall(bodyPush.processLeavingTail(output, context));
                    time = System.nanoTime() - time;
                    expr.byteCodeMonitor.logExecution(expr, "process", time);
                } else {
                    Expression.dispatchTailCall(bodyPush.processLeavingTail(output, context));
                }
                return null;
            };
        }

        public ItemEvaluator elaborateForItem() {
            ByteCodeCandidate expr = (ByteCodeCandidate) getExpression();
            interpretedBody = expr.getBaseExpression();
            bodyItem = expr.getBaseExpression().makeElaborator().elaborateForItem();
            return context -> {
                Expression body = expr.targetExpression(context, Expression.EVALUATE_METHOD);
                if (body != interpretedBody) {
                    bodyItem = body.makeElaborator().elaborateForItem();
                }
                if (expr.byteCodeMonitor != null) {
                    long time = System.nanoTime();
                    Item result = bodyItem.eval(context);
                    time = System.nanoTime() - time;
                    expr.byteCodeMonitor.logExecution(expr, "evaluate", time);
                    return result;
                } else {
                    return bodyItem.eval(context);
                }
            };
        }

        @Override
        public BooleanEvaluator elaborateForBoolean() {
            ByteCodeCandidate expr = (ByteCodeCandidate) getExpression();
            interpretedBody = expr.getBaseExpression();
            bodyBool = expr.getBaseExpression().makeElaborator().elaborateForBoolean();
            return context -> {
                Expression body = expr.targetExpression(context, Expression.EFFECTIVE_BOOLEAN_VALUE);
                if (body != interpretedBody) {
                    bodyBool = body.makeElaborator().elaborateForBoolean();
                }
                if (expr.byteCodeMonitor != null) {
                    long time = System.nanoTime();
                    boolean result = bodyBool.eval(context);
                    time = System.nanoTime() - time;
                    expr.byteCodeMonitor.logExecution(expr, "evaluate", time);
                    return result;
                } else {
                    return bodyBool.eval(context);
                }
            };
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited

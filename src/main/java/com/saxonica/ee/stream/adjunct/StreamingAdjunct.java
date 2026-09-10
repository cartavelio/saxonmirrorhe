package com.saxonica.ee.stream.adjunct;

import com.saxonica.ee.stream.*;
import com.saxonica.ee.stream.feed.BufferingFeed;
import com.saxonica.ee.stream.feed.CallableFeed;
import com.saxonica.ee.stream.feed.FeedMaker;
import com.saxonica.ee.stream.feed.FeedMakerDelegate;
import com.saxonica.ee.stream.watch.WatchMaker;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.sort.DocumentSorter;
import net.sf.saxon.functions.ContextItemAccessorFunction;
import net.sf.saxon.functions.IntegratedFunctionCall;
import net.sf.saxon.functions.Reverse;
import net.sf.saxon.om.Function;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.FunctionStreamability;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpInnerClass;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.Cardinality;

import java.util.HashMap;
import java.util.List;
import java.util.Map;


/**
 * This abstract class supplements an {@link Expression} with methods used at compile-time
 * to support streaming, by generating a {@link com.saxonica.ee.stream.watch.Watch}
 * that allows the expression to be evaluated in push mode.
 * <p>The default implementation is adequate only for expressions that take single items as their
 * input. For other kinds of expression, either a WatchMaker or FeedMaker, or both, need to be supplied.
 * If the input sequence is atomized (as with the sum() function), then only a FeedMaker is required;
 * a TypedValueWatch will automatically be added to the Streaming Route to perform atomization of
 * the supplied sequence. For expressions that do not perform atomization, both a WatchMaker (for use
 * when the input is striding or consuming) and a FeedMaker (for use when it is climbing or grounded)
 * need to be provided.</p>
 * <p>Neither method should be called in cases where streamability analysis has decided that the
 * expression is not streamable. In such cases the methods may throw an exception.</p>
 * <p>There is a fallback implementation that allows any expression to be evaluated as a Feed, but it
 * involves buffering the input in memory, and performing an implicit copy-of() to create a grounded
 * copy, so it is only really suitable for the case where the input is a single atomic value.</p>
 */

public class StreamingAdjunct {

    private static final Map<String, Class<? extends StreamingAdjunct>> adjunctMap = emptyAdjunctMap();

    @CSharpReplaceBody(code="return new System.Collections.Generic.Dictionary<string, System.Type>();")
    public static Map<String, Class<? extends StreamingAdjunct>> emptyAdjunctMap() {
        // written this way to enable C# transpilation
        return new HashMap<>(50);
    }

    private static void map(String s, Class<? extends StreamingAdjunct> a) {
        adjunctMap.put(s, a);
    }

    static {
        map("AdjacentTextNodeMerger", AdjacentTextNodeMergerAdjunct.class);
        map("ApplyImports", ApplyImportsAdjunct.class);
        map("ApplyTemplates", ApplyTemplatesAdjunct.class);
        map("ArrayBlock", ArrayBlockAdjunct.class);
        map("AtomicSequenceConverter", AtomicSequenceConverterAdjunct.class);
        map("Atomizer", AtomizerAdjunct.class);
        map("AxisExpression", AxisExpressionAdjunct.class);
        map("Block", BlockAdjunct.class);
        map("CallTemplate", CallTemplateAdjunct.class);
        map("CardinalityChecker", CardinalityCheckerAdjunct.class);
        map("Choose", ChooseAdjunct.class);
        map("ConditionalBlock", ConditionalBlockAdjunct.class);
        map("ContextItemExpr", ContextItemExprAdjunct.class);
        map("Copy", CopyAdjunct.class);
        map("CopyOf", CopyOfAdjunct.class);
        map("CurrentGroup", CurrentGroupStreamer.class);
        map("DocumentInstr", DocumentInstrAdjunct.class);
        map("DocumentSorterAdjunct", DocumentSorterAdjunct.class);
        map("ElementCreator", ElementCreatorAdjunct.class);
        map("EmptyTextNodeRemover", EmptyTextNodeRemoverAdjunct.class);
        map("FilterExpression", FilterExpressionAdjunct.class);
        map("FirstItemExpression", FirstItemExpressionAdjunct.class);
        map("ForEach", ForEachAdjunct.class);
        map("ForEachGroup", ForEachGroupAdjunct.class);
        map("ForExpression", ForExpressionAdjunct.class);
        map("Fork", ForkAdjunct.class);
        map("GeneralComparison", GeneralComparisonAdjunct.class);
        map("InstanceOf", InstanceOfAdjunct.class);
        map("IsLastExpr", IsLastExprAdjunct.class);
        map("ItemChecker", ItemCheckerAdjunct.class);
        map("Iterate", IterateAdjunct.class);
        map("Literal", LiteralAdjunct.class);
        map("MergeInstr", MergeInstrAdjunct.class);
        map("NextIteration", NextIterationAdjunct.class);
        map("NextMatch", NextMatchAdjunct.class);
        map("OnEmpty", OnEmptyAdjunct.class);
        map("OnNonEmpty", OnNonEmptyAdjunct.class);
        map("RootExpression", RootExpressionAdjunct.class);
        map("ResultDocument", ResultDocumentAdjunct.class);
        map("SequenceInstr", SequenceInstrAdjunct.class);
        map("SimpleNodeConstructor", SimpleNodeConstructorAdjunct.class);
        map("SingletonAtomizer", SingletonAtomizerAdjunct.class);
        map("SlashExpression", ForEachAdjunct.class);
        map("SortExpression", SortExpressionAdjunct.class);
        map("SubscriptExpression", SubscriptExpressionAdjunct.class);
        map("SwitchExpression", SwitchAdjunct.class);
        map("TailExpression", TailExpressionAdjunct.class);
        map("TraceExpr", TraceExprAdjunct.class);
        map("TryCatch", TryCatchAdjunct.class);
        map("UseAttributeSet", UseAttributeSetAdjunct.class);
        map("VariableReference", VariableReferenceAdjunct.class);
        map("VennExpression", VennExpressionAdjunct.class);
        map("WherePopulated", WherePopulatedAdjunct.class);
    }

    private Configuration config;
    private Expression expression;

    /**
     * Make a streaming adjunct for a particular expression.
     *
     * @param config the Saxon Configuration
     * @param exp    the expression to be compiled
     * @return the compiler for this kind of expression
     */

    public static StreamingAdjunct makeStreamingAdjunct(Configuration config, final Expression exp) {
        StreamingAdjunct adj = createStreamingAdjunct(config, exp);
        adj.setConfiguration(config);
        adj.setExpression(exp);
        return adj;
    }

    private static StreamingAdjunct createStreamingAdjunct(Configuration config, final Expression exp) {
        if (exp instanceof DocumentSorter) {
            // The DocumentSorterAdjunct is designed to handle ancestors arriving in axis order, not in reverse axis order. If
            // a reverse() call has been wrapped around the ancestor axis, remove it.
            Expression operand = ((DocumentSorter)exp).getBaseExpression();
            if (operand instanceof SlashExpression && ((SlashExpression) operand).getActionExpression().isCallOn(Reverse.class)) {
                ((SlashExpression) operand).setStep(((SystemFunctionCall) ((SlashExpression) operand).getActionExpression()).getArg(0));
            }
            return new DocumentSorterAdjunct();
        } else if (exp instanceof IntegratedFunctionCall) {
            Object adjunct = ((IntegratedFunctionCall)exp).getFunction().getStreamingImplementation();
            if (adjunct instanceof StreamingAdjunct) {
                return (StreamingAdjunct) adjunct;
            }
        } else if (exp instanceof LetExpression) {
            Expression action = ((LetExpression)exp).getAction();
            if (Streamability.getPostureAndSweepIfKnown(action) != null && Streamability.getSweep(action) == Sweep.CONSUMING) {
                return new LetExpressionAdjunctB();
            } else {
                return new LetExpressionAdjunct();
            }
        } else if (exp instanceof StreamInstr) {
            return adjunctForStreamInstr((StreamInstr) exp);
        } else if (exp instanceof SystemFunctionCall) {
            SystemFunctionCall call = (SystemFunctionCall)exp;
            String streamerName = call.getTargetFunction().getStreamerName();
            StreamingAdjunct sa = StreamerMap.getStreamingAdjunct(streamerName);
            if (sa == null) {
                if (call.getTargetFunction() instanceof ContextItemAccessorFunction) {
                    Expression e2 = ((ContextItemAccessorFunction) call.getTargetFunction()).makeContextItemExplicit();
                    return makeStreamingAdjunct(config, e2);
                }
                return new StreamingAdjunct();
            } else {
                return sa;
            }
        } else if (exp instanceof UserFunctionCall) {
            UserFunctionCall ufc = (UserFunctionCall)exp;
            FunctionStreamability ability = ufc.getFunction().getDeclaredStreamability();
            if (ability == FunctionStreamability.UNCLASSIFIED) {
                // fall through
            } else if (ability == FunctionStreamability.FILTER) {
                return new FilterFunctionCallAdjunct();
            } else if (ability == FunctionStreamability.ABSORBING) {
                if (Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null &&
                        Streamability.getPosture(ufc.getArg(0)) == Posture.GROUNDED) {
                    return new StreamingAdjunct();
                } else {
                    return new AbsorbingFunctionCallAdjunct();
                }
            } else if (ability == FunctionStreamability.SHALLOW_DESCENT) {
                if (Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null &&
                        Streamability.getPosture(ufc.getArg(0)) == Posture.GROUNDED) {
                    return new StreamingAdjunct();
                } else {
                    return new ShallowDescentFunctionCallAdjunct();
                }

            } else if (ability == FunctionStreamability.DEEP_DESCENT) {
                if (Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null &&
                        Streamability.getPosture(ufc.getArg(0)) == Posture.GROUNDED) {
                    return new StreamingAdjunct();
                } else {
                    return new DeepDescentFunctionCallAdjunct();
                }

            } else if (ability == FunctionStreamability.ASCENT) {
                if (Streamability.getPostureAndSweepIfKnown(ufc.getArg(0)) != null &&
                        Streamability.getPosture(ufc.getArg(0)) == Posture.GROUNDED) {
                    return new StreamingAdjunct();
                } else {
                    return new AscentFunctionCallAdjunct();
                }

            } else {
                return new UserFunctionCallAdjunct();
            }
        }

        if (exp.getStreamerName() != null) {
            Class<? extends StreamingAdjunct> aClass = adjunctMap.get(exp.getStreamerName());
            if (aClass == null) {
                throw new AssertionError("No streaming support for " + exp.getStreamerName());
            }
            try {
                return aClass.newInstance();
            } catch (Exception e) {
                throw new AssertionError(e);
            }
        }
        return new StreamingAdjunct();
    }

    @CSharpInnerClass(outer=false, extra={"Saxon.Eej.ee.stream.StreamInstr stream"})
    private static StreamingAdjunct adjunctForStreamInstr(final StreamInstr stream) {
        return new StreamingAdjunct() {
            @Override
            public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
                Streamability.getStreamability(stream.getHref(), contextInfo, reasons);
                return new PostureAndSweep(Posture.GROUNDED, Streamability.getSweep(stream.getHref()));
            }
        };
    }

    /**
     * Set the Saxon Configuration
     *
     * @param config the Configuration
     */

    public void setConfiguration(Configuration config) {
        this.config = config;
    }

    /**
     * Get the Saxon Configuration
     *
     * @return the configuration
     */

    public Configuration getConfiguration() {
        return config;
    }

    /**
     * Set the expression being processed by this StreamingAdjunct
     *
     * @param expression the expression being processed
     */

    public void setExpression(Expression expression) {
        this.expression = expression;
    }

    /**
     * Get the expression being processed by this StreamingAdjunct
     *
     * @return the expression being processed
     */

    public Expression getExpression() {
        return expression;
    }

    /**
     * Compute the posture and sweep of this expression as defined in the W3C streamability specifications.
     * This must be implemented in all subclasses that don't use the general streamability rules. The implementation
     * should have the side-effect of computing the streamability of all subexpressions, recursively.
     *
     * @param contextInfo     Information about the context item type and posture
     * @param reasons         the caller may supply a list, in which case the implementation may add to this; the
     *   contents are human-readable messages explaining why the expression is not streamable.
     * @return the posture and sweep of the expression
     */

    public PostureAndSweep computeStreamability(ContextItemStaticInfoEE contextInfo, List<String> reasons) {
        Expression exp = getExpression();
        Iterable<Operand> ops = exp.operands();
        return Streamability.generalStreamabilityRules(exp, ops, contextInfo, reasons);
    }

    /**
     * Find which operand of an expression is the consuming operand. Helper method for use by subclasses
     * @param expression the expression
     * @param knownConsumingArg index of the consuming operand if known, otherwise -1
     * @return index identifying the consuming operand
     * @throws XPathException if there is no consuming operand
     */

    public static int findConsumingArg(Expression expression, int knownConsumingArg) throws XPathException {
        if (knownConsumingArg == -1) {
            int i = 0;
            int arg = 0;
            for (Operand o : expression.operands()) {
                if (Streamability.getSweep(o.getChildExpression()) == Sweep.CONSUMING) {
                    arg = i;
                }
                i++;
            }
            if (arg == -1) {
                throw new XPathException("No consuming subexpression found");
            }
            return arg;
        } else {
            return knownConsumingArg;
        }
    }

    /**
     * Convert this expression to a streaming pattern (a pattern used internally to match nodes during
     * push processing of an event stream)
     *
     * @param config the Saxon configuration
     * @return the equivalent pattern if conversion succeeds; otherwise null
     */

    public Pattern toStreamingPattern(Configuration config) {
        return null;
    }


    /**
     * Get a WatchMaker, a factory class that creates a Watch to evaluate this expression
     * in streaming mode. This method will only be called if the getImplementationMethod()
     * method of the expression includes the value {@link Expression#WATCH_METHOD}
     *
     * @return a Watchmaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @throws XPathException in the event of a failure
     * @param forGrouping true if the selected items are being grouped
     */

    public WatchMaker getWatchMaker(boolean forGrouping) throws XPathException {
        Posture posture = Streamability.getPosture(getExpression());
        if (posture == Posture.STRIDING || posture == Posture.CRAWLING) {
            throw new XPathException("No watch implemented for " + getExpression().getExpressionName(),
                                     SaxonErrorCode.SXST0067, getExpression().getLocation());
        }
        return null;
    }

    /**
     * Get a FeedMaker, a factory class that creates a Feed to evaluate this expression
     * in streaming mode.  In many cases the StreamingAdjunct is itself a FeedMaker, in which
     * case it returns itself. But where an expression has several operands, it will typically
     * create different feeds depending on which argument is the streamed input, and in such cases
     * the individual StreamingAdjunct makes the decision which feed to use, by returning an
     * appropriate FeedMaker.
     *
     * @param arg identifies the argument with respect to which the expression is to be inverted:
     *            usually 0 to indicate the first argument is the value that is streamed. Argument numbering
     *            is the same as the sequence of arguments/operands returned by the method {@link net.sf.saxon.expr.Expression#operands}
     * @return a FeedMaker to evaluate the expression, or null if it is not streamable with respect
     *         to the specificed argument.
     * @throws XPathException in the event of a failure
     */

    /*@Nullable*/
    public FeedMaker getFeedMaker(final int arg) throws XPathException {

        // If the class is itself a feedmaker, then return it.

        if (this instanceof FeedMaker) {
            return (FeedMaker) this;
        }

        // Otherwise return a default feed maker, which returns a BufferingFeed.
        // This is a generic implementation, for expressions that have no intrinsic capability
        // to operate in streamed mode. It replaces the subexpression that accesses streamed data
        // with a SuppliedParameterReference taking a value from the top of the context stack,
        // and returns a BufferingFeed which accumulates the streamed data and saves it to the context stack,
        // from where the expression can read it.
        return getDefaultFeedMaker(arg);

    }

    /**
     * Get a FeedMaker which will generate a feed to handle streamed input of operand "arg".
     * @param arg identifies which operand of the expression is the streamed (consuming) input
     * @return a FeedMaker
     */

    protected FeedMaker getDefaultFeedMaker(final int arg) {
        boolean allowsMany = false;
        int n = 0;
        Operand streamingOp = null;

        // Find out whether the relevant operand of the expression allows a sequence as its value
        for (Operand o : getExpression().operands()) {
            if (n++ == arg) {
                allowsMany = Cardinality.allowsMany(o.getRequiredType().getCardinality());
                streamingOp = o;
                break;
            }
        }

        if (getExpression() instanceof StaticFunctionCall && !allowsMany) {
            final Function fn = ((StaticFunctionCall)getExpression()).getTargetFunction();
            final Expression[] args = ((StaticFunctionCall) getExpression()).getArguments();
            return new FeedMakerDelegate((watchManager, out, context) -> new CallableFeed(fn, out, context, args));
        }

        // Fallback code for an expression that accepts a sequence as its streamed input, but has no
        // genuine streamed implementation. Generally used only for expressions such as arithmetic expressions
        // where the operands are singletons. (Code rewritten in 9.7 to fix bug 2320.)

        assert streamingOp != null;
        Expression proxy = streamingOp.getChildExpression();
        ConsumingOperand co = new ConsumingOperand(proxy);
        streamingOp.setChildExpression(co);
        co.getOperand().setOperandRole(new OperandRole(0, streamingOp.getUsage()));
        Streamability.setPostureAndSweep(co, Streamability.getPostureAndSweepIfKnown(proxy));

        return new FeedMakerDelegate((watchManager, out, context) -> new BufferingFeed(getExpression(), out, context));
    }


}


// Copyright (c) 2010-2022 Saxonica Limited
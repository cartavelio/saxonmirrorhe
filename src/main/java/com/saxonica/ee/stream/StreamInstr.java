package com.saxonica.ee.stream;

import com.saxonica.ee.parallel.PushToPull;
import com.saxonica.ee.parallel.Pusher;
import com.saxonica.ee.stream.watch.StreamWatch;
import com.saxonica.ee.stream.watch.WatchManager;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.PullEvaluator;
import net.sf.saxon.expr.elab.PushElaborator;
import net.sf.saxon.expr.elab.PushEvaluator;
import net.sf.saxon.expr.instruct.SourceDocument;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.functions.DocumentFn;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.QuitParsingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.value.AtomicValue;

import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.List;

/**
 * Implementation of the xsl:source-document (streamable="yes") instruction
 */

public class StreamInstr extends SourceDocument implements Pusher {

    private Inversion action;

    public StreamInstr(Expression hrefExp, Expression body, ParseOptions options) {
        super(hrefExp, body, options);
    }


    /**
     * Perform optimisation of an expression and its subexpressions. This is the third and final
     * phase of static optimization.
     * <p>This method is called after all references to functions and variables have been resolved
     * to the declaration of the function or variable, and after all type checking has been done.</p>
     *
     * @param visitor         an expression visitor
     * @param contextItemType the static type of "." at the point where this expression is invoked.
     *                        The parameter is set to null if it is known statically that the context item will be undefined.
     *                        If the type of the context item is not known statically, the argument is set to
     *                        {@link net.sf.saxon.type.Type#ITEM_TYPE}
     * @return the original expression, rewritten if appropriate to optimize execution
     * @throws net.sf.saxon.trans.XPathException
     *          if an error is discovered during this phase
     *          (typically a type error)
     */
    @Override
    public Expression optimize(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        ContextItemStaticInfo newType =
                getConfiguration().makeContextItemStaticInfo(NodeKindTest.DOCUMENT, false);
        newType.setContextPostureStriding();

        hrefOp.optimize(visitor, contextItemType);
        boolean previouslyStreaming = visitor.isOptimizeForStreaming();
        visitor.setOptimizeForStreaming(true);
        bodyOp.optimize(visitor, newType);
        visitor.setOptimizeForStreaming(previouslyStreaming);
        return this;
    }

    public Expression prepareForStreaming(PackageData pack) throws XPathException {
        Configuration config = pack.getConfiguration();
        int localLicense = pack.getLocalLicenseId();
        /*if (localLicense >= 0) {
            // we're dealing with a reloaded package, so all the preliminary checking has already been done
            if (config.isFeatureAllowedBySecondaryLicense(localLicense, Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
                Streamability.getStreamability(getBody(), ContextItemStaticInfoEE.DEFAULT, new ArrayList<>());
            }
        } else { */
            List<String> reasons = new ArrayList<>();
            Expression doc = checkStreamability(reasons, config.isStreamabilityEnabled(), false);
            if (doc != null) {
                return doc;
            }
        //}

        action = Inversion.invertExpression(getBody(), false);
        return this;
    }

    @Override
    public void prepareForStreaming() throws XPathException {
        if (action == null) {
            prepareForStreaming(getPackageData());
        }
    }

    public Expression checkStreamability(List<String> reasons, boolean enabled, boolean strict) throws XPathException {
        Configuration config = getConfiguration();
        boolean streamable;
        if (!enabled) {
            reasons.add("Streaming is disabled for this Saxon Configuration");
            streamable = false;
        } else {
            ContextItemStaticInfoEE contextInfo = new ContextItemStaticInfoEE(AnyItemType.getInstance(), true, Posture.STRIDING);
            contextInfo.setStrictStreamabilityRules(strict);
            PostureAndSweep s = Streamability.getStreamability(getBody(), contextInfo, reasons);
            streamable = s.getPosture() == Posture.GROUNDED;

            if (!streamable) {
                if (reasons.isEmpty() && Streamability.isIncrementalPosture(s.getPosture())) {
                    reasons.add("The instruction returns nodes from the streamed input document.");
                }
            }
        }

        if (!streamable) {
            boolean fallback = config.getBooleanProperty(Feature.STREAMING_FALLBACK);
            if (fallback) {
                reasons.add("Falling back to non-streaming implementation");
            }
            StringBuilder message = new StringBuilder("The body of the xsl:source-document instruction is not streamable");
            for (String r : reasons) {
                message.append("\n  *  ").append(r);
            }
            XmlProcessingIncident err = new XmlProcessingIncident(message.toString(), "XTSE3430", this.getLocation()).asWarning();
            err.setStaticError(true);
            if (fallback) {
                config.makeErrorReporter().report(err);
                return new SourceDocument(getHref(), getBody(), parseOptions);
            } else {
                throw XPathException.fromXmlProcessingError(err);
            }
        }
        return null;
    }

    /**
     * Determine whether this instruction creates new nodes.
     * This implementation returns a default value of false
     *
     * @return true if the instruction creates new nodes (or if it can't be proved that it doesn't)
     */
    @Override
    public boolean mayCreateNewNodes() {
        return !getBody().hasSpecialProperty(StaticProperty.NO_NODES_NEWLY_CREATED);
    }

    /**
     * Compute the dependencies of an expression, as the union of the
     * dependencies of its subexpressions. (This is overridden for path expressions
     * and filter expressions, where the dependencies of a subexpression are not all
     * propogated). This method should be called only once, to compute the dependencies;
     * after that, getDependencies should be used.
     *
     * @return the depencies, as a bit-mask
     */

    @Override
    public int computeDependencies() {
        // Focus-dependency in the body is not relevant.
        int dependencies = 0;
        dependencies |= getHref().getDependencies();
        dependencies |= getBody().getDependencies() & ~StaticProperty.DEPENDS_ON_FOCUS;
        return dependencies;
    }

    /**
     * Get the static properties of this expression (other than its type). The result is
     * bit-signficant. These properties are used for optimizations. In general, if
     * property bit is set, it is true, but if it is unset, the value is unknown.
     *
     * @return a set of flags indicating static properties of this expression
     */
    @Override
    protected int computeSpecialProperties() {
        // Not sure of the general rules here but we'll pick up some special cases which are useful for XQuery streaming
        // use cases written using saxon:stream() - where the body is always a call on snapshot()
        Expression body = getBody();
        if (body.hasSpecialProperty(StaticProperty.ALL_NODES_NEWLY_CREATED)) {
            return StaticProperty.ORDERED_NODESET | StaticProperty.PEER_NODESET;
        }
        return super.computeSpecialProperties();
    }

    public Inversion getAction() {
        return action;
    }


    /**
     * Copy an expression. This makes a deep copy.
     *
     * @return the copy of the original expression
     * @param rebindings variables that need to be re-bound
     */
    @Override
    public Expression copy(RebindingMap rebindings) {
        StreamInstr exp = new StreamInstr(getHref().copy(rebindings), getBody().copy(rebindings), parseOptions);
        exp.setRetainedStaticContext(getRetainedStaticContext());
        ExpressionTool.copyLocationInfo(this, exp);
        if (action != null) {
            try {
                exp.prepareForStreaming(getPackageData());
            } catch (XPathException e) {
                throw new AssertionError("Failure while preparing copy of xsl:stream instruction for streaming");
            }
        }
        return exp;
    }


    /**
     * Evaluate the instruction in push mode
     *
     * @param output the final destination for the output of the instruction
     * @param context the evaluation context
     * @throws XPathException in the event of a failure
     * @throws QuitParsingException if there was an early exit, that is, if the instruction was evaluated
     * without reading the input to completion
     */

    @Override
    public void push(Outputter output, XPathContext context) throws XPathException {
        PackageData pd = getPackageData();
        if (pd instanceof StylesheetPackage && ((StylesheetPackage)pd).isFallbackToNonStreaming()) {
            super.push(output, context);
        } else {
            Controller controller = context.getController();
            XPathContextMajor c2 = context.newContext();
            c2.setCurrentTemplateRule(null);
            PipelineConfiguration pipe = controller.makePipelineConfiguration();

            WatchManager wm = new WatchManager(pipe);
            wm.setXPathContext(context);
            wm.setOutputter(output);

            AccumulatorRegistryEE am = (AccumulatorRegistryEE)
                    getRetainedStaticContext().getPackageData().getAccumulatorRegistry();

            if (am != null && accumulators != null) {
                AccumulatorRegistryEE.registerSelectedAccumulators(wm, accumulators);
            }

            Receiver stream = new ContentDetector(wm);

            if (getPackageData() instanceof StylesheetPackage && ((StylesheetPackage) getPackageData()).isStripsTypeAnnotations()) {
                stream = getConfiguration().getAnnotationStripper(stream);
            }
            if (action == null) {
                prepareForStreaming(getPackageData());
                //throw new XPathException("Cannot execute xsl:stream when optimization has been disabled", SaxonErrorCode.SXST0066, this.getLocation());
            }
            wm.addWatch(new StreamWatch(action, wm, c2), true);

            AtomicValue hrefVal = (AtomicValue) getHref().evaluateItem(context);
            String href = hrefVal.getStringValue();
            boolean verbose = controller.getConfiguration().isTiming();
            if (verbose) {
                try {
                    controller.getConfiguration().getLogger().info("Streaming " + ResolveURI.makeAbsolute(href, getStaticBaseURIString()));
                } catch (URISyntaxException e) {
                    controller.getConfiguration().getLogger().info("Streaming " + href);
                }
            }
            try {
                DocumentFn.sendDoc(href, getStaticBaseURIString(), getPackageData(),
                                   context, getLocation(), stream, parseOptions);
            } catch (QuitParsingException q) {
                // This is an early exit indicating success
                q.maybeSetLocation(getLocation());
                q.maybeSetContext(context);
                if (verbose) {
                    controller.getConfiguration().getLogger().info("Streaming " + href + " : early exit ");
                }
                throw q;
            }
        }

    }

    /**
     * Return an Iterator to iterate over the values of a sequence. The value of every
     * expression can be regarded as a sequence, so this method is supported for all
     * expressions. This implementation uses two threads: the instruction is evaluated
     * in push mode in a child thread (using the process() method), putting its results
     * on a queue, from which this method reads them.
     *
     * @param context supplies the context for evaluation
     * @return a SequenceIterator that can be used to iterate over the result
     * of the expression
     * @throws XPathException if any dynamic error occurs evaluating the
     *                        expression
     */

    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        return new PushToPull(makeElaborator().elaborateForPush(), context).getIterator();
    }

    @Override
    public String getExportTag() {
        return "stream";
    }

    public Elaborator getElaborator() {
        return new StreamInstrElaborator();
    }

    public static class StreamInstrElaborator extends PushElaborator {

        @Override
        public PushEvaluator elaborateForPush() {
            StreamInstr expr = (StreamInstr) getExpression();
            return (output, context) -> {
                try {
                    expr.push(output, context);
                } catch (QuitParsingException q) {
                    // no action, this is an early exit indicating success
                    throw q;
                } catch (XPathException e) {
                    e.maybeSetLocation(expr.getLocation());
                    if (e.getErrorCodeQName() == null) {
                        e.setErrorCode("FODC0002");
                    }
                    throw e;
                }
                return null;
            };
        }

        @Override
        public PullEvaluator elaborateForPull() {
            PushEvaluator pusher = elaborateForPush();
            return context -> new PushToPull(pusher, context).getIterator();
        }
    }



}

// Copyright (c) 2018-2022 Saxonica Limited

using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class StreamInstr : Saxon.Hej.expr.instruct.SourceDocument, Saxon.Eej.ee.parallel.Pusher {
        private Saxon.Eej.ee.stream.Inversion action;
        public StreamInstr(Saxon.Hej.expr.Expression hrefExp, Saxon.Hej.expr.Expression body, Saxon.Hej.lib.ParseOptions options) : base (hrefExp, body, options) {
        }
        public override void setSpaceStrippingRule(Saxon.Hej.om.SpaceStrippingRule rule) {
            parseOptions = parseOptions.withSpaceStrippingRule(rule);
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo newType = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
            newType.setContextPostureStriding();
            hrefOp.optimize(visitor, contextItemType);
            bool previouslyStreaming = visitor.isOptimizeForStreaming();
            visitor.setOptimizeForStreaming(true);
            bodyOp.optimize(visitor, newType);
            visitor.setOptimizeForStreaming(previouslyStreaming);
            return this;
        }
        public Saxon.Hej.expr.Expression prepareForStreaming(Saxon.Hej.expr.PackageData pack) {
            Saxon.Hej.Configuration config = pack.getConfiguration();
            int localLicense = pack.getLocalLicenseId();
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            Saxon.Hej.expr.Expression doc = checkStreamability(reasons, config.isStreamabilityEnabled(), false);
            if ((doc != null)) {
                return doc;
            }
            action = /*134*/Saxon.Eej.ee.stream.Inversion.invertExpression(getBody(), false);
            return this;
        }
        public override void prepareForStreaming() {
            if ((action == null)) {
                prepareForStreaming(getPackageData());
            }
        }
        public Saxon.Hej.expr.Expression checkStreamability(System.Collections.Generic.IList<string> reasons, bool enabled, bool strict) {
            Saxon.Hej.Configuration config = getConfiguration();
            bool streamable;
            if (!(enabled)) {
                reasons.Add("Streaming is disabled for this Saxon Configuration");
                streamable = false;
            } else {
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.type.AnyItemType.getInstance(), true, Saxon.Eej.ee.stream.Posture.STRIDING);
                contextInfo.setStrictStreamabilityRules(strict);
                Saxon.Eej.ee.stream.PostureAndSweep s = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(getBody(), contextInfo, reasons);
                streamable = (s.getPosture() == Saxon.Eej.ee.stream.Posture.GROUNDED);
                if (!(streamable)) {
                    if ((reasons.Count==0 && /*134*/Saxon.Eej.ee.stream.Streamability.isIncrementalPosture(s.getPosture()))) {
                        reasons.Add("The instruction returns nodes from the streamed input document.");
                    }
                }
            }
            if (!(streamable)) {
                bool fallback = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK);
                if (fallback) {
                    reasons.Add("Falling back to non-streaming implementation");
                }
                Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder("The body of the xsl:source-document instruction is not streamable");
                foreach (string r in reasons) {
                    message.append("\n  *  ").append(r);
                }
                Saxon.Hej.trans.XmlProcessingIncident err = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message.ToString(), "XTSE3430", this.getLocation()).asWarning());
                err.setStaticError(true);
                if (fallback) {
                    config.makeErrorReporter().report(err);
                    return new Saxon.Hej.expr.instruct.SourceDocument(getHref(), getBody(), parseOptions);
                } else {
                    throw Saxon.Hej.trans.XPathException.fromXmlProcessingError(err);
                }
            }
            return null;
        }
        public override bool mayCreateNewNodes() {
            return !(getBody().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public override int computeDependencies() {
            int dependencies = 0;
            dependencies |= getHref().getDependencies();
            dependencies |= (getBody().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS));
            return dependencies;
        }
        protected internal override int computeSpecialProperties() {
            Saxon.Hej.expr.Expression body = getBody();
            if (body.hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED)) {
                return (Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET);
            }
            return base.computeSpecialProperties();
        }
        public Saxon.Eej.ee.stream.Inversion getAction() {
            return action;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Eej.ee.stream.StreamInstr exp = new Saxon.Eej.ee.stream.StreamInstr(getHref().copy(rebindings), getBody().copy(rebindings), parseOptions);
            exp.setRetainedStaticContext(getRetainedStaticContext());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            if ((action != null)) {
                try {
                    exp.prepareForStreaming(getPackageData());
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.AssertionError("Failure while preparing copy of xsl:stream instruction for streaming");
                }
            }
            return exp;
        }
        public override void push(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.PackageData pd = getPackageData();
            if (((pd is Saxon.Hej.style.StylesheetPackage) && ((Saxon.Hej.style.StylesheetPackage)(pd)).isFallbackToNonStreaming())) {
                base.push(output, context);
            } else {
                Saxon.Hej.Controller controller = context.getController();
                Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                c2.setCurrentTemplateRule(null);
                Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
                Saxon.Eej.ee.stream.watch.WatchManager wm = new Saxon.Eej.ee.stream.watch.WatchManager(pipe);
                wm.setXPathContext(context);
                wm.setOutputter(output);
                Saxon.Eej.ee.stream.AccumulatorRegistryEE am = (Saxon.Eej.ee.stream.AccumulatorRegistryEE)(getRetainedStaticContext().getPackageData().getAccumulatorRegistry());
                if (((am != null) && (accumulators != null))) {
                    /*134*/Saxon.Eej.ee.stream.AccumulatorRegistryEE.registerSelectedAccumulators(wm, accumulators);
                }
                Saxon.Hej.@event.Receiver stream = new Saxon.Eej.ee.stream.ContentDetector(wm);
                if (((getPackageData() is Saxon.Hej.style.StylesheetPackage) && ((Saxon.Hej.style.StylesheetPackage)(getPackageData())).isStripsTypeAnnotations())) {
                    stream = getConfiguration().getAnnotationStripper(stream);
                }
                if ((action == null)) {
                    prepareForStreaming(getPackageData());
                }
                wm.addWatch(new Saxon.Eej.ee.stream.watch.StreamWatch(action, wm, c2), true);
                Saxon.Hej.value.AtomicValue hrefVal = (Saxon.Hej.value.AtomicValue)(getHref().evaluateItem(context));
                string href = hrefVal.getStringValue();
                bool verbose = controller.getConfiguration().isTiming();
                if (verbose) {
                    try {
                        controller.getConfiguration().getLogger().info(("Streaming "+Saxon.Hej.functions.ResolveURI.makeAbsolute(href, getStaticBaseURIString())));
                    } catch (System.UriFormatException e) {
                        controller.getConfiguration().getLogger().info(("Streaming "+href));
                    }
                }
                try {
                    Saxon.Hej.functions.DocumentFn.sendDoc(href, getStaticBaseURIString(), getPackageData(), context, getLocation(), stream, parseOptions);
                } catch (Saxon.Hej.trans.QuitParsingException q) {
                    q.maybeSetLocation(getLocation());
                    q.maybeSetContext(context);
                    if (verbose) {
                        controller.getConfiguration().getLogger().info((("Streaming "+href)+" : early exit "));
                    }
                    throw;
                }
            }
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Impl.Parallel.PushToPull(makeElaborator().elaborateForPush(), context).getIterator();
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            try {
                base.process(output, context);
            } catch (Saxon.Hej.trans.QuitParsingException e) {
            }
        }
        public override string getExportTag() {
            return "stream";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.ee.stream.StreamInstr.StreamInstrElaborator();
        }
        public class StreamInstrElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Eej.ee.stream.StreamInstr expr = (Saxon.Eej.ee.stream.StreamInstr)(getExpression());
                return (output, context) => {
                    try {
                        expr.push(output, context);
                    } catch (Saxon.Hej.trans.QuitParsingException q) {
                        if (q.isNotifiedByConsumer()) {
                            throw;
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(expr.getLocation());
                        if ((e.getErrorCodeQName() == null)) {
                            e.setErrorCode("FODC0002");
                        }
                        throw;
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.elab.PushEvaluator pusher = elaborateForPush();
                return context => new Saxon.Impl.Parallel.PushToPull(pusher, context).getIterator();
            }
        
        }
    
    }
}
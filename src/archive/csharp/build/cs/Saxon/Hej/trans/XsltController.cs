using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class XsltController : Saxon.Hej.Controller {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> messageCounters = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, int>();
        private bool assertionsEnabled = true;
        private Saxon.Hej.lib.ResultDocumentResolver resultDocumentResolver;
        private System.Collections.Generic.HashSet<Saxon.Hej.om.DocumentKey> allOutputDestinations;
        private Saxon.Hej.expr.Component.M initialMode = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> initialTemplateParams;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> initialTemplateTunnelParams;
        private readonly System.Collections.Generic.IDictionary<System.Int64,System.Collections.Generic.Stack<Saxon.Hej.expr.instruct.AttributeSet>> attributeSetEvaluationStacks = new System.Collections.Generic.Dictionary<System.Int64, System.Collections.Generic.Stack<Saxon.Hej.expr.instruct.AttributeSet>>();
        private Saxon.Hej.expr.accum.AccumulatorManager accumulatorManager = new Saxon.Hej.expr.accum.AccumulatorManager();
        private Saxon.Hej.serialize.PrincipalOutputGatekeeper gatekeeper = null;
        private Saxon.Hej.s9api.Destination principalDestination;
        private Saxon.Hej.trace.TemplateRuleTraceListener templateRuleTraceListener = null;
        private Saxon.Ejava.util.function.Consumer<Saxon.Api.Message> messageHandler;
        public XsltController(Saxon.Hej.Configuration config, Saxon.Hej.PreparedStylesheet pss) : base (config, pss) {
            initMessageHandler(config);
        }
        private void initMessageHandler(Saxon.Hej.Configuration config) {
            Saxon.Callbacks.StandardMessageHandler smh = new (config); setMessageHandler(message => smh.accept(message));
        }
        public override void reset() {
            base.reset();
            Saxon.Hej.Configuration config = getConfiguration();
            validationMode = config.getSchemaValidationMode();
            accumulatorManager = new Saxon.Hej.expr.accum.AccumulatorManager();
            traceListener = null;
            Saxon.Hej.lib.TraceListener tracer;
            try {
                tracer = config.makeTraceListener();
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.IllegalStateException(err.Message);
            }
            if ((tracer != null)) {
                addTraceListener(tracer);
            }
            setModel(config.getParseOptions().getModel());
            globalContextItem = null;
            initialMode = null;
            clearPerTransformationData();
        }
        protected internal override void clearPerTransformationData() { lock(this) {
            base.clearPerTransformationData();
            principalResult = null;
            allOutputDestinations = null;
            if ((messageCounters != null)) {
                messageCounters.Clear();
            }
        }}
        public void setInitialMode(Saxon.Hej.om.StructuredQName expandedModeName) {
            if (((expandedModeName == null) || expandedModeName.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME))) {
                Saxon.Hej.trans.Mode initial = ((Saxon.Hej.PreparedStylesheet)(executable)).getRuleManager().obtainMode(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME, true);
                initialMode = initial.getDeclaringComponent();
            } else {
                Saxon.Hej.style.StylesheetPackage topLevelPackage = (Saxon.Hej.style.StylesheetPackage)(executable.getTopLevelPackage());
                if (expandedModeName.Equals(Saxon.Hej.trans.Mode.DEFAULT_MODE_NAME)) {
                    Saxon.Hej.om.StructuredQName defaultModeName = topLevelPackage.getDefaultMode();
                    if (!(expandedModeName.Equals(defaultModeName))) {
                        setInitialMode(defaultModeName);
                    }
                } else {
                    bool declaredModes = topLevelPackage.isDeclaredModes();
                    Saxon.Hej.trans.SymbolicName sn = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, expandedModeName);
                    Saxon.Hej.expr.Component.M c = (Saxon.Hej.expr.Component.M)(topLevelPackage.getComponent(sn));
                    if ((c == null)) {
                        throw new Saxon.Hej.trans.XPathException((("Requested initial mode "+expandedModeName)+" is not defined in the stylesheet"), "XTDE0045");
                    }
                    if (!(((Saxon.Hej.PreparedStylesheet)(executable)).isEligibleInitialMode(c))) {
                        throw new Saxon.Hej.trans.XPathException((("Requested initial mode "+expandedModeName)+" is private in the top-level package"), "XTDE0045");
                    }
                    initialMode = c;
                    if (((!(declaredModes) && initialMode.getActor().isEmpty()) && !(expandedModeName.Equals(topLevelPackage.getDefaultMode())))) {
                        throw new Saxon.Hej.trans.XPathException((("Requested initial mode "+expandedModeName)+" contains no template rules"), "XTDE0045");
                    }
                }
            }
        }
        public Saxon.Hej.om.StructuredQName getInitialModeName() {
            return ((initialMode == null) ? null : initialMode.getActor().getModeName());
        }
        public Saxon.Hej.trans.Mode getInitialMode() {
            if ((initialMode == null)) {
                Saxon.Hej.style.StylesheetPackage top = (Saxon.Hej.style.StylesheetPackage)(executable.getTopLevelPackage());
                Saxon.Hej.om.StructuredQName defaultMode = top.getDefaultMode();
                if ((defaultMode == null)) {
                    defaultMode = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                }
                Saxon.Hej.expr.Component.M c = (Saxon.Hej.expr.Component.M)(top.getComponent(new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, defaultMode)));
                initialMode = c;
                return c.getActor();
            } else {
                return initialMode.getActor();
            }
        }
        public Saxon.Hej.expr.accum.AccumulatorManager getAccumulatorManager() {
            return accumulatorManager;
        }
        public bool checkUniqueOutputDestination(Saxon.Hej.om.DocumentKey uri) { lock(this) {
            if ((uri == null)) {
                return true;
            }
            if ((allOutputDestinations == null)) {
                allOutputDestinations = new System.Collections.Generic.HashSet<Saxon.Hej.om.DocumentKey>(20);
            }
            return !(allOutputDestinations.Contains(uri));
        }}
        public void addUnavailableOutputDestination(Saxon.Hej.om.DocumentKey uri) { lock(this) {
            if ((allOutputDestinations == null)) {
                allOutputDestinations = new System.Collections.Generic.HashSet<Saxon.Hej.om.DocumentKey>(20);
            }
            allOutputDestinations.Add(uri);
        }}
        public void removeUnavailableOutputDestination(Saxon.Hej.om.DocumentKey uri) { lock(this) {
            if ((allOutputDestinations != null)) {
                allOutputDestinations.Remove(uri);
            }
        }}
        public bool isUnusedOutputDestination(Saxon.Hej.om.DocumentKey uri) { lock(this) {
            return ((allOutputDestinations == null) || !(allOutputDestinations.Contains(uri)));
        }}
        public void setInitialTemplateParameters(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> @params, bool tunnel) {
            if (tunnel) {
                this.initialTemplateTunnelParams = @params;
            } else {
                this.initialTemplateParams = @params;
            }
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> getInitialTemplateParameters(bool tunnel) {
            return (tunnel ? initialTemplateTunnelParams : initialTemplateParams);
        }
        public void setMessageHandler(Saxon.Ejava.util.function.Consumer<Saxon.Api.Message> handler) {
            messageHandler = handler;
        }
        public Saxon.Ejava.util.function.Consumer<Saxon.Api.Message> getMessageHandler() {
            return messageHandler;
        }
        public void incrementMessageCounter(Saxon.Hej.om.StructuredQName code) {
            lock (messageCounters) {
                int n = Saxon.Impl.Helpers.MapUtils.GetOrDefault(messageCounters, code, 0);
                Saxon.Impl.Helpers.MapUtils.Put(messageCounters, code, (n+1));
            }
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> getMessageCounters() {
            lock (messageCounters) {
                return new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, int>(messageCounters);
            }
        }
        public Saxon.Hej.lib.ResultDocumentResolver getResultDocumentResolver() {
            return resultDocumentResolver;
        }
        public void setResultDocumentResolver(Saxon.Hej.lib.ResultDocumentResolver resultDocumentResolver) {
            this.resultDocumentResolver = resultDocumentResolver;
        }
        public void setPrincipalDestination(Saxon.Hej.s9api.Destination destination) {
            principalDestination = destination;
        }
        public Saxon.Hej.s9api.Destination getPrincipalDestination() {
            return principalDestination;
        }
        public Saxon.Hej.trace.TemplateRuleTraceListener getTemplateRuleTraceListener() {
            return templateRuleTraceListener;
        }
        public void setTemplateRuleTraceListener(Saxon.Hej.trace.TemplateRuleTraceListener templateRuleTraceListener) {
            this.templateRuleTraceListener = templateRuleTraceListener;
        }
        public bool isAssertionsEnabled() {
            return assertionsEnabled;
        }
        public void setAssertionsEnabled(bool enabled) {
            this.assertionsEnabled = enabled;
        }
        public override void preEvaluateGlobals(Saxon.Hej.expr.XPathContext context) {
        }
        public void applyTemplates(Saxon.Hej.om.Sequence source, Saxon.Hej.@event.Receiver @out) {
            checkReadiness();
            try {
                Saxon.Hej.@event.ComplexContentOutputter dest = prepareOutputReceiver(@out);
                Saxon.Hej.expr.XPathContextMajor initialContext = newXPathContext();
                initialContext.createThreadManager();
                initialContext.setOrigin(this);
                Saxon.Hej.trans.Mode mode = getInitialMode();
                if ((mode == null)) {
                    throw new Saxon.Hej.trans.XPathException((("Requested initial mode "+(((initialMode == null) ? "#unnamed" : initialMode.getActor().getModeName().getDisplayName())))+" does not exist"), "XTDE0045");
                }
                if (!(((Saxon.Hej.PreparedStylesheet)(executable)).isEligibleInitialMode(initialMode))) {
                    throw new Saxon.Hej.trans.XPathException((("Requested initial mode "+(mode.getModeName().getDisplayName()))+" is not public or final"), "XTDE0045");
                }
                warningIfStreamable(mode);
                Saxon.Hej.expr.instruct.ParameterSet ordinaryParams = null;
                if ((initialTemplateParams != null)) {
                    ordinaryParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateParams);
                }
                Saxon.Hej.expr.instruct.ParameterSet tunnelParams = null;
                if ((initialTemplateTunnelParams != null)) {
                    tunnelParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateTunnelParams);
                }
                Saxon.Hej.om.SequenceIterator iter = source.iterate();
                Saxon.Hej.expr.MappingFunction preprocessor = getInputPreprocessor(mode);
                iter = new Saxon.Hej.expr.MappingIterator(iter, preprocessor);
                initialContext.trackFocus(iter);
                initialContext.setCurrentMode(initialMode);
                initialContext.setCurrentComponent(initialMode);
                Saxon.Hej.expr.instruct.TailCall tc = mode.applyTemplates(ordinaryParams, tunnelParams, null, dest, initialContext, Saxon.Hej.expr.parser.Loc.NONE);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
                initialContext.waitForChildThreads();
                dest.close();
            } catch (Saxon.Hej.expr.instruct.TerminationException err) {
                if (!(err.hasBeenReported())) {
                    reportFatalError(err);
                }
                throw;
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                handleXPathException(err.getXPathException());
            } catch (Saxon.Hej.trans.XPathException err) {
                handleXPathException(err);
            } finally {
                inUse = false;
                if ((traceListener != null)) {
                    traceListener.close();
                }
                principalResultURI = null;
            }
        }
        private Saxon.Hej.@event.ComplexContentOutputter prepareOutputReceiver(Saxon.Hej.@event.Receiver @out) {
            principalResult = @out;
            if ((principalResultURI == null)) {
                principalResultURI = @out.getSystemId();
            }
            if (getExecutable().createsSecondaryResult()) {
                @out = this.gatekeeper = new Saxon.Hej.serialize.PrincipalOutputGatekeeper(this, @out);
            }
            Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(@out);
            cco.setSystemId(@out.getSystemId());
            cco.open();
            return cco;
        }
        public Saxon.Hej.serialize.PrincipalOutputGatekeeper getGatekeeper() {
            return gatekeeper;
        }
        private Saxon.Hej.expr.MappingFunction getInputPreprocessor(Saxon.Hej.trans.Mode finalMode) {
            return Saxon.Hej.expr.SequenceMapper.of(item => {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    if ((node.getConfiguration() == null)) {
                        throw new Saxon.Hej.trans.XPathException("The supplied source document must be associated with a Configuration");
                    }
                    if (!(node.getConfiguration().isCompatible(executable.getConfiguration()))) {
                        throw new Saxon.Hej.trans.XPathException("Source document and stylesheet must use the same or compatible Configurations", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                    }
                    if ((node.getTreeInfo().isTyped() && !(executable.isSchemaAware()))) {
                        throw new Saxon.Hej.trans.XPathException("Cannot use a schema-validated source document unless the stylesheet is schema-aware");
                    }
                    if ((isStylesheetStrippingTypeAnnotations() && (node != globalContextItem))) {
                        Saxon.Hej.om.TreeInfo docInfo = node.getTreeInfo();
                        if (docInfo.isTyped()) {
                            Saxon.Hej.tree.wrapper.TypeStrippedDocument strippedDoc = new Saxon.Hej.tree.wrapper.TypeStrippedDocument(docInfo);
                            node = strippedDoc.wrap(node);
                        }
                    }
                    Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = getSpaceStrippingRule();
                    if (((((isStylesheetContainingStripSpace() && isStripSourceTree()) && !(((node is Saxon.Hej.tree.wrapper.SpaceStrippedNode)))) && (node != globalContextItem)) && (node.getTreeInfo().getSpaceStrippingRule() != spaceStrippingRule))) {
                        Saxon.Hej.tree.wrapper.SpaceStrippedDocument strippedDoc = new Saxon.Hej.tree.wrapper.SpaceStrippedDocument(node.getTreeInfo(), spaceStrippingRule);
                        if (!(Saxon.Hej.tree.wrapper.SpaceStrippedNode.isPreservedNode(node, strippedDoc, node.getParent()))) {
                            return Saxon.Hej.tree.iter.EmptyIterator.getInstance();
                        }
                        node = strippedDoc.wrap(node);
                    }
                    if ((getAccumulatorManager() != null)) {
                        getAccumulatorManager().setApplicableAccumulators(node.getTreeInfo(), finalMode.getAccumulators());
                    }
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(node);
                } else {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item);
                }
            } );
        }
        private void warningIfStreamable(Saxon.Hej.trans.Mode mode) {
            if (mode.isDeclaredStreamable()) {
                warning(((((initialMode == null) ? "" : getInitialMode().getModeTitle(true)))+" is streamable, but the input is not supplied as a stream"), Saxon.Hej.trans.SaxonErrorCode.SXWN9045, Saxon.Hej.expr.parser.Loc.NONE);
            }
        }
        public void callTemplate(Saxon.Hej.om.StructuredQName initialTemplateName, Saxon.Hej.@event.Receiver @out) {
            checkReadiness();
            try {
                Saxon.Hej.@event.ComplexContentOutputter dest = prepareOutputReceiver(@out);
                Saxon.Hej.expr.XPathContextMajor initialContext = newXPathContext();
                initialContext.createThreadManager();
                initialContext.setOrigin(this);
                if ((globalContextItem != null)) {
                    initialContext.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator(globalContextItem));
                }
                Saxon.Hej.expr.instruct.ParameterSet ordinaryParams = null;
                if ((initialTemplateParams != null)) {
                    ordinaryParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateParams);
                }
                Saxon.Hej.expr.instruct.ParameterSet tunnelParams = null;
                if ((initialTemplateTunnelParams != null)) {
                    tunnelParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateTunnelParams);
                }
                Saxon.Hej.style.StylesheetPackage pack = (Saxon.Hej.style.StylesheetPackage)(executable.getTopLevelPackage());
                Saxon.Hej.expr.Component initialComponent = pack.getComponent(new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, initialTemplateName));
                if ((initialComponent == null)) {
                    throw new Saxon.Hej.trans.XPathException((("Template "+initialTemplateName.getDisplayName())+" does not exist"), "XTDE0040");
                }
                if ((!(pack.isImplicitPackage()) && !((((initialComponent.getVisibility() == Saxon.Hej.trans.Visibility.PUBLIC) || (initialComponent.getVisibility() == Saxon.Hej.trans.Visibility.FINAL)))))) {
                    throw new Saxon.Hej.trans.XPathException(((("Template "+initialTemplateName.getDisplayName())+" is ")+Saxon.Hej.trans.Err.describeVisibility(initialComponent.getVisibility())), "XTDE0040");
                }
                Saxon.Hej.expr.instruct.NamedTemplate t = (Saxon.Hej.expr.instruct.NamedTemplate)(initialComponent.getActor());
                Saxon.Hej.expr.XPathContextMajor c2 = initialContext.newContext();
                initialContext.setOrigin(this);
                c2.setCurrentComponent(initialComponent);
                c2.openStackFrame(t.getStackFrameMap());
                c2.setLocalParameters(ordinaryParams);
                c2.setTunnelParameters(tunnelParams);
                Saxon.Hej.expr.instruct.TailCall tc = t.expand(dest, c2);
                while ((tc != null)) {
                    tc = tc.processLeavingTail();
                }
                initialContext.waitForChildThreads();
                dest.close();
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                handleXPathException(err.getXPathException());
            } catch (Saxon.Hej.trans.XPathException err) {
                handleXPathException(err);
            } finally {
                if ((traceListener != null)) {
                    traceListener.close();
                }
                inUse = false;
            }
        }
        public void applyStreamingTemplates(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.@event.Receiver @out) {
            checkReadiness();
            Saxon.Hej.@event.ComplexContentOutputter dest = prepareOutputReceiver(@out);
            bool close = false;
            try {
                int validationMode = getSchemaValidationMode();
                Saxon.Ejavax.xml.transform.Source underSource = source;
                if ((source is Saxon.Hej.lib.AugmentedSource)) {
                    close = ((Saxon.Hej.lib.AugmentedSource)(source)).isPleaseCloseAfterUse();
                    int localValidate = ((Saxon.Hej.lib.AugmentedSource)(source)).getSchemaValidation();
                    if ((localValidate != Saxon.Hej.lib.Validation.DEFAULT)) {
                        validationMode = localValidate;
                    }
                    underSource = ((Saxon.Hej.lib.AugmentedSource)(source)).getContainedSource();
                }
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Ejavax.xml.transform.Source s2 = config.getSourceResolver().resolveSource(underSource, config);
                if ((s2 != null)) {
                    underSource = s2;
                }
                if (!(initialMode.getActor().isDeclaredStreamable())) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Initial mode is not streamable");
                }
                Saxon.Hej.expr.XPathContextMajor initialContext = newXPathContext();
                initialContext.createThreadManager();
                initialContext.setOrigin(this);
                Saxon.Hej.expr.instruct.ParameterSet ordinaryParams = null;
                if ((initialTemplateParams != null)) {
                    ordinaryParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateParams);
                }
                Saxon.Hej.expr.instruct.ParameterSet tunnelParams = null;
                if ((initialTemplateTunnelParams != null)) {
                    tunnelParams = new Saxon.Hej.expr.instruct.ParameterSet(initialTemplateTunnelParams);
                }
                Saxon.Hej.@event.Receiver despatcher = config.makeStreamingTransformer(initialMode.getActor(), ordinaryParams, tunnelParams, dest, initialContext);
                if ((config.isStripsAllWhiteSpace() || isStylesheetContainingStripSpace())) {
                    despatcher = makeStripper(despatcher);
                }
                Saxon.Hej.@event.PipelineConfiguration pipe = despatcher.getPipelineConfiguration();
                pipe.setParseOptions(pipe.getParseOptions().withSchemaValidationMode(this.validationMode));
                bool verbose = getConfiguration().isTiming();
                if (verbose) {
                    getConfiguration().getLogger().info(("Streaming "+source.getSystemId()));
                }
                try {
                    Saxon.Hej.@event.Sender.send(underSource, despatcher, null);
                } catch (Saxon.Hej.trans.QuitParsingException e) {
                    if (verbose) {
                        getConfiguration().getLogger().info((("Streaming "+source.getSystemId())+" : early exit"));
                    }
                }
                initialContext.waitForChildThreads();
                dest.close();
            } catch (Saxon.Hej.expr.instruct.TerminationException err) {
                if (!(err.hasBeenReported())) {
                    reportFatalError(err);
                }
                throw;
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                handleXPathException(err.getXPathException());
            } catch (Saxon.Hej.trans.XPathException err) {
                handleXPathException(err);
            } finally {
                inUse = false;
                if ((close && (source is Saxon.Hej.lib.AugmentedSource))) {
                    ((Saxon.Hej.lib.AugmentedSource)(source)).close();
                }
                if ((traceListener != null)) {
                    traceListener.close();
                }
            }
        }
        public Saxon.Hej.@event.Receiver getStreamingReceiver(Saxon.Hej.trans.Mode mode, Saxon.Hej.@event.Receiver result) {
            checkReadiness();
            Saxon.Hej.@event.ComplexContentOutputter dest = prepareOutputReceiver(result);
            Saxon.Hej.expr.XPathContextMajor initialContext = newXPathContext();
            initialContext.setOrigin(this);
            globalContextItem = null;
            if (!(mode.isDeclaredStreamable())) {
                throw new Saxon.Hej.trans.XPathException("mode supplied to getStreamingReceiver() must be streamable");
            }
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.@event.Receiver despatcher = config.makeStreamingTransformer(mode, null, null, dest, initialContext);
            if ((despatcher == null)) {
                throw new Saxon.Hej.trans.XPathException("Streaming requires Saxon-EE");
            }
            if ((config.isStripsAllWhiteSpace() || isStylesheetContainingStripSpace())) {
                despatcher = makeStripper(despatcher);
            }
            despatcher.setPipelineConfiguration(makePipelineConfiguration());
            Saxon.Hej.@event.Outputter finalResult = dest;
            return new Inner_ProxyReceiver_233(this, despatcher, finalResult);
        }
        public System.Collections.Generic.Stack<Saxon.Hej.expr.instruct.AttributeSet> getAttributeSetEvaluationStack() { lock(this) {
            long thread = Saxon.Ejava.lang.Thread.currentThread().getId();
            return Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(attributeSetEvaluationStacks, thread, k => new System.Collections.Generic.Stack<Saxon.Hej.expr.instruct.AttributeSet>());
        }}
        public void releaseAttributeSetEvaluationStack() { lock(this) {
            long thread = Saxon.Ejava.lang.Thread.currentThread().getId();
            attributeSetEvaluationStacks.Remove(thread);
        }}
    
        private class Inner_ProxyReceiver_233 : Saxon.Hej.@event.ProxyReceiver {
            private readonly XsltController outerInstance;
            public Inner_ProxyReceiver_233(XsltController outerInstance, /*AA*/Saxon.Hej.@event.Receiver arg1, Saxon.Hej.@event.Outputter finalResult) : base(arg1) {
                this.outerInstance = outerInstance;
                this.finalResult = finalResult;
            }
            private Saxon.Hej.@event.Outputter finalResult;
            public override void close() {
                if ((/*guess*/outerInstance.traceListener != null)) {
                    /*guess*/outerInstance.traceListener.close();
                }
                /*extraParam*/finalResult.close();
                /*guess*/outerInstance.inUse = false;
            }
        }

    }
}
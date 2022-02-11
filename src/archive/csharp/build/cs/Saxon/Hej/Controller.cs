using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public class Controller : Saxon.Hej.expr.ContextOriginator {
        private readonly Saxon.Hej.Configuration config;
        protected internal Saxon.Hej.expr.instruct.Executable executable;
        protected internal Saxon.Hej.om.Item globalContextItem;
        private bool globalContextItemPreset;
        private System.Collections.Generic.IDictionary<Saxon.Hej.expr.PackageData,Saxon.Hej.expr.instruct.Bindery> binderies;
        private Saxon.Hej.expr.instruct.GlobalParameterSet globalParameters;
        private bool convertParameters = true;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.instruct.GlobalVariable,System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.GlobalVariable>> globalVariableDependencies = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.instruct.GlobalVariable, System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.GlobalVariable>>();
        protected internal Saxon.Hej.lib.TraceListener traceListener;
        private bool tracingPaused;
        private Saxon.Hej.lib.Logger traceFunctionDestination;
        private Saxon.Hej.lib.JJ_ResourceResolver resourceResolver;
        protected internal Saxon.Hej.@event.Receiver principalResult;
        protected internal string principalResultURI;
        private Saxon.Hej.lib.UnparsedTextURIResolver unparsedTextResolver;
        private string defaultCollectionURI;
        private Saxon.Hej.lib.ErrorReporter errorReporter;
        private Saxon.Hej.om.TreeModel treeModel = Saxon.Hej.om.TreeModel.TINY_TREE;
        private Saxon.Hej.om.DocumentPool sourceDocumentPool;
        private Saxon.Hej.z.IntHashMap<System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex>> localIndexes;
        private System.Collections.Generic.Dictionary<string,System.Object> userDataTable;
        private Saxon.Hej.om.NodeInfo lastRememberedNode = null;
        private int lastRememberedNumber = -1;
        private Saxon.Hej.value.DateTimeValue currentDateTime;
        private bool dateTimePreset = false;
        private Saxon.Hej.expr.parser.PathMap pathMap = null;
        protected internal int validationMode = Saxon.Hej.lib.Validation.DEFAULT;
        protected internal bool inUse = false;
        private bool stripSourceTrees = true;
        private Saxon.Hej.lib.CollectionFinder collectionFinder = null;
        public const string ANONYMOUS_PRINCIPAL_OUTPUT_URI = "dummy:/anonymous/principal/result";
        private Saxon.Hej.trans.StylesheetCache stylesheetCache = null;
        private System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> focusTrackerFactory = (iter) => new Saxon.Hej.om.FocusTrackingIterator(iter);
        private System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> multiThreadedFocusTrackerFactory;
        public Controller(Saxon.Hej.Configuration config)  {
            this.config = config;
            executable = new Saxon.Hej.expr.instruct.Executable(config);
            sourceDocumentPool = new Saxon.Hej.om.DocumentPool();
            reset();
        }
        public Controller(Saxon.Hej.Configuration config, Saxon.Hej.expr.instruct.Executable executable)  {
            this.config = config;
            this.executable = executable;
            sourceDocumentPool = new Saxon.Hej.om.DocumentPool();
            reset();
        }
        public virtual void reset() {
            globalParameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            focusTrackerFactory = config.getFocusTrackerFactory(executable, false);
            multiThreadedFocusTrackerFactory = config.getFocusTrackerFactory(executable, true);
            resourceResolver = null;
            unparsedTextResolver = config.getUnparsedTextURIResolver();
            validationMode = config.getSchemaValidationMode();
            errorReporter = config.makeErrorReporter();
            traceListener = null;
            traceFunctionDestination = config.getLogger();
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
            currentDateTime = null;
            dateTimePreset = false;
            clearPerTransformationData();
        }
        protected internal virtual void clearPerTransformationData() { lock(this) {
            userDataTable = new System.Collections.Generic.Dictionary<string, System.Object>(20);
            principalResult = null;
            tracingPaused = false;
            lastRememberedNode = null;
            lastRememberedNumber = -1;
            stylesheetCache = null;
            localIndexes = null;
            if (!(globalContextItemPreset)) {
                globalContextItem = null;
            }
        }}
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public Saxon.Hej.om.Sequence getParameter(Saxon.Hej.om.StructuredQName name) {
            return globalParameters.get(name);
        }
        public Saxon.Hej.om.GroundedValue getConvertedParameter(Saxon.Hej.om.StructuredQName name, Saxon.Hej.value.SequenceType requiredType, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.GroundedValue val = globalParameters.convertParameterValue(name, requiredType, convertParameters, context);
            if ((val != null)) {
                Saxon.Hej.Configuration config = getConfiguration();
                Saxon.Hej.om.SequenceIterator iter = val.iterate();
                Saxon.Hej.om.Item next;
                while (((next = iter.next()) != null)) {
                    if (((next is Saxon.Hej.om.NodeInfo) && !(config.isCompatible(((Saxon.Hej.om.NodeInfo)(next)).getConfiguration())))) {
                        throw new Saxon.Hej.trans.XPathException(("A node supplied in a global parameter must be built using the same Configuration "+"that was used to compile the stylesheet or query"), Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                    }
                }
                if (((val is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(val)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                    string systemId = ((Saxon.Hej.om.NodeInfo)(val)).getRoot().getSystemId();
                    try {
                        if (((systemId != null) && new System.Uri(systemId, System.UriKind.RelativeOrAbsolute).IsAbsoluteUri)) {
                            Saxon.Hej.om.DocumentKey key = new Saxon.Hej.om.DocumentKey(systemId);
                            Saxon.Hej.om.DocumentPool pool = getDocumentPool();
                            if ((pool.find(key) == null)) {
                                pool.add(((Saxon.Hej.om.NodeInfo)(val)).getTreeInfo(), key);
                            }
                        }
                    } catch (System.UriFormatException err) {
                    }
                }
                val = val.materialize();
            }
            return val;
        }
        public void setBaseOutputURI(string uri) {
            principalResultURI = uri;
        }
        public string getBaseOutputURI() {
            return principalResultURI;
        }
        public Saxon.Hej.@event.Receiver getPrincipalResult() {
            return principalResult;
        }
        public Saxon.Hej.@event.SequenceCollector allocateSequenceOutputter() {
            Saxon.Hej.@event.PipelineConfiguration pipe = makePipelineConfiguration();
            return new Saxon.Hej.@event.SequenceCollector(pipe, 20);
        }
        public Saxon.Hej.@event.SequenceCollector allocateSequenceOutputter(int size) {
            Saxon.Hej.@event.PipelineConfiguration pipe = makePipelineConfiguration();
            return new Saxon.Hej.@event.SequenceCollector(pipe, size);
        }
        public Saxon.Hej.@event.PipelineConfiguration makePipelineConfiguration() {
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.getParseOptions().setSchemaValidationMode(validationMode);
            pipe.getParseOptions().setErrorReporter(errorReporter);
            pipe.setController(this);
            Saxon.Hej.expr.instruct.Executable executable = getExecutable();
            if ((executable != null)) {
                pipe.setHostLanguage(executable.getHostLanguage());
            }
            return pipe;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            errorReporter = reporter;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return errorReporter;
        }
        public void reportFatalError(Saxon.Hej.trans.XPathException err) {
            if (!(err.hasBeenReported())) {
                if ((err.getHostLanguage() == null)) {
                    if ((executable.getHostLanguage() == Saxon.Api.HostLanguage.XSLT)) {
                        err.setHostLanguage("XSLT");
                    } else {
                        if ((executable.getHostLanguage() == Saxon.Api.HostLanguage.XQUERY)) {
                            err.setHostLanguage("XQuery");
                        }
                    }
                }
                getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
                err.setHasBeenReported(true);
            }
        }
        public void warning(string message, string errorCode, Saxon.Hej.s9api.Location locator) {
            if ((locator == null)) {
                locator = Saxon.Hej.expr.parser.Loc.NONE;
            }
            if ((errorCode == null)) {
                errorCode = Saxon.Hej.trans.SaxonErrorCode.SXWN9000;
            }
            if ((message == null)) {
                message = "Unspecified warning";
            }
            Saxon.Hej.trans.XmlProcessingIncident warning = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, errorCode, locator).asWarning());
            errorReporter.report(warning);
        }
        protected internal void handleXPathException(Saxon.Hej.trans.XPathException err) {
            reportFatalError(err); throw err;
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public Saxon.Hej.om.DocumentPool getDocumentPool() {
            return sourceDocumentPool;
        }
        public void clearDocumentPool() {
            foreach (Saxon.Hej.expr.PackageData pack in getExecutable().getPackages()) {
                sourceDocumentPool.discardIndexes(pack.getKeyManager());
            }
            sourceDocumentPool = new Saxon.Hej.om.DocumentPool();
        }
        public Saxon.Hej.expr.instruct.Bindery getBindery(Saxon.Hej.expr.PackageData packageData) { lock(this) {
            Saxon.Hej.expr.instruct.Bindery b = Saxon.Impl.Helpers.MapUtils.Get(binderies, packageData);
            if ((b == null)) {
                b = new Saxon.Hej.expr.instruct.Bindery(packageData);
                Saxon.Impl.Helpers.MapUtils.Put(binderies, packageData, b);
            }
            return b;
        }}
        public void setGlobalContextItem(Saxon.Hej.om.Item contextItem) {
            setGlobalContextItem(contextItem, false);
        }
        public void setGlobalContextItem(Saxon.Hej.om.Item contextItem, bool alreadyStripped) {
            if (!(alreadyStripped)) {
                if (((globalContextItem is Saxon.Hej.tree.wrapper.SpaceStrippedNode) && (((/*1430*/Saxon.Hej.om.NodeInfo)((Saxon.Hej.tree.wrapper.SpaceStrippedNode)(globalContextItem)).getUnderlyingNode()) == contextItem))) {
                    return ;
                }
                if ((contextItem is Saxon.Hej.om.NodeInfo)) {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(contextItem);
                    contextItem = prepareInputTree(node.asActiveSource());
                    if (((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT) && (node.getSystemId() != null))) {
                        Saxon.Hej.om.DocumentKey key = new Saxon.Hej.om.DocumentKey(node.getSystemId());
                        if ((getDocumentPool().find(key) == null)) {
                            getDocumentPool().add(node.getTreeInfo(), key);
                        }
                    }
                }
            }
            if ((contextItem is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo startNode = (Saxon.Hej.om.NodeInfo)(contextItem);
                if ((startNode.getConfiguration() == null)) {
                    throw new Saxon.Hej.trans.XPathException("The supplied source document must be associated with a Configuration");
                }
                if (!(startNode.getConfiguration().isCompatible(executable.getConfiguration()))) {
                    throw new Saxon.Hej.trans.XPathException("Source document and stylesheet must use the same or compatible Configurations", Saxon.Hej.trans.SaxonErrorCode.SXXP0004);
                }
                if ((startNode.getTreeInfo().isTyped() && !(executable.isSchemaAware()))) {
                    throw new Saxon.Hej.trans.XPathException("Cannot use a schema-validated source document unless the stylesheet is schema-aware");
                }
            }
            this.globalContextItem = contextItem;
            this.globalContextItemPreset = true;
        }
        public void clearGlobalContextItem() {
            this.globalContextItem = null;
            this.globalContextItemPreset = false;
        }
        public Saxon.Hej.om.Item getGlobalContextItem() {
            return globalContextItem;
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            resourceResolver = resolver;
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return resourceResolver;
        }
        public void setUnparsedTextURIResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            unparsedTextResolver = resolver;
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return unparsedTextResolver;
        }
        public Saxon.Hej.lib.CollectionFinder getCollectionFinder() {
            if ((collectionFinder == null)) {
                collectionFinder = config.getCollectionFinder();
            }
            return collectionFinder;
        }
        public void setCollectionFinder(Saxon.Hej.lib.CollectionFinder cf) {
            collectionFinder = cf;
        }
        public void setDefaultCollection(string uri) {
            defaultCollectionURI = uri;
        }
        public string getDefaultCollection() {
            return ((defaultCollectionURI == null) ? getConfiguration().getDefaultCollection() : defaultCollectionURI);
        }
        public int getSchemaValidationMode() {
            return validationMode;
        }
        public void setSchemaValidationMode(int validationMode) {
            this.validationMode = validationMode;
        }
        public void setModel(Saxon.Hej.om.TreeModel model) {
            treeModel = model;
        }
        public Saxon.Hej.om.TreeModel getModel() {
            return treeModel;
        }
        public Saxon.Hej.@event.Builder makeBuilder() {
            Saxon.Hej.@event.Builder b = treeModel.makeBuilder(makePipelineConfiguration());
            b.setTiming(config.isTiming());
            b.setLineNumbering(config.isLineNumbering());
            return b;
        }
        public void setStripSourceTrees(bool strip) {
            stripSourceTrees = strip;
        }
        public bool isStripSourceTree() {
            return stripSourceTrees;
        }
        protected internal bool isStylesheetContainingStripSpace() {
            Saxon.Hej.om.SpaceStrippingRule rule;
            return (((executable is Saxon.Hej.PreparedStylesheet) && ((rule = ((Saxon.Hej.PreparedStylesheet)(executable)).getTopLevelPackage().getSpaceStrippingRule()) != null)) && (rule != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance()));
        }
        public bool isStylesheetStrippingTypeAnnotations() {
            return ((executable is Saxon.Hej.PreparedStylesheet) && ((Saxon.Hej.PreparedStylesheet)(executable)).getTopLevelPackage().isStripsTypeAnnotations());
        }
        public Saxon.Hej.@event.Stripper makeStripper(Saxon.Hej.@event.Receiver next) {
            if ((next == null)) {
                next = new Saxon.Hej.@event.Sink(makePipelineConfiguration());
            }
            return new Saxon.Hej.@event.Stripper(getSpaceStrippingRule(), next);
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            if ((config.getParseOptions().getSpaceStrippingRule() == Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance())) {
                return Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance();
            } else {
                if ((executable is Saxon.Hej.PreparedStylesheet)) {
                    Saxon.Hej.om.SpaceStrippingRule rule = ((Saxon.Hej.PreparedStylesheet)(executable)).getTopLevelPackage().getSpaceStrippingRule();
                    if ((rule != null)) {
                        return rule;
                    }
                }
            }
            return Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
        }
        public void registerDocument(Saxon.Hej.om.TreeInfo doc, Saxon.Hej.om.DocumentKey uri) {
            if ((!(getExecutable().isSchemaAware()) && !(Saxon.Hej.type.Untyped.getInstance().Equals(doc.getRootNode().getSchemaType())))) {
                bool isXSLT = (getExecutable().getHostLanguage() == Saxon.Api.HostLanguage.XSLT);
                string message;
                if (isXSLT) {
                    message = ((("The source document has been schema-validated, but"+" the stylesheet is not schema-aware. A stylesheet is schema-aware if")+" either (a) it contains an xsl:import-schema declaration, or (b) the stylesheet compiler")+" was configured to be schema-aware.");
                } else {
                    message = ((("The source document has been schema-validated, but"+" the query is not schema-aware. A query is schema-aware if")+" either (a) it contains an 'import schema' declaration, or (b) the query compiler")+" was configured to be schema-aware.");
                }
                throw new Saxon.Hej.trans.XPathException(message);
            }
            if ((uri != null)) {
                sourceDocumentPool.add(doc, uri);
            }
        }
        public Saxon.Hej.trans.rules.RuleManager getRuleManager() {
            Saxon.Hej.expr.instruct.Executable exec = getExecutable();
            return ((exec is Saxon.Hej.PreparedStylesheet) ? ((Saxon.Hej.PreparedStylesheet)(getExecutable())).getRuleManager() : null);
        }
        public void setTraceListener(Saxon.Hej.lib.TraceListener listener) {
            this.traceListener = listener;
        }
        public Saxon.Hej.lib.TraceListener getTraceListener() {
            return traceListener;
        }
        public bool isTracing() {
            return ((traceListener != null) && !(tracingPaused));
        }
        public void pauseTracing(bool pause) {
            tracingPaused = pause;
        }
        public void addTraceListener(Saxon.Hej.lib.TraceListener trace) {
            if ((trace != null)) {
                traceListener = Saxon.Hej.trace.TraceEventMulticaster.add(traceListener, trace);
            }
        }
        public void removeTraceListener(Saxon.Hej.lib.TraceListener trace) {
            traceListener = Saxon.Hej.trace.TraceEventMulticaster.remove(traceListener, trace);
        }
        public void setTraceFunctionDestination(Saxon.Hej.lib.Logger stream) {
            traceFunctionDestination = stream;
        }
        public Saxon.Hej.lib.Logger getTraceFunctionDestination() {
            return traceFunctionDestination;
        }
        public void initializeController(Saxon.Hej.expr.instruct.GlobalParameterSet @params) {
            binderies = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.PackageData, Saxon.Hej.expr.instruct.Bindery>();
            try {
                executable.checkSuppliedParameters(@params);
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                    throw;
                }
            }
            globalParameters = @params;
            globalContextItem = executable.checkInitialContextItem(globalContextItem, newXPathContext());
            if ((traceListener != null)) {
                traceListener.open(this);
                preEvaluateGlobals(newXPathContext());
            }
        }
        public void setApplyFunctionConversionRulesToExternalVariables(bool applyConversionRules) {
            convertParameters = applyConversionRules;
        }
        public System.Object getUserData(System.Object key, string name) { lock(this) {
            string keyValue = ((key.GetHashCode()+" ")+name);
            return Saxon.Impl.Helpers.MapUtils.Get(userDataTable, keyValue);
        }}
        public void setUserData(System.Object key, string name, System.Object data) { lock(this) {
            string keyVal = ((key.GetHashCode()+" ")+name);
            if ((data == null)) {
                userDataTable.Remove(keyVal);
            } else {
                Saxon.Impl.Helpers.MapUtils.Put(userDataTable, keyVal, data);
            }
        }}
        public Saxon.Hej.z.IntHashMap<System.Collections.Generic.IDictionary<System.Int64,Saxon.Hej.trans.KeyIndex>> getLocalIndexes() { lock(this) {
            if ((localIndexes == null)) {
                localIndexes = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.IDictionary<System.Int64, Saxon.Hej.trans.KeyIndex>>();
            }
            return localIndexes;
        }}
        public void setRememberedNumber(Saxon.Hej.om.NodeInfo node, int number) { lock(this) {
            lastRememberedNode = node;
            lastRememberedNumber = number;
        }}
        public int getRememberedNumber(Saxon.Hej.om.NodeInfo node) { lock(this) {
            if ((lastRememberedNode == node)) {
                return lastRememberedNumber;
            }
            return -1;
        }}
        protected internal void checkReadiness() {
            if (inUse) {
                throw new Saxon.Ejava.lang.IllegalStateException("The Controller is being used recursively or concurrently. This is not permitted.");
            }
            if ((binderies == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("The Controller has not been initialized");
            }
            inUse = true;
            clearPerTransformationData();
            if ((executable == null)) {
                throw new Saxon.Hej.trans.XPathException("Stylesheet has not been prepared");
            }
            if (!(dateTimePreset)) {
                currentDateTime = null;
            }
        }
        public Saxon.Hej.om.NodeInfo makeSourceTree(Saxon.Ejavax.xml.transform.Source source, int validationMode) {
            if ((source is Saxon.Hej.om.NodeSource)) {
                return ((Saxon.Hej.om.NodeSource)(source)).getNode();
            } else {
                if (((((source is Saxon.Ejavax.xml.transform.stream.StreamSource) && (source.getSystemId() != null)) && (((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getInputStream() == null)) && (((Saxon.Ejavax.xml.transform.stream.StreamSource)(source)).getReader() == null))) {
                    Saxon.Hej.om.DocumentKey key = new Saxon.Hej.om.DocumentKey(source.getSystemId());
                    Saxon.Hej.om.TreeInfo existing = sourceDocumentPool.find(key);
                    if ((existing != null)) {
                        return existing.getRootNode();
                    }
                }
            }
            Saxon.Hej.@event.Builder sourceBuilder = makeBuilder();
            sourceBuilder.setUseEventLocation(true);
            if ((sourceBuilder is Saxon.Hej.tree.tiny.TinyBuilder)) {
                ((Saxon.Hej.tree.tiny.TinyBuilder)(sourceBuilder)).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
            }
            Saxon.Hej.@event.Receiver r = sourceBuilder;
            Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance();
            if ((((config.isStripsAllWhiteSpace() || isStylesheetContainingStripSpace()) || (validationMode == Saxon.Hej.lib.Validation.STRICT)) || (validationMode == Saxon.Hej.lib.Validation.LAX))) {
                r = makeStripper(sourceBuilder);
                spaceStrippingRule = getSpaceStrippingRule();
            }
            if (isStylesheetStrippingTypeAnnotations()) {
                r = config.getAnnotationStripper(r);
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = sourceBuilder.getPipelineConfiguration();
            pipe.getParseOptions().setSchemaValidationMode(validationMode);
            r.setPipelineConfiguration(pipe);
            Saxon.Hej.@event.Sender.send(source, r, null);
            if (((source is Saxon.Hej.lib.AugmentedSource) && ((Saxon.Hej.lib.AugmentedSource)(source)).isPleaseCloseAfterUse())) {
                ((Saxon.Hej.lib.AugmentedSource)(source)).close();
            }
            Saxon.Hej.om.NodeInfo doc = sourceBuilder.getCurrentRoot();
            sourceBuilder.reset();
            if ((source.getSystemId() != null)) {
                registerDocument(doc.getTreeInfo(), new Saxon.Hej.om.DocumentKey(source.getSystemId()));
            }
            doc.getTreeInfo().setSpaceStrippingRule(spaceStrippingRule);
            return doc;
        }
        public Saxon.Hej.om.NodeInfo prepareInputTree(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.om.NodeInfo start = getConfiguration().unravel(source);
            if (isStylesheetStrippingTypeAnnotations()) {
                Saxon.Hej.om.TreeInfo docInfo = start.getTreeInfo();
                if (docInfo.isTyped()) {
                    Saxon.Hej.tree.wrapper.TypeStrippedDocument strippedDoc = new Saxon.Hej.tree.wrapper.TypeStrippedDocument(docInfo);
                    start = strippedDoc.wrap(start);
                }
            }
            if ((stripSourceTrees && isStylesheetContainingStripSpace())) {
                Saxon.Hej.om.TreeInfo docInfo = start.getTreeInfo();
                Saxon.Hej.om.SpaceStrippingRule spaceStrippingRule = getSpaceStrippingRule();
                if ((docInfo.getSpaceStrippingRule() != spaceStrippingRule)) {
                    Saxon.Hej.tree.wrapper.SpaceStrippedDocument strippedDoc = new Saxon.Hej.tree.wrapper.SpaceStrippedDocument(docInfo, spaceStrippingRule);
                    if (!(Saxon.Hej.tree.wrapper.SpaceStrippedNode.isPreservedNode(start, strippedDoc, start.getParent()))) {
                        return null;
                    }
                    start = strippedDoc.wrap(start);
                }
            }
            return start;
        }
        public virtual void preEvaluateGlobals(Saxon.Hej.expr.XPathContext context) {
            foreach (Saxon.Hej.expr.PackageData pack in getExecutable().getPackages()) {
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in pack.getGlobalVariableList()) {
                    if (!(var.isUnused())) {
                        try {
                            var.evaluateVariable(context, var.getDeclaringComponent());
                        } catch (Saxon.Hej.trans.XPathException err) {
                            getBindery(var.getPackageData()).setGlobalVariable(var, new Saxon.Hej.expr.instruct.Bindery.FailureValue(err));
                        }
                    }
                }
            }
        }
        public void registerGlobalVariableDependency(Saxon.Hej.expr.instruct.GlobalVariable one, Saxon.Hej.expr.instruct.GlobalVariable two) { lock(this) {
            if ((one == two)) {
                throw new Saxon.Hej.trans.XPathException.Circularity((("Circular dependency among global variables: "+one.getVariableQName().getDisplayName())+" depends on its own value"));
            }
            System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.GlobalVariable> transitiveDependencies = Saxon.Impl.Helpers.MapUtils.Get(globalVariableDependencies, two);
            if ((transitiveDependencies != null)) {
                if (transitiveDependencies.Contains(one)) {
                    throw new Saxon.Hej.trans.XPathException.Circularity(((((("Circular dependency among variables: "+one.getVariableQName().getDisplayName())+" depends on the value of ")+two.getVariableQName().getDisplayName())+", which depends directly or indirectly on the value of ")+one.getVariableQName().getDisplayName()));
                }
                foreach (Saxon.Hej.expr.instruct.GlobalVariable var in transitiveDependencies) {
                    registerGlobalVariableDependency(one, var);
                }
            }
            System.Collections.Generic.ISet<Saxon.Hej.expr.instruct.GlobalVariable> existingDependencies = Saxon.Impl.Helpers.MapUtils.Get(globalVariableDependencies, one);
            if ((existingDependencies == null)) {
                existingDependencies = new System.Collections.Generic.HashSet<Saxon.Hej.expr.instruct.GlobalVariable>();
                Saxon.Impl.Helpers.MapUtils.Put(globalVariableDependencies, one, existingDependencies);
            }
            existingDependencies.Add(two);
        }}
        public void setCurrentDateTime(Saxon.Hej.value.DateTimeValue dateTime) {
            if ((currentDateTime == null)) {
                if ((dateTime.getComponent(Saxon.Hej.functions.AccessorFn.Component.TIMEZONE) == null)) {
                    throw new Saxon.Hej.trans.XPathException("No timezone is present in supplied value of current date/time");
                }
                currentDateTime = dateTime;
                dateTimePreset = true;
            } else {
                throw new Saxon.Ejava.lang.IllegalStateException("Current date and time can only be set once, and cannot subsequently be changed");
            }
        }
        public Saxon.Hej.value.DateTimeValue getCurrentDateTime() {
            if ((currentDateTime == null)) {
                currentDateTime = Saxon.Hej.value.DateTimeValue.now();
            }
            return currentDateTime;
        }
        public int getImplicitTimezone() {
            return getCurrentDateTime().getTimezoneInMinutes();
        }
        public Saxon.Hej.expr.XPathContextMajor newXPathContext() {
            Saxon.Hej.expr.XPathContextMajor c = new Saxon.Hej.expr.XPathContextMajor(this);
            c.setCurrentOutputUri(principalResultURI);
            return c;
        }
        public void setUseDocumentProjection(Saxon.Hej.expr.parser.PathMap pathMap) {
            this.pathMap = pathMap;
        }
        public Saxon.Hej.expr.parser.PathMap getPathMapForDocumentProjection() {
            return pathMap;
        }
        internal  /* REPLACING public */ Saxon.Hej.trans.StylesheetCache getStylesheetCache() { lock(this) {
            if ((stylesheetCache == null)) {
                this.stylesheetCache = new Saxon.Hej.trans.StylesheetCache();
            }
            return stylesheetCache;
        }}
        public System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> getFocusTrackerFactory(bool multithreaded) {
            return ((multithreaded && (multiThreadedFocusTrackerFactory != null)) ? multiThreadedFocusTrackerFactory : focusTrackerFactory);
        }
        public void setFocusTrackerFactory(System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> focusTrackerFactory) {
            this.focusTrackerFactory = focusTrackerFactory;
        }
        public void setMultithreadedFocusTrackerFactory(System.Func<Saxon.Hej.om.SequenceIterator,Saxon.Hej.om.FocusTrackingIterator> focusTrackerFactory) {
            this.multiThreadedFocusTrackerFactory = focusTrackerFactory;
        }
        public void setMemoizingFocusTrackerFactory() {
            setFocusTrackerFactory(@base => {
                Saxon.Hej.om.FocusTrackingIterator fti;
                if (((!((((@base is Saxon.Hej.tree.iter.GroundedIterator) && ((Saxon.Hej.tree.iter.GroundedIterator)(@base)).isActuallyGrounded()))) && !(((@base is Saxon.Hej.expr.sort.GroupIterator)))) && !(((@base is Saxon.Hej.regex.RegexIterator))))) {
                    try {
                        Saxon.Hej.om.MemoSequence ms = new Saxon.Hej.om.MemoSequence(@base);
                        fti = Saxon.Hej.om.FocusTrackingIterator.track(ms.iterate());
                    } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                        fti = Saxon.Hej.om.FocusTrackingIterator.track(@base);
                    }
                } else {
                    fti = Saxon.Hej.om.FocusTrackingIterator.track(@base);
                }
                return fti;
            } );
        }
    
    }
}
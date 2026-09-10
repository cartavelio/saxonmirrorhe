using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class CopyOfFeed : Saxon.Eej.ee.stream.feed.AbsorptionFeed {
        private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private bool copyNamespaces = true;
        private bool copyAccumulators = true;
        private int validation = Saxon.Hej.lib.Validation.PRESERVE;
        private Saxon.Hej.type.SchemaType type = null;
        private Saxon.Hej.trans.XPathException validationError;
        private Saxon.Hej.@event.ComplexContentOutputter proxy;
        public static Saxon.Eej.ee.stream.watch.Trigger getWatch(Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.pattern.Pattern selection, Saxon.Eej.ee.stream.watch.WatchManager wm, Saxon.Hej.expr.XPathContext context) {
            Saxon.Eej.ee.stream.feed.CopyOfFeed action = new Saxon.Eej.ee.stream.feed.CopyOfFeed(wm, result, context);
            Saxon.Eej.ee.stream.watch.Trigger watch = new Saxon.Eej.ee.stream.watch.Trigger(selection, action, context);
            watch.setPipelineConfiguration(wm.getPipelineConfiguration());
            return watch;
        }
        public CopyOfFeed(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Eej.ee.stream.feed.ItemFeed result, Saxon.Hej.expr.XPathContext context) : base (result, context) {
            this.watchManager = watchManager;
            Saxon.Hej.@event.Receiver writer = new Saxon.Eej.ee.stream.StreamingSequenceWriter(getPipelineConfiguration(), result);
            proxy = new Saxon.Hej.@event.ComplexContentOutputter(writer);
        }
        public override void close() {
            if (hasFailed()) {
                if ((validationError != null)) {
                    dynamicError(validationError);
                } else {
                    dynamicError(new Saxon.Hej.trans.XPathException("Streamed copy-of has failed. A validation error, perhaps?)"));
                }
            }
            base.close();
        }
        public void setCopyNamespaces(bool copy) {
            this.copyNamespaces = copy;
        }
        public void setCopyAccumulators(bool copy) {
            this.copyAccumulators = copy;
        }
        public void setValidationOptions(int validation, Saxon.Hej.type.SchemaType type) {
            this.validation = validation;
            this.type = type;
        }
        protected internal override Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem makePendingItem(int sequenceNr, Saxon.Eej.ee.stream.om.FleetingParentNode node) {
            return new Saxon.Eej.ee.stream.feed.CopyOfFeed.PendingCopyAction(this, sequenceNr, node);
        }
        protected internal override Saxon.Hej.om.Sequence processGroundedItem(Saxon.Hej.om.Item item) {
            Saxon.Hej.s9api.Location loc = Saxon.Hej.expr.parser.Loc.NONE;
            if ((item is Saxon.Hej.om.NodeInfo)) {
                int kind = ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind();
                if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    Saxon.Hej.Configuration config = getContext().getConfiguration();
                    Saxon.Hej.tree.util.Orphan orphan = new Saxon.Hej.tree.util.Orphan(config);
                    orphan.setNodeKind((short)(node.getNodeKind()));
                    orphan.setNodeName(Saxon.Hej.om.NameOfNode.makeName(node));
                    orphan.setStringValue(item.getUnicodeStringValue());
                    orphan.setTypeAnnotation(node.getSchemaType());
                    if (((validation != Saxon.Hej.lib.Validation.PRESERVE) || (type != null))) {
                        if (((type != null) && !(type.isSimpleType()))) {
                            throw new Saxon.Hej.trans.XPathException("When copying an attribute with schema validation, the requested type must not be a complex type", "XTTE1535", getExpression().getLocation());
                        }
                        Saxon.Hej.type.SchemaType annotation = Saxon.Hej.expr.instruct.CopyOf.validateAttribute(node, (Saxon.Hej.type.SimpleType)(type), validation, getContext());
                        orphan.setTypeAnnotation(annotation);
                    }
                    return orphan;
                } else {
                    if (((kind == Saxon.Hej.type.Type.ELEMENT) || (kind == Saxon.Hej.type.Type.DOCUMENT))) {
                        if (!(copyNamespaces)) {
                            Saxon.Hej.@event.SequenceCollector writer = new Saxon.Hej.@event.SequenceCollector(getPipelineConfiguration());
                            ((Saxon.Hej.om.NodeInfo)(item)).copy(writer, 0, loc);
                            return writer.getFirstItem();
                        }
                        if (((validation != Saxon.Hej.lib.Validation.PRESERVE) || (type != null))) {
                            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                            Saxon.Hej.lib.ParseOptions validationOptions = new Saxon.Hej.lib.ParseOptions();
                            validationOptions.setSchemaValidationMode(validation);
                            validationOptions.setTopLevelType(type);
                            Saxon.Hej.Configuration config = getPipelineConfiguration().getConfiguration();
                            Saxon.Hej.@event.SequenceCollector writer = new Saxon.Hej.@event.SequenceCollector(getPipelineConfiguration());
                            Saxon.Hej.@event.Receiver copier;
                            if ((kind == Saxon.Hej.type.Type.DOCUMENT)) {
                                copier = config.getDocumentValidator(writer, node.getBaseURI(), validationOptions, loc);
                            } else {
                                validationOptions.setTopLevelElement(Saxon.Hej.om.NameOfNode.makeName(node).getStructuredQName());
                                copier = config.getElementValidator(writer, validationOptions, loc);
                            }
                            node.copy(copier, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, loc);
                            return writer.getFirstItem();
                        }
                    }
                }
            }
            return item;
        }
        public override void dynamicError(Saxon.Hej.trans.XPathException error) {
            base.dynamicError(error);
            proxy = new Saxon.Hej.@event.ComplexContentOutputter(new Saxon.Hej.@event.Sink(getPipelineConfiguration()));
        }
        public override void startDocument(int properties) {
            getNextOutputter().startDocument(properties);
        }
        public override void endDocument() {
            getNextOutputter().endDocument();
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            getNextOutputter().setUnparsedEntity(name, systemID, publicID);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().startElement(elemName, typeCode, location, properties);
        }
        public override void @namespace(string prefix, string namespaceUri, int properties) {
            getNextOutputter().@namespace(prefix, namespaceUri, properties);
        }
        public override void attribute(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType typeCode, string value, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().attribute(attName, typeCode, value, location, properties);
        }
        public override void startContent() {
            getNextOutputter().startContent();
        }
        public override void endElement() {
            getNextOutputter().endElement();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().characters(chars, location, properties);
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().processingInstruction(name, data, location, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().comment(content, location, properties);
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            getNextOutputter().startElement(elemName, type, attributes, namespaces, location, properties);
        }
        private class PendingCopyAction : Saxon.Eej.ee.stream.feed.AbsorptionFeed.PendingItem {
            private Saxon.Hej.tree.tiny.TinyBuilder builder;
            private Saxon.Hej.@event.Receiver destination;
            private readonly Saxon.Eej.ee.stream.om.FleetingParentNode rootOfCopy;
            public readonly Saxon.Eej.ee.stream.feed.CopyOfFeed feed;
            public PendingCopyAction(Saxon.Eej.ee.stream.feed.CopyOfFeed feed, int sequence, Saxon.Eej.ee.stream.om.FleetingParentNode node) : base (sequence) {
                this.feed = feed;
                this.rootOfCopy = node;
                Saxon.Hej.@event.PipelineConfiguration pipe = feed.getPipelineConfiguration();
                if ((feed.valuesReadyToBeOutput.Count==0 && (((feed.getResultFeed() is Saxon.Eej.ee.stream.feed.DecomposingFeed) || (feed.getResultFeed() is Saxon.Eej.ee.stream.feed.ComplexNodeEventFeed))))) {
                    Saxon.Hej.@event.Outputter r = feed.getNextOutputter();
                    destination = new Inner_ProxyOutputter_d333e1640(this, r);
                } else {
                    builder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
                    builder.setStatistics(pipe.getConfiguration().getTreeStatistics().TEMPORARY_TREE_STATISTICS);
                    destination = builder;
                    if (feed.copyAccumulators) {
                        destination = new Saxon.Eej.ee.stream.feed.CopyOfFeed.AccumulatorCopier(feed.watchManager, builder, builder);
                    }
                }
            }
            public override Saxon.Hej.@event.Receiver getGatherer() {
                Saxon.Hej.@event.Receiver copier = destination;
                if (!(feed.copyNamespaces)) {
                    copier = new Inner_ProxyReceiver_d333e1704(this, copier);
                }
                if (((feed.validation != Saxon.Hej.lib.Validation.PRESERVE) || (feed.type != null))) {
                    Saxon.Hej.lib.ParseOptions validationOptions = new Saxon.Hej.lib.ParseOptions();
                    validationOptions.setSchemaValidationMode(feed.validation);
                    validationOptions.setTopLevelType(feed.type);
                    Saxon.Hej.Configuration config = feed.getPipelineConfiguration().getConfiguration();
                    Saxon.Hej.s9api.Location loc = ((feed.getExpression() == null) ? Saxon.Hej.expr.parser.Loc.NONE : feed.getExpression().getLocation());
                    if ((rootOfCopy.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                        copier = new Saxon.Hej.@event.ComplexContentOutputter(config.getDocumentValidator(copier, rootOfCopy.getBaseURI(), validationOptions, loc));
                    } else {
                        validationOptions.setTopLevelElement(Saxon.Hej.om.NameOfNode.makeName(rootOfCopy).getStructuredQName());
                        try {
                            copier = new Saxon.Hej.@event.ComplexContentOutputter(config.getElementValidator(copier, validationOptions, loc));
                        } catch (Saxon.Hej.trans.XPathException e) {
                            feed.dynamicError(e);
                            return null;
                        }
                    }
                    Saxon.Hej.@event.PipelineConfiguration newPipe = new Saxon.Hej.@event.PipelineConfiguration(feed.getPipelineConfiguration());
                    newPipe.setErrorReporter(new Inner_ErrorReporter_d333e2051(this));
                    copier.setPipelineConfiguration(newPipe);
                    copier = new Saxon.Eej.ee.validate.CatchingReceiver(copier, newPipe.getErrorReporter());
                }
                return copier;
            }
            public override Saxon.Hej.om.Sequence deliver() {
                if (feed.hasFailed()) {
                    if ((feed.validationError != null)) {
                        throw feed.validationError;
                    } else {
                        throw new Saxon.Hej.trans.XPathException("Streamed copy-of has failed. Perhaps a validation error?");
                    }
                }
                if ((builder != null)) {
                    return builder.getCurrentRoot();
                } else {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            }
            public override void rollback() {
            }
        
            private class Inner_ProxyOutputter_d333e1640 : Saxon.Hej.@event.ProxyOutputter {
                private readonly PendingCopyAction outerInstance;
                public Inner_ProxyOutputter_d333e1640(PendingCopyAction outerInstance, /*AA*/Saxon.Hej.@event.Outputter arg1) : base(arg1) {
                    this.outerInstance = outerInstance;
                }
                public override void open() {
                }
                public override void close() {
                }
            }

            private class Inner_ProxyReceiver_d333e1704 : Saxon.Hej.@event.ProxyReceiver {
                private readonly PendingCopyAction outerInstance;
                public Inner_ProxyReceiver_d333e1704(PendingCopyAction outerInstance, /*AA*/Saxon.Hej.@event.Receiver arg1) : base(arg1) {
                    this.outerInstance = outerInstance;
                }
                public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                    Saxon.Hej.om.NamespaceMap map = Saxon.Hej.om.NamespaceMap.emptyMap();
                    if (!(/*localparam*/elemName.getURI().Length==0)) {
                        /*localvar*/map = /*localvar*/map.put(/*localparam*/elemName.getPrefix(), /*localparam*/elemName.getURI());
                    }
                    foreach (Saxon.Hej.om.AttributeInfo att in /*localparam*/attributes) {
                        Saxon.Hej.om.NodeName name = /*localvar*/att.getNodeName();
                        if (!(/*localvar*/name.getURI().Length==0)) {
                            /*localvar*/map = /*localvar*/map.put(/*localvar*/name.getPrefix(), /*localvar*/name.getURI());
                        }
                    }
                    base.startElement(/*localparam*/elemName, /*localparam*/type, /*localparam*/attributes, /*localvar*/map, /*localparam*/location, /*localparam*/properties);
                }
            }

            private class Inner_ErrorReporter_d333e2051 : Saxon.Hej.lib.ErrorReporter {
                private readonly PendingCopyAction outerInstance;
                public Inner_ErrorReporter_d333e2051(PendingCopyAction outerInstance) {
                    this.outerInstance = outerInstance;
                    errorCount = 0;
                }
                private int errorCount;
                public void report(Saxon.Hej.s9api.XmlProcessingError error) {
                    if ((!(/*localparam*/error.isWarning()) && (/*field*/errorCount++ == 0))) {
                        outerInstance.feed.validationError = Saxon.Hej.trans.XPathException.fromXmlProcessingError(/*localparam*/error);
                        try {
                            outerInstance.feed.dynamicError(outerInstance.feed.validationError);
                        } catch (Saxon.Hej.trans.XPathException e) {
                        }
                    }
                }
            }

        }
        public class AccumulatorCopier : Saxon.Hej.@event.ProxyReceiver {
            private readonly Saxon.Eej.ee.stream.watch.WatchManager watchManager;
            private readonly Saxon.Hej.tree.tiny.TinyBuilder builder;
            private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.feed.CopyOfFeed.CopiedAccumulatorData> dataMap = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.accum.Accumulator, Saxon.Eej.ee.stream.feed.CopyOfFeed.CopiedAccumulatorData>();
            public AccumulatorCopier(Saxon.Eej.ee.stream.watch.WatchManager watchManager, Saxon.Hej.@event.Receiver next, Saxon.Hej.tree.tiny.TinyBuilder builder) : base (next) {
                this.builder = builder;
                this.watchManager = watchManager;
                Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
                if ((multiAccumulatorWatch != null)) {
                    System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> accumulators = multiAccumulatorWatch.getAccumulatorMap();
                    foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulators.Values) {
                        Saxon.Impl.Helpers.MapUtils.Put(dataMap, watch.getAccumulator(), new Saxon.Eej.ee.stream.feed.CopyOfFeed.CopiedAccumulatorData(watch.getAccumulator()));
                    }
                }
            }
            internal int depth = 0;
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                base.startElement(elemName, type, attributes, namespaces, location, properties);
                depth = watchManager.getDepth();
            }
            public override void endElement() {
                base.endElement();
                depth--;
                Saxon.Hej.tree.tiny.TinyNodeImpl newNode = builder.getLastCompletedElement();
                Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
                if ((multiAccumulatorWatch != null)) {
                    System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> accumulators = multiAccumulatorWatch.getAccumulatorMap();
                    foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulators.Values) {
                        Saxon.Hej.expr.accum.Accumulator acc = watch.getAccumulator();
                        Saxon.Eej.ee.stream.feed.CopyOfFeed.CopiedAccumulatorData data = Saxon.Impl.Helpers.MapUtils.Get(dataMap, acc);
                        data.addValues(newNode, watch.getPreDescentValueAtDepth(depth), watch.getPostDescentValue(null));
                    }
                }
            }
            public override void close() {
                Saxon.Hej.Controller controller = watchManager.getPipelineConfiguration().getController();
                if ((controller is Saxon.Hej.trans.XsltController)) {
                    Saxon.Hej.expr.accum.AccumulatorManager manager = ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager();
                    Saxon.Hej.om.TreeInfo tree = builder.getTree();
                    Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiAccumulatorWatch = watchManager.getMultiAccumulatorWatch();
                    if ((multiAccumulatorWatch != null)) {
                        System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> accumulatorWatchMap = multiAccumulatorWatch.getAccumulatorMap();
                        foreach (Saxon.Hej.expr.accum.Accumulator acc in Saxon.Impl.Helpers.MapUtils.KeySet(accumulatorWatchMap)) {
                            manager.addAccumulatorData(tree, acc, Saxon.Impl.Helpers.MapUtils.Get(dataMap, acc));
                        }
                    }
                    foreach (Saxon.Eej.ee.stream.feed.CopyOfFeed.CopiedAccumulatorData data in dataMap.Values) {
                        manager.addAccumulatorData(tree, data.getAccumulator(), data);
                    }
                }
            }
        
        }
        private class CopiedAccumulatorData : Saxon.Hej.expr.accum.IAccumulatorData {
            private readonly Saxon.Hej.expr.accum.Accumulator accumulator;
            private readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.om.Sequence> preDescentValues = new Saxon.Hej.z.IntHashMap<Saxon.Hej.om.Sequence>();
            private readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.om.Sequence> postDescentValues = new Saxon.Hej.z.IntHashMap<Saxon.Hej.om.Sequence>();
            public CopiedAccumulatorData(Saxon.Hej.expr.accum.Accumulator acc)  {
                this.accumulator = acc;
            }
            public void addValues(Saxon.Hej.tree.tiny.TinyNodeImpl node, Saxon.Hej.om.Sequence preDescent, Saxon.Hej.om.Sequence postDescent) {
                preDescentValues.put(node.getNodeNumber(), preDescent);
                postDescentValues.put(node.getNodeNumber(), postDescent);
            }
            public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
                return accumulator;
            }
            public Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent) {
                int nodeNr = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).getNodeNumber();
                return (postDescent ? postDescentValues.get(nodeNr) : preDescentValues.get(nodeNr));
            }
        
        }
    
        private class Inner_ProxyOutputter_d333e1640 : Saxon.Hej.@event.ProxyOutputter {
            private readonly PendingCopyAction outerInstance;
            public Inner_ProxyOutputter_d333e1640(PendingCopyAction outerInstance, /*AA*/Saxon.Hej.@event.Outputter arg1) : base(arg1) {
                this.outerInstance = outerInstance;
            }
            public override void open() {
            }
            public override void close() {
            }
        }

        private class Inner_ProxyReceiver_d333e1704 : Saxon.Hej.@event.ProxyReceiver {
            private readonly PendingCopyAction outerInstance;
            public Inner_ProxyReceiver_d333e1704(PendingCopyAction outerInstance, /*AA*/Saxon.Hej.@event.Receiver arg1) : base(arg1) {
                this.outerInstance = outerInstance;
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                Saxon.Hej.om.NamespaceMap map = Saxon.Hej.om.NamespaceMap.emptyMap();
                if (!(/*localparam*/elemName.getURI().Length==0)) {
                    /*localvar*/map = /*localvar*/map.put(/*localparam*/elemName.getPrefix(), /*localparam*/elemName.getURI());
                }
                foreach (Saxon.Hej.om.AttributeInfo att in /*localparam*/attributes) {
                    Saxon.Hej.om.NodeName name = /*localvar*/att.getNodeName();
                    if (!(/*localvar*/name.getURI().Length==0)) {
                        /*localvar*/map = /*localvar*/map.put(/*localvar*/name.getPrefix(), /*localvar*/name.getURI());
                    }
                }
                base.startElement(/*localparam*/elemName, /*localparam*/type, /*localparam*/attributes, /*localvar*/map, /*localparam*/location, /*localparam*/properties);
            }
        }

        private class Inner_ErrorReporter_d333e2051 : Saxon.Hej.lib.ErrorReporter {
            private readonly PendingCopyAction outerInstance;
            public Inner_ErrorReporter_d333e2051(PendingCopyAction outerInstance) {
                this.outerInstance = outerInstance;
                errorCount = 0;
            }
            private int errorCount;
            public void report(Saxon.Hej.s9api.XmlProcessingError error) {
                if ((!(/*localparam*/error.isWarning()) && (/*field*/errorCount++ == 0))) {
                    outerInstance.feed.validationError = Saxon.Hej.trans.XPathException.fromXmlProcessingError(/*localparam*/error);
                    try {
                        outerInstance.feed.dynamicError(outerInstance.feed.validationError);
                    } catch (Saxon.Hej.trans.XPathException e) {
                    }
                }
            }
        }

    }
}
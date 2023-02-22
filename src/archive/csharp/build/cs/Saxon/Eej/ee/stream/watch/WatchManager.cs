using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class WatchManager : Saxon.Hej.@event.SequenceReceiver, Saxon.Hej.om.NamespaceResolver {
        private const bool TRACING = false;
        private bool startDocumentPending;
        private int startDocumentProperties;
        private readonly Saxon.Eej.ee.stream.om.FleetingDocumentNode documentNode;
        private Saxon.Eej.ee.stream.om.FleetingParentNode[] elementStack = new Saxon.Eej.ee.stream.om.FleetingParentNode[20];
        private int[] receiverCountStack = new int[20];
        protected internal int elementStackTop = 0;
        private System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> namespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        protected internal System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.Watch> watchList = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.Watch>(10);
        protected internal System.Collections.Generic.Stack<System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction>> activeWatchStack = new System.Collections.Generic.Stack<System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction>>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.@event.Receiver> activeReceiverStack = new System.Collections.Generic.Stack<Saxon.Hej.@event.Receiver>();
        private Saxon.Hej.s9api.Location lastLocationId = Saxon.Hej.expr.parser.Loc.NONE;
        private Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.@event.Outputter outputter;
        private Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiAccumulatorWatch;
        public class GroupingScope {
            public bool isActive = true;
            public System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.Watch> watches = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.Watch>();
            public void addWatch(Saxon.Eej.ee.stream.watch.Watch watch) {
                if (isActive) {
                    watches.Add(watch);
                }
            }
        
        }
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope> capturingScopes = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope>();
        public WatchManager(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            context = config.getConversionContext();
            long docNr = config.getDocumentNumberAllocator().allocateStreamedDocumentNumber();
            documentNode = new Saxon.Eej.ee.stream.om.FleetingDocumentNode(docNr);
            documentNode.setConfiguration(config);
            pipe.setComponent("com.saxonica.ee.stream.watch.WatchManager", this);
        }
        public void setXPathContext(Saxon.Hej.expr.XPathContext context) {
            this.context = context;
        }
        public Saxon.Hej.expr.XPathContext getXPathContext() {
            return context;
        }
        public void setOutputter(Saxon.Hej.@event.Outputter outputter) {
            outputter.setPipelineConfiguration(getPipelineConfiguration());
            this.outputter = outputter;
        }
        public Saxon.Hej.@event.Outputter getOutputter() {
            return outputter;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("WatchManager does not implement append()");
        }
        public override void close() {
        }
        public override void startDocument(int properties) {
            if ((elementStackTop == 0)) {
                startDocumentPending = true;
                startDocumentProperties = properties;
                namespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
            }
        }
        private void deferredStartDocument() {
            documentNode.setSystemId(getSystemId());
            documentNode.setMultiAccumulatorWatch(multiAccumulatorWatch);
            elementStack[elementStackTop] = documentNode;
            receiverCountStack[elementStackTop] = 0;
            elementStackTop++;
            System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction> activeWatchList = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction>(4);
            activeWatchStack.Push(activeWatchList);
            for (int i = 0; (i<watchList.Count); i++) {
                Saxon.Eej.ee.stream.watch.Watch watch = watchList[i];
                bool matches = watch.matchesNode(documentNode, context);
                if (matches) {
                    Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = new Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction(watch, Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.DEACTIVATE);
                    activeWatchList.Add(action);
                    Saxon.Hej.@event.Receiver @out = watch.startSelectedParentNode(documentNode, Saxon.Hej.expr.parser.Loc.NONE);
                    if ((@out != null)) {
                        @out.open();
                        activeReceiverStack.Push(@out);
                        receiverCountStack[(elementStackTop-1)]++;
                    }
                }
            }
            foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                copyUnparsedEntities(@out);
                @out.startDocument(startDocumentProperties);
            }
            documentNode.copyInitialCommentsAndProcessingInstructions(this);
        }
        private void copyUnparsedEntities(Saxon.Hej.@event.Receiver @out) {
            if ((documentNode != null)) {
                System.Collections.Generic.IEnumerator<string> names = documentNode.getUnparsedEntityNames();
                while (names.MoveNext() /*UNSAFE CONVERSION*/) {
                    string name = names.Current;
                    string[] entity = documentNode.getUnparsedEntity(name);
                    @out.setUnparsedEntity(name, entity[0], entity[1]);
                }
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elementName, Saxon.Hej.type.SchemaType typeCode, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            try {
                if (startDocumentPending) {
                    startDocumentPending = false;
                    deferredStartDocument();
                }
                Saxon.Hej.type.ValidationException ve = null;
                ensureStackSpace();
                Saxon.Eej.ee.stream.om.FleetingElementNode element = new Saxon.Eej.ee.stream.om.FleetingElementNode(elementName, typeCode);
                Saxon.Eej.ee.stream.om.FleetingParentNode parent = ((elementStackTop>0) ? elementStack[(elementStackTop-1)] : documentNode);
                element.setParent(parent);
                element.setNamespaceMap(namespaces);
                element.setAttributes(attributes);
                element.setHasChildren(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.HAS_CHILDREN));
                elementStack[elementStackTop] = element;
                receiverCountStack[elementStackTop] = 0;
                namespaceStack.Push(namespaces);
                elementStackTop++;
                System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction> activeWatchList = new System.Collections.Generic.List<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction>(4);
                activeWatchStack.Push(activeWatchList);
                addPrearrangedWatches();
                for (int i = 0; (i<watchList.Count); i++) {
                    Saxon.Eej.ee.stream.watch.Watch watch = watchList[i];
                    bool matches = watch.matchesNode(element, context);
                    if (matches) {
                        Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = new Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction(watch, Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.DEACTIVATE);
                        activeWatchList.Add(action);
                        Saxon.Hej.@event.Receiver @out = watch.startSelectedParentNode(element, location);
                        if ((@out != null)) {
                            @out.open();
                            activeReceiverStack.Push(@out);
                            receiverCountStack[(elementStackTop-1)]++;
                        }
                    }
                }
                foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                    try {
                        @out.startElement(elementName, typeCode, attributes, namespaces, location, properties);
                    } catch (Saxon.Hej.type.ValidationException err) {
                        ve = err;
                    }
                }
                if ((ve != null)) {
                    throw ve;
                }
                lastLocationId = location.saveLocation();
            } catch (Saxon.Hej.trans.QuitParsingException quit) {
                while (!((namespaceStack.Count==0))) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw;
            }
            processAttributes(attributes);
        }
        public void restartElement(Saxon.Eej.ee.stream.watch.Watch watch) {
            try {
                if (startDocumentPending) {
                    startDocumentPending = false;
                    deferredStartDocument();
                }
                Saxon.Hej.type.ValidationException ve = null;
                ensureStackSpace();
                Saxon.Eej.ee.stream.om.FleetingElementNode element = (Saxon.Eej.ee.stream.om.FleetingElementNode)(elementStack[(elementStackTop-1)]);
                System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction> activeWatchList = activeWatchStack.Peek();
                bool matches = watch.matchesNode(element, context);
                if (matches) {
                    Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = new Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction(watch, Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.DEACTIVATE);
                    activeWatchList.Add(action);
                    Saxon.Hej.@event.Receiver @out = watch.startSelectedParentNode(element, element);
                    if ((@out != null)) {
                        @out.open();
                        try {
                            @out.startElement(element.getNodeName(), element.getSchemaType(), element.getAttributes(), element.getAllNamespaces(), lastLocationId, 0);
                        } catch (Saxon.Hej.type.ValidationException err) {
                            ve = err;
                        }
                        activeReceiverStack.Push(@out);
                        receiverCountStack[(elementStackTop-1)]++;
                    }
                }
                if ((ve != null)) {
                    throw ve;
                }
            } catch (Saxon.Hej.trans.QuitParsingException quit) {
                while (!((namespaceStack.Count==0))) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw;
            }
        }
        private void processAttributes(Saxon.Hej.om.AttributeMap attributes) {
            foreach (Saxon.Hej.om.AttributeInfo attribute in attributes) {
                try {
                    Saxon.Eej.ee.stream.om.FleetingNode att = null;
                    Saxon.Hej.om.NodeName nameCode = attribute.getNodeName();
                    foreach (Saxon.Eej.ee.stream.watch.Watch watch in watchList) {
                        if (watch.matchesNodesOfKind(Saxon.Hej.type.UType.ATTRIBUTE)) {
                            if ((att == null)) {
                                att = new Saxon.Eej.ee.stream.om.FleetingNode();
                                att.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
                                att.setNodeName(nameCode);
                                att.setTypeAnnotation(attribute.getType());
                                att.setParent(elementStack[(elementStackTop-1)]);
                                att.setStringValue(Saxon.Hej.str.StringView.tidy(attribute.getValue()));
                            }
                            bool matches = watch.matchesNode(att, context);
                            if (matches) {
                                watch.processItem(att);
                            }
                        }
                    }
                    lastLocationId = attribute.getLocation();
                } catch (Saxon.Hej.trans.QuitParsingException quit) {
                    while (!((namespaceStack.Count==0))) {
                        endElement();
                    }
                    endDocument();
                    warning(quit);
                    throw;
                }
            }
        }
        public int getDepth() {
            return elementStackTop;
        }
        private void ensureStackSpace() {
            if (((elementStackTop+1)>elementStack.Length)) {
                elementStack = Saxon.Ejava.util.Arrays.copyOf(elementStack, (elementStackTop*2));
                receiverCountStack = Saxon.Ejava.util.Arrays.copyOf(receiverCountStack, (elementStackTop*2));
            }
        }
        public Saxon.Eej.ee.stream.om.FleetingParentNode getCurrentNode() {
            return elementStack[(elementStackTop-1)];
        }
        protected internal virtual void addPrearrangedWatches() {
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            try {
                ensureStackSpace();
                Saxon.Eej.ee.stream.om.FleetingNode node = null;
                Saxon.Eej.ee.stream.om.FleetingParentNode parent = ((elementStackTop>0) ? elementStack[(elementStackTop-1)] : documentNode);
                foreach (Saxon.Eej.ee.stream.watch.Watch watch in watchList) {
                    if (watch.matchesNodesOfKind(Saxon.Hej.type.UType.TEXT)) {
                        if ((node == null)) {
                            node = new Saxon.Eej.ee.stream.om.FleetingNode();
                            node.setNodeKind(Saxon.Hej.type.Type.TEXT);
                            node.setParent(parent);
                            node.setStringValue(chars.tidy());
                            node.setTypeAnnotation(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                        }
                        bool matches = watch.matchesNode(node, context);
                        if (matches) {
                            watch.processItem(node);
                        }
                    }
                }
                foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                    @out.characters(chars, locationId, properties);
                }
                lastLocationId = locationId;
            } catch (Saxon.Hej.trans.QuitParsingException quit) {
                while (!((namespaceStack.Count==0))) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw;
            }
        }
        private void warning(Saxon.Hej.trans.QuitParsingException exception) {
            Saxon.Hej.trans.XmlProcessingException err = new Saxon.Hej.trans.XmlProcessingException(exception);
            err.setWarning(true);
            getPipelineConfiguration().getErrorReporter().report(err);
        }
        public override void endElement() {
            foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                @out.endElement();
            }
            elementStackTop--;
            if (!((namespaceStack.Count==0))) {
                namespaceStack.Pop();
            }
            int drop = receiverCountStack[elementStackTop];
            receiverCountStack[elementStackTop] = 0;
            for (int i = 0; (i<drop); i++) {
                Saxon.Hej.@event.Receiver @out = activeReceiverStack.Pop();
                @out.close();
            }
            System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction> activeWatchList = activeWatchStack.Pop();
            Saxon.Hej.trans.QuitParsingException quit = null;
            int[] doItWhen = new int[activeWatchList.Count];
            for (int i = 0; (i<activeWatchList.Count); i++) {
                Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = activeWatchList[i];
                if (((action.watch is Saxon.Eej.ee.stream.watch.Trigger) && (((Saxon.Eej.ee.stream.watch.Trigger)(action.watch)).getAction() is Saxon.Eej.ee.stream.feed.SnapshotFeed))) {
                    doItWhen[i] = 0;
                } else {
                    if ((action.watch is Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch)) {
                        doItWhen[i] = 1;
                    } else {
                        doItWhen[i] = 2;
                    }
                }
            }
            for (int phase = 0; (phase<=2); phase++) {
                for (int i = (activeWatchList.Count-1); (i>=0); i--) {
                    Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = activeWatchList[i];
                    if ((doItWhen[i] == phase)) {
                        if ((action.action == Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.DEACTIVATE)) {
                            try {
                                action.watch.endSelectedParentNode(lastLocationId);
                            } catch (Saxon.Hej.trans.QuitParsingException q) {
                                quit = q;
                            }
                        } else {
                            try {
                                action.watch.close();
                            } catch (Saxon.Hej.trans.QuitParsingException q) {
                                quit = q;
                            }
                            Saxon.Impl.Helpers.ListUtils.Remove(watchList, action.watch);
                        }
                    }
                }
            }
            if ((quit != null)) {
                while (!((namespaceStack.Count==0))) {
                    endElement();
                }
                endDocument();
                warning(quit);
                throw quit;
            }
        }
        public override void endDocument() {
            elementStackTop--;
            if ((elementStackTop == 0)) {
                foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                    @out.endDocument();
                }
                int drop = receiverCountStack[elementStackTop];
                receiverCountStack[elementStackTop] = 0;
                for (int i = 0; (i<drop); i++) {
                    Saxon.Hej.@event.Receiver @out = activeReceiverStack.Pop();
                    @out.close();
                }
                System.Collections.Generic.IList<Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction> activeWatchList = activeWatchStack.Pop();
                for (int i = (activeWatchList.Count-1); (i>=0); i--) {
                    Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction action = activeWatchList[i];
                    if ((action.action == Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.DEACTIVATE)) {
                        action.watch.endSelectedParentNode(lastLocationId);
                    } else {
                        action.watch.close();
                        Saxon.Impl.Helpers.ListUtils.Remove(watchList, action.watch);
                    }
                }
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            ensureStackSpace();
            Saxon.Eej.ee.stream.om.FleetingNode node = new Saxon.Eej.ee.stream.om.FleetingNode();
            Saxon.Eej.ee.stream.om.FleetingParentNode parent = ((elementStackTop>0) ? elementStack[(elementStackTop-1)] : documentNode);
            node.setNodeKind(Saxon.Hej.type.Type.COMMENT);
            node.setParent(parent);
            node.setStringValue(chars.tidy());
            node.setTypeAnnotation(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
            if (startDocumentPending) {
                documentNode.captureInitialCommentOrProcessingInstruction(node);
            } else {
                foreach (Saxon.Eej.ee.stream.watch.Watch watch in watchList) {
                    bool matches = watch.matchesNode(node, context);
                    if (matches) {
                        watch.processItem(node);
                    }
                }
                foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                    @out.comment(chars, locationId, properties);
                }
                lastLocationId = locationId;
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            ensureStackSpace();
            Saxon.Eej.ee.stream.om.FleetingNode node = new Saxon.Eej.ee.stream.om.FleetingNode();
            Saxon.Eej.ee.stream.om.FleetingParentNode parent = ((elementStackTop>0) ? elementStack[(elementStackTop-1)] : documentNode);
            node.setNodeKind(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
            node.setParent(parent);
            Saxon.Hej.om.NoNamespaceName nodeName = new Saxon.Hej.om.NoNamespaceName(target);
            node.setNodeName(nodeName);
            node.setStringValue(data.tidy());
            node.setTypeAnnotation(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
            if (startDocumentPending) {
                documentNode.captureInitialCommentOrProcessingInstruction(node);
            } else {
                foreach (Saxon.Eej.ee.stream.watch.Watch watch in watchList) {
                    bool matches = watch.matchesNode(node, context);
                    if (matches) {
                        watch.processItem(node);
                    }
                }
                foreach (Saxon.Hej.@event.Receiver @out in activeReceiverStack) {
                    @out.processingInstruction(target, data, locationId, properties);
                }
                lastLocationId = locationId;
            }
        }
        public override void setUnparsedEntity(string name, string systemID, string publicID) {
            if ((documentNode != null)) {
                documentNode.setUnparsedEntity(name, systemID, publicID);
            }
        }
        public void addWatch(Saxon.Eej.ee.stream.watch.Watch watch, bool autoClose) {
            watchList.Add(watch);
            watch.setPipelineConfiguration(getPipelineConfiguration());
            watch.setNamespaceResolver(this);
            bool foundGroupCapture = false;
            if (!(capturingScopes.Count==0)) {
                foreach (Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope scope in capturingScopes) {
                    if (scope.isActive) {
                        scope.addWatch(watch);
                        watch.setAnchorNode(elementStack[(elementStackTop-1)]);
                        foundGroupCapture = true;
                    }
                }
            }
            if ((!(foundGroupCapture) && (elementStackTop>0))) {
                watch.setAnchorNode(elementStack[(elementStackTop-1)]);
                if (autoClose) {
                    activeWatchStack.Peek().Add(new Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction(watch, Saxon.Eej.ee.stream.watch.WatchManager.ClosedownAction.CLOSE));
                }
            }
            if (autoClose) {
                watch.open(/*134*/Saxon.Eej.ee.stream.watch.Terminator.getInstance());
            }
        }
        public void removeWatch(Saxon.Eej.ee.stream.watch.Watch watch) {
            Saxon.Impl.Helpers.ListUtils.Remove(watchList, watch);
        }
        public string getURIForPrefix(string prefix, bool useDefault) {
            if ((!(useDefault) && prefix.Length==0)) {
                return Saxon.Hej.lib.NamespaceConstant.NULL;
            }
            return namespaceStack.Peek().getURIForPrefix(prefix, useDefault);
        }
        public System.Collections.Generic.IEnumerator<string> iteratePrefixes() {
            return namespaceStack.Peek().iteratePrefixes();
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        public Saxon.Eej.ee.stream.om.FleetingDocumentNode getDocumentNode() {
            return documentNode;
        }
        public Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch getMultiAccumulatorWatch() {
            return multiAccumulatorWatch;
        }
        public void watchAccumulators(Saxon.Eej.ee.stream.watch.MultiAccumulatorWatch multiWatch) {
            this.multiAccumulatorWatch = multiWatch;
            addWatch(multiWatch, true);
        }
        protected internal class ClosedownAction {
            public const int DEACTIVATE = 0;
            public const int CLOSE = 1;
            public Saxon.Eej.ee.stream.watch.Watch watch;
            public int action;
            public ClosedownAction(Saxon.Eej.ee.stream.watch.Watch watch, int action)  {
                this.watch = watch;
                this.action = action;
            }
        
        }
        public Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope startGroupingScope() {
            return new Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope();
        }
        public void endGroupingScope(Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope current) {
            foreach (Saxon.Eej.ee.stream.watch.Watch watch in current.watches) {
                watch.close();
                removeWatch(watch);
            }
        }
        public void startCapturingGroupingWatches(Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope scope) {
            capturingScopes.Add(scope);
        }
        public void stopCapturingGroupingWatches(Saxon.Eej.ee.stream.watch.WatchManager.GroupingScope scope) {
            Saxon.Impl.Helpers.ListUtils.Remove(capturingScopes, scope);
        }
    
    }
}
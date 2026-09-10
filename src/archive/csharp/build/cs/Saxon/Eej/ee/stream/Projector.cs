using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class Projector : Saxon.Hej.@event.ProxyReceiver {
        private readonly System.Collections.Generic.Stack<Saxon.Hej.expr.parser.PathMap.PathMapNodeSet> pathPositionStack;
        private readonly Saxon.Hej.tree.util.IndexedStack<bool> isCopiedStack;
        private int level = 0;
        private int copyAllLevel = System.Int32.MaxValue;
        private int inputNodes = 0;
        private int outputNodes = 0;
        private bool displayStatistics = false;
        private readonly Saxon.Hej.om.NamespaceBinding[] nsStack = new Saxon.Hej.om.NamespaceBinding[20];
        private int nsStackTop = 0;
        private readonly int[] nsCountStack = new int[20];
        private readonly static Saxon.Hej.expr.parser.PathMap.PathMapNodeSet DUMMY_PATHMAP_NODESET = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
        public Projector(Saxon.Hej.expr.parser.PathMap.PathMapRoot pathMapRoot, Saxon.Hej.@event.Receiver next) : base (next) {
            pathPositionStack = new System.Collections.Generic.Stack<Saxon.Hej.expr.parser.PathMap.PathMapNodeSet>();
            isCopiedStack = new Saxon.Hej.tree.util.IndexedStack<bool>();
            pathPositionStack.Push(new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMapRoot));
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            displayStatistics = pipe.getConfiguration().isTiming();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            level++;
            inputNodes++;
            bool elementCopied = false;
            if ((level>copyAllLevel)) {
                base.startElement(elemName, type, attributes, namespaces, location, properties);
                elementCopied = true;
                pathPositionStack.Push(DUMMY_PATHMAP_NODESET);
            } else {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet state = pathPositionStack.Peek();
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet newState = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet();
                bool foundMatch = false;
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode aState in state) {
                    Saxon.Hej.expr.parser.PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in pathMapArcs) {
                        Saxon.Hej.pattern.NodeTest test = arc.getNodeTest();
                        if (test.matches(Saxon.Hej.type.Type.ELEMENT, elemName, type)) {
                            foundMatch = true;
                            Saxon.Hej.expr.parser.PathMap.PathMapNode target = arc.getTarget();
                            newState.Add(target);
                            if ((target.isAtomized() || target.isReturnable())) {
                                copyAllLevel = level;
                            }
                        }
                        if ((arc.getAxis() == Saxon.Hej.om.AxisInfo.DESCENDANT)) {
                            newState.Add(aState);
                        }
                    }
                }
                pathPositionStack.Push(newState);
                if (foundMatch) {
                    base.startElement(elemName, type, attributes, namespaces, location, properties);
                    elementCopied = true;
                }
            }
            if (elementCopied) {
                outputNodes++;
                int pendingNamespaces = 0;
                for (int i = (level-2); (i>=0); i--) {
                    if (isCopiedStack.get(i)) {
                        break ;
                    } else {
                        pendingNamespaces += nsCountStack[i];
                    }
                }
                for (int i = (nsStackTop-pendingNamespaces); (i<nsStackTop); i++) {
                    bool duplicate = false;
                    for (int j = (i+1); (j<nsStackTop); j++) {
                        if (nsStack[j].getPrefix().Equals(nsStack[i].getPrefix())) {
                            duplicate = true;
                            break ;
                        }
                    }
                    if (!(duplicate)) {
                        throw new Saxon.Ejava.lang.UnsupportedOperationException("to be re-implemented");
                    }
                }
            }
            isCopiedStack.push(elementCopied);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            inputNodes++;
            if ((level>=copyAllLevel)) {
                base.characters(chars, locationId, properties);
                outputNodes++;
            } else {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet state = (Saxon.Hej.expr.parser.PathMap.PathMapNodeSet)(pathPositionStack.Peek());
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode pathNode in state) {
                    Saxon.Hej.expr.parser.PathMap.PathMapArc[] pathMapArcs = pathNode.getArcs();
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in pathMapArcs) {
                        Saxon.Hej.pattern.NodeTest test = arc.getNodeTest();
                        if (test.matches(Saxon.Hej.type.Type.TEXT, null, null)) {
                            base.characters(chars, locationId, properties);
                            outputNodes++;
                            return ;
                        }
                    }
                }
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            inputNodes++;
            if ((level>copyAllLevel)) {
                base.comment(chars, locationId, properties);
                outputNodes++;
            } else {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet state = (Saxon.Hej.expr.parser.PathMap.PathMapNodeSet)(pathPositionStack.Peek());
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode aState in state) {
                    Saxon.Hej.expr.parser.PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in pathMapArcs) {
                        Saxon.Hej.pattern.NodeTest test = arc.getNodeTest();
                        if (test.matches(Saxon.Hej.type.Type.COMMENT, null, null)) {
                            base.comment(chars, locationId, properties);
                            outputNodes++;
                            return ;
                        }
                    }
                }
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            inputNodes++;
            if ((level>copyAllLevel)) {
                base.processingInstruction(target, data, locationId, properties);
                outputNodes++;
            } else {
                Saxon.Hej.expr.parser.PathMap.PathMapNodeSet state = (Saxon.Hej.expr.parser.PathMap.PathMapNodeSet)(pathPositionStack.Peek());
                foreach (Saxon.Hej.expr.parser.PathMap.PathMapNode aState in state) {
                    Saxon.Hej.expr.parser.PathMap.PathMapArc[] pathMapArcs = aState.getArcs();
                    foreach (Saxon.Hej.expr.parser.PathMap.PathMapArc arc in pathMapArcs) {
                        Saxon.Hej.pattern.NodeTest test = arc.getNodeTest();
                        if (test.matches(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION, new Saxon.Hej.om.NoNamespaceName(target), null)) {
                            base.processingInstruction(target, data, locationId, properties);
                            outputNodes++;
                            return ;
                        }
                    }
                }
            }
        }
        public override void endElement() {
            bool wasCopied = isCopiedStack.pop();
            if (wasCopied) {
                base.endElement();
            }
            pathPositionStack.Pop();
            level--;
            if ((level<copyAllLevel)) {
                copyAllLevel = System.Int32.MaxValue;
            }
            nsStackTop -= nsCountStack[level];
            nsCountStack[level] = 0;
        }
        public override void endDocument() {
            if (displayStatistics) {
                outputStatistics(getConfiguration().getLogger());
            }
            base.endDocument();
        }
        public void outputStatistics(Saxon.Hej.lib.Logger @out) {
            @out.info(("Document projection for "+getSystemId()));
            @out.info((((((("-- Input nodes "+inputNodes)+"; output nodes ")+outputNodes)+"; reduction = ")+((100-(((100*outputNodes))/inputNodes))))+"%"));
        }
    
    }
}
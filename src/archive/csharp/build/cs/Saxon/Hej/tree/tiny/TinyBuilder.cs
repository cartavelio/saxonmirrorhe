using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TinyBuilder : Saxon.Hej.@event.Builder {
        private enum Eligibility {
            INELIGIBLE,
            PRIMED,
            ELIGIBLE,
        }
        private const int PARENT_POINTER_INTERVAL = 10;
        private Saxon.Hej.tree.tiny.TinyTree tree;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> namespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        private int currentDepth = 0;
        private int nodeNr = 0;
        private bool ended = false;
        private bool noNewNamespaces = true;
        private Saxon.Hej.tree.tiny.Statistics statistics;
        private bool markDefaultedAttributes = false;
        private Saxon.Hej.tree.tiny.TinyBuilder.Eligibility textualElementEligibilityState = Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE;
        private Saxon.Hej.str.UnicodeBuilder commentBuilder = new Saxon.Hej.str.UnicodeBuilder();
        public TinyBuilder(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            statistics = config.getTreeStatistics().TEMPORARY_TREE_STATISTICS;
            markDefaultedAttributes = (config.isExpandAttributeDefaults() && config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.MARK_DEFAULTED_ATTRIBUTES));
        }
        public void setStatistics(Saxon.Hej.tree.tiny.Statistics stats) {
            statistics = stats;
        }
        private int[] prevAtDepth = new int[100];
        private int[] siblingsAtDepth = new int[100];
        private bool isIDElement = false;
        public Saxon.Hej.tree.tiny.TinyTree getTree() {
            return tree;
        }
        public int getCurrentDepth() {
            return currentDepth;
        }
        public override void open() {
            if (started) {
                return ;
            }
            if ((tree == null)) {
                tree = new Saxon.Hej.tree.tiny.TinyTree(config, statistics);
                currentDepth = 0;
                if (lineNumbering) {
                    tree.setLineNumbering();
                }
                uniformBaseURI = true;
                tree.setUniformBaseUri(baseURI);
            }
            base.open();
        }
        public override void startDocument(int properties) {
            if ((((started && !(ended))) || (currentDepth>0))) {
                return ;
            }
            started = true;
            ended = false;
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            System.Diagnostics.Debug.Assert((tt != null));
            currentRoot = new Saxon.Hej.tree.tiny.TinyDocumentImpl(tt);
            Saxon.Hej.tree.tiny.TinyDocumentImpl doc = (Saxon.Hej.tree.tiny.TinyDocumentImpl)(currentRoot);
            doc.setSystemId(getSystemId());
            doc.setBaseURI(getBaseURI());
            currentDepth = 0;
            int nodeNr = tt.addDocumentNode((Saxon.Hej.tree.tiny.TinyDocumentImpl)(currentRoot));
            prevAtDepth[0] = nodeNr;
            prevAtDepth[1] = -1;
            siblingsAtDepth[0] = 0;
            siblingsAtDepth[1] = 0;
            tt.next[nodeNr] = -1;
            currentDepth++;
        }
        public override void endDocument() {
            tree.commentBuffer = commentBuilder.toUnicodeString();
            tree.addNode(Saxon.Hej.type.Type.STOPPER, 0, 0, 0, -1);
            tree.numberOfNodes--;
            if ((currentDepth>1)) {
                return ;
            }
            if (ended) {
                return ;
            }
            ended = true;
            prevAtDepth[currentDepth] = -1;
            currentDepth--;
        }
        public override void reset() {
            base.reset();
            tree = null;
            currentDepth = 0;
            nodeNr = 0;
            ended = false;
            statistics = config.getTreeStatistics().TEMPORARY_TREE_STATISTICS;
        }
        public override void close() {
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            if ((tt != null)) {
                tree.commentBuffer = commentBuilder.toUnicodeString();
                tree.textBuffer.close();
                tt.addNode(Saxon.Hej.type.Type.STOPPER, 0, 0, 0, -1);
                tt.condense(statistics);
            }
            base.close();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            System.Diagnostics.Debug.Assert((tt != null));
            textualElementEligibilityState = Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE;
            noNewNamespaces = true;
            if ((namespaceStack.Count==0)) {
                noNewNamespaces = false;
                namespaceStack.Push(namespaces);
            } else {
                noNewNamespaces = (namespaces == namespaceStack.Peek());
                namespaceStack.Push(namespaces);
            }
            if ((siblingsAtDepth[currentDepth]>PARENT_POINTER_INTERVAL)) {
                nodeNr = tt.addNode(Saxon.Hej.type.Type.PARENT_POINTER, currentDepth, prevAtDepth[(currentDepth-1)], 0, 0);
                int prev = prevAtDepth[currentDepth];
                if ((prev>0)) {
                    tt.next[prev] = nodeNr;
                }
                tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
                prevAtDepth[currentDepth] = nodeNr;
                siblingsAtDepth[currentDepth] = 0;
            }
            int fp = elemName.obtainFingerprint(namePool);
            int prefixCode = tree.prefixPool.obtainPrefixCode(elemName.getPrefix());
            int nameCode = (((prefixCode<<20))|fp);
            nodeNr = tt.addNode(Saxon.Hej.type.Type.ELEMENT, currentDepth, -1, -1, nameCode);
            isIDElement = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.IS_ID);
            int typeCode = type.getFingerprint();
            if ((typeCode != Saxon.Hej.om.StandardNames.XS_UNTYPED)) {
                tt.setElementAnnotation(nodeNr, type);
                if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NILLED_ELEMENT)) {
                    tt.setNilled(nodeNr);
                }
                if ((!(isIDElement) && type.isIdType())) {
                    isIDElement = true;
                }
            }
            if ((currentDepth == 0)) {
                prevAtDepth[0] = nodeNr;
                prevAtDepth[1] = -1;
                currentRoot = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tt.getNode(nodeNr));
            } else {
                int prev = prevAtDepth[currentDepth];
                if ((prev>0)) {
                    tt.next[prev] = nodeNr;
                }
                tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
                prevAtDepth[currentDepth] = nodeNr;
                siblingsAtDepth[currentDepth]++;
            }
            currentDepth++;
            if ((currentDepth == prevAtDepth.Length)) {
                prevAtDepth = Saxon.Ejava.util.Arrays.copyOf(prevAtDepth, (currentDepth*2));
                siblingsAtDepth = Saxon.Ejava.util.Arrays.copyOf(siblingsAtDepth, (currentDepth*2));
            }
            prevAtDepth[currentDepth] = -1;
            siblingsAtDepth[currentDepth] = 0;
            string localSystemId = location.getSystemId();
            if ((isUseEventLocation() && (localSystemId != null))) {
                tt.setSystemId(nodeNr, localSystemId);
            } else {
                if ((currentDepth == 1)) {
                    tt.setSystemId(nodeNr, systemId);
                }
            }
            if (((uniformBaseURI && (localSystemId != null)) && !(localSystemId.Equals(baseURI)))) {
                uniformBaseURI = false;
                tt.setUniformBaseUri(null);
            }
            if (lineNumbering) {
                tt.setLineNumber(nodeNr, location.getLineNumber(), location.getColumnNumber());
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                attribute2(att.getNodeName(), att.getType(), getAttValue(att), location, att.getProperties());
            }
            textualElementEligibilityState = (noNewNamespaces ? Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.PRIMED : Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE);
            tree.addNamespaces(nodeNr, namespaceStack.Peek());
            nodeNr++;
        }
        protected internal virtual string getAttValue(Saxon.Hej.om.AttributeInfo att) {
            return att.getValue();
        }
        private void attribute2(Saxon.Hej.om.NodeName attName, Saxon.Hej.type.SimpleType type, string value, Saxon.Hej.s9api.Location locationId, int properties) {
            int fp = attName.obtainFingerprint(namePool);
            string prefix = attName.getPrefix();
            int nameCode = (prefix.Length==0 ? fp : (((tree.prefixPool.obtainPrefixCode(prefix)<<20))|fp));
            System.Diagnostics.Debug.Assert((tree != null));
            System.Diagnostics.Debug.Assert((currentRoot != null));
            tree.addAttribute(currentRoot, nodeNr, nameCode, type, value, properties);
            if ((markDefaultedAttributes && Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DEFAULTED_VALUE))) {
                tree.markDefaultedAttribute((tree.numberOfAttributes-1));
            }
            if ((fp == Saxon.Hej.om.StandardNames.XML_BASE)) {
                uniformBaseURI = false;
                tree.setUniformBaseUri(null);
            }
        }
        public override void endElement() {
            System.Diagnostics.Debug.Assert((tree != null));
            bool eligibleAsTextualElement = (textualElementEligibilityState == Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.ELIGIBLE);
            textualElementEligibilityState = Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE;
            prevAtDepth[currentDepth] = -1;
            siblingsAtDepth[currentDepth] = 0;
            currentDepth--;
            namespaceStack.Pop();
            if (isIDElement) {
                tree.indexIDElement(currentRoot, prevAtDepth[currentDepth]);
                isIDElement = false;
            } else {
                if (((((eligibleAsTextualElement && (tree.nodeKind[nodeNr] == Saxon.Hej.type.Type.TEXT)) && (tree.nodeKind[(nodeNr-1)] == Saxon.Hej.type.Type.ELEMENT)) && (tree.alpha[(nodeNr-1)] == -1)) && noNewNamespaces)) {
                    tree.nodeKind[(nodeNr-1)] = (byte)(Saxon.Hej.type.Type.TEXTUAL_ELEMENT);
                    tree.alpha[(nodeNr-1)] = tree.alpha[nodeNr];
                    tree.beta[(nodeNr-1)] = tree.beta[nodeNr];
                    nodeNr--;
                    tree.numberOfNodes--;
                    if ((currentDepth == 0)) {
                        currentRoot = ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(nodeNr));
                    }
                }
            }
        }
        public Saxon.Hej.tree.tiny.TinyNodeImpl getLastCompletedElement() {
            if ((tree == null)) {
                return null;
            }
            return (Saxon.Hej.tree.tiny.TinyNodeImpl)(((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)tree.getNode(((currentDepth>=0) ? prevAtDepth[currentDepth] : 0))));
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (((chars is Saxon.Hej.str.CompressedWhitespace) && Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.WHOLE_TEXT_NODE))) {
                Saxon.Hej.tree.tiny.TinyTree tt = tree;
                System.Diagnostics.Debug.Assert((tt != null));
                long lvalue = ((Saxon.Hej.str.CompressedWhitespace)(chars)).getCompressedValue();
                nodeNr = tt.addNode(Saxon.Hej.type.Type.WHITESPACE_TEXT, currentDepth, (int)(((lvalue>>32))), (int)(lvalue), -1);
                int prev = prevAtDepth[currentDepth];
                if ((prev>0)) {
                    tt.next[prev] = nodeNr;
                }
                tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
                prevAtDepth[currentDepth] = nodeNr;
                siblingsAtDepth[currentDepth]++;
                if (lineNumbering) {
                    tt.setLineNumber(nodeNr, locationId.getLineNumber(), locationId.getColumnNumber());
                }
                return ;
            }
            if (!(chars.isEmpty())) {
                nodeNr = makeTextNode(chars.tidy());
                if (lineNumbering) {
                    tree.setLineNumber(nodeNr, locationId.getLineNumber(), locationId.getColumnNumber());
                }
                textualElementEligibilityState = ((textualElementEligibilityState == Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.PRIMED) ? Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.ELIGIBLE : Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE);
            }
        }
        protected internal int makeTextNode(Saxon.Hej.str.UnicodeString chars) {
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            System.Diagnostics.Debug.Assert((tt != null));
            Saxon.Hej.str.LargeTextBuffer buffer = tt.textBuffer;
            int bufferStart = buffer.length();
            tt.appendChars(chars);
            int bufferEnd = buffer.length();
            int len = (bufferEnd-bufferStart);
            int n = (tt.numberOfNodes-1);
            if (((tt.nodeKind[n] == Saxon.Hej.type.Type.TEXT) && (tt.depth[n] == currentDepth))) {
                tt.beta[n] += len;
            } else {
                nodeNr = tt.addNode(Saxon.Hej.type.Type.TEXT, currentDepth, bufferStart, len, -1);
                int prev = prevAtDepth[currentDepth];
                if ((prev>0)) {
                    tt.next[prev] = nodeNr;
                }
                tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
                prevAtDepth[currentDepth] = nodeNr;
                siblingsAtDepth[currentDepth]++;
            }
            return nodeNr;
        }
        public override void processingInstruction(string piname, Saxon.Hej.str.UnicodeString remainder, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            System.Diagnostics.Debug.Assert((tt != null));
            textualElementEligibilityState = Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE;
            int s = (int)(commentBuilder.length());
            commentBuilder.accept(remainder);
            int nameCode = namePool.allocateFingerprint("", piname);
            nodeNr = tt.addNode(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION, currentDepth, s, remainder.tidy().length32(), nameCode);
            int prev = prevAtDepth[currentDepth];
            if ((prev>0)) {
                tt.next[prev] = nodeNr;
            }
            tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
            prevAtDepth[currentDepth] = nodeNr;
            siblingsAtDepth[currentDepth]++;
            string localLocation = locationId.getSystemId();
            tt.setSystemId(nodeNr, localLocation);
            if (((localLocation != null) && !(localLocation.Equals(baseURI)))) {
                uniformBaseURI = false;
                tree.setUniformBaseUri(null);
            }
            if (lineNumbering) {
                tt.setLineNumber(nodeNr, locationId.getLineNumber(), locationId.getColumnNumber());
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.tree.tiny.TinyTree tt = tree;
            System.Diagnostics.Debug.Assert((tt != null));
            textualElementEligibilityState = Saxon.Hej.tree.tiny.TinyBuilder.Eligibility.INELIGIBLE;
            int s = (int)(commentBuilder.length());
            commentBuilder.accept(chars);
            nodeNr = tt.addNode(Saxon.Hej.type.Type.COMMENT, currentDepth, s, chars.tidy().length32(), -1);
            int prev = prevAtDepth[currentDepth];
            if ((prev>0)) {
                tt.next[prev] = nodeNr;
            }
            tt.next[nodeNr] = prevAtDepth[(currentDepth-1)];
            prevAtDepth[currentDepth] = nodeNr;
            siblingsAtDepth[currentDepth]++;
            if (lineNumbering) {
                tt.setLineNumber(nodeNr, locationId.getLineNumber(), locationId.getColumnNumber());
            }
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            if ((tree.getUnparsedEntity(name) == null)) {
                tree.setUnparsedEntity(name, uri, publicId);
            }
        }
        public override Saxon.Hej.@event.BuilderMonitor getBuilderMonitor() {
            return new Saxon.Hej.tree.tiny.TinyBuilderMonitor(this);
        }
    
    }
}
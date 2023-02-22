using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class LinkedTreeBuilder : Saxon.Hej.@event.Builder {
        private Saxon.Hej.tree.linked.ParentNodeImpl currentNode;
        private Saxon.Hej.tree.linked.NodeFactory nodeFactory;
        private int[] size = new int[100];
        private int depth = 0;
        private System.Collections.Generic.List<Saxon.Hej.tree.linked.NodeImpl[]> arrays = new System.Collections.Generic.List<Saxon.Hej.tree.linked.NodeImpl[]>(20);
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> namespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        private bool allocateSequenceNumbers = true;
        private int nextNodeNumber = 1;
        public LinkedTreeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe) : base (pipe) {
            nodeFactory = Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory.THE_INSTANCE;
        }
        public LinkedTreeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.om.Durability durability) : base (pipe) {
            this.durability = durability;
            nodeFactory = Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory.THE_INSTANCE;
        }
        public override void setDurability(Saxon.Hej.om.Durability durability) {
            if ((this.durability != Saxon.Hej.om.Durability.MUTABLE)) {
                this.durability = durability;
            }
        }
        public override Saxon.Hej.om.NodeInfo getCurrentRoot() {
            Saxon.Hej.om.NodeInfo physicalRoot = currentRoot;
            if (((physicalRoot is Saxon.Hej.tree.linked.DocumentImpl) && ((Saxon.Hej.tree.linked.DocumentImpl)(physicalRoot)).isImaginary())) {
                return ((Saxon.Hej.tree.linked.DocumentImpl)(physicalRoot)).getDocumentElement();
            } else {
                return physicalRoot;
            }
        }
        public override void reset() {
            base.reset();
            currentNode = null;
            nodeFactory = Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory.THE_INSTANCE;
            depth = 0;
            allocateSequenceNumbers = true;
            nextNodeNumber = 1;
        }
        public void setAllocateSequenceNumbers(bool allocate) {
            allocateSequenceNumbers = allocate;
        }
        public void setNodeFactory(Saxon.Hej.tree.linked.NodeFactory factory) {
            nodeFactory = factory;
        }
        public override void open() {
            started = true;
            depth = 0;
            size[depth] = 0;
            if ((arrays == null)) {
                arrays = new System.Collections.Generic.List<Saxon.Hej.tree.linked.NodeImpl[]>(20);
            }
            if (useEventLocation) {
                System.Object copier = getPipelineConfiguration().getComponent(typeof(Saxon.Hej.@event.CopyInformee).Name);
                if ((copier is Saxon.Hej.@event.LocationCopier)) {
                    setSystemId(((Saxon.Hej.@event.LocationCopier)(copier)).getSystemId());
                }
            }
            base.open();
        }
        public override void startDocument(int properties) {
            Saxon.Hej.tree.linked.DocumentImpl doc = new Saxon.Hej.tree.linked.DocumentImpl();
            doc.setMutable((durability == Saxon.Hej.om.Durability.MUTABLE));
            currentRoot = doc;
            doc.setSystemId(getSystemId());
            doc.setBaseURI(getBaseURI());
            doc.setConfiguration(config);
            currentNode = doc;
            depth = 0;
            size[depth] = 0;
            if ((arrays == null)) {
                arrays = new System.Collections.Generic.List<Saxon.Hej.tree.linked.NodeImpl[]>(20);
            }
            doc.setRawSequenceNumber(0);
            if (lineNumbering) {
                doc.setLineNumbering();
            }
        }
        public override void endDocument() {
            currentNode.compact(size[depth]);
        }
        public override void close() {
            if ((currentNode == null)) {
                return ;
            }
            currentNode.compact(size[depth]);
            currentNode = null;
            arrays = null;
            base.close();
            nodeFactory = Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory.THE_INSTANCE;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap suppliedAttributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((currentNode == null)) {
                startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                ((Saxon.Hej.tree.linked.DocumentImpl)(currentRoot)).setImaginary(true);
            }
            bool isNilled = Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.NILLED_ELEMENT);
            namespaceStack.Push(namespaces);
            bool isTopWithinEntity = false;
            Saxon.Hej.om.AttributeInfo xmlId = suppliedAttributes.get(Saxon.Hej.om.NamespaceUri.XML, "id");
            if (((xmlId != null) && Saxon.Hej.value.Whitespace.containsWhitespace(Saxon.Hej.str.StringTool.codePoints(xmlId.getValue())))) {
                suppliedAttributes = suppliedAttributes.put(new Saxon.Hej.om.AttributeInfo(xmlId.getNodeName(), xmlId.getType(), Saxon.Hej.value.Whitespace.trim(xmlId.getValue()), xmlId.getLocation(), xmlId.getProperties()));
            }
            if ((location.getSystemId() == null)) {
                location = new Saxon.Hej.expr.parser.Loc(getSystemId(), location.getLineNumber(), location.getColumnNumber());
            }
            Saxon.Hej.tree.linked.ElementImpl elem = nodeFactory.makeElementNode(currentNode, elemName, type, isNilled, suppliedAttributes, namespaceStack.Peek(), pipe, location, (allocateSequenceNumbers ? nextNodeNumber++ : -1));
            while ((depth>=arrays.Count)) {
                arrays.Add(new Saxon.Hej.tree.linked.NodeImpl[20]);
            }
            elem.setChildren(arrays[depth]);
            currentNode.addChild(elem, size[depth]++);
            if ((depth>=(size.Length-1))) {
                size = Saxon.Ejava.util.Arrays.copyOf(size, (size.Length*2));
            }
            size[++depth] = 0;
            if ((currentNode is Saxon.Hej.om.TreeInfo)) {
                ((Saxon.Hej.tree.linked.DocumentImpl)(currentNode)).setDocumentElement(elem);
            }
            if (isTopWithinEntity) {
                currentNode.getPhysicalRoot().markTopWithinEntity(elem);
            }
            currentNode = elem;
        }
        public override void endElement() {
            currentNode.compact(size[depth]);
            depth--;
            currentNode = (Saxon.Hej.tree.linked.ParentNodeImpl)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)currentNode.getParent()));
            namespaceStack.Pop();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(chars.isEmpty())) {
                Saxon.Hej.str.UnicodeString t = chars.tidy();
                Saxon.Hej.om.NodeInfo prev = currentNode.getNthChild((size[depth]-1));
                if ((prev is Saxon.Hej.tree.linked.TextImpl)) {
                    ((Saxon.Hej.tree.linked.TextImpl)(prev)).appendStringValue(t);
                } else {
                    Saxon.Hej.tree.linked.TextImpl n = nodeFactory.makeTextNode(currentNode, t);
                    currentNode.addChild(n, size[depth]++);
                }
            }
        }
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString remainder, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.tree.linked.ProcInstImpl pi = new Saxon.Hej.tree.linked.ProcInstImpl(name, remainder.tidy());
            currentNode.addChild(pi, size[depth]++);
            pi.setLocation(locationId.getSystemId(), locationId.getLineNumber(), locationId.getColumnNumber());
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            Saxon.Hej.tree.linked.CommentImpl comment = new Saxon.Hej.tree.linked.CommentImpl(chars.tidy());
            currentNode.addChild(comment, size[depth]++);
            comment.setLocation(locationId.getSystemId(), locationId.getLineNumber(), locationId.getColumnNumber());
        }
        public Saxon.Hej.tree.linked.ParentNodeImpl getCurrentParentNode() {
            return currentNode;
        }
        public Saxon.Hej.tree.linked.NodeImpl getCurrentLeafNode() {
            return currentNode.getLastChild();
        }
        public void graftElement(Saxon.Hej.tree.linked.ElementImpl element) {
            currentNode.addChild(element, size[depth]++);
        }
        public override void setUnparsedEntity(string name, string uri, string publicId) {
            if ((((Saxon.Hej.tree.linked.DocumentImpl)(currentRoot)).getUnparsedEntity(name) == null)) {
                ((Saxon.Hej.tree.linked.DocumentImpl)(currentRoot)).setUnparsedEntity(name, uri, publicId);
            }
        }
        public override Saxon.Hej.@event.BuilderMonitor getBuilderMonitor() {
            return new Saxon.Hej.tree.linked.LinkedBuilderMonitor(this);
        }
        private class DefaultNodeFactory : Saxon.Hej.tree.linked.NodeFactory {
            public static Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory THE_INSTANCE = new Saxon.Hej.tree.linked.LinkedTreeBuilder.DefaultNodeFactory();
            public Saxon.Hej.tree.linked.ElementImpl makeElementNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NodeName nodeName, Saxon.Hej.type.SchemaType elementType, bool isNilled, Saxon.Hej.om.AttributeMap attlist, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.s9api.Location locationId, int sequenceNumber) {
                Saxon.Hej.tree.linked.ElementImpl e = new Saxon.Hej.tree.linked.ElementImpl();
                e.setNamespaceMap(namespaces);
                e.initialise(nodeName, elementType, attlist, parent, sequenceNumber);
                if (isNilled) {
                    e.setNilled();
                }
                if (((locationId != Saxon.Hej.expr.parser.Loc.NONE) && (sequenceNumber>=0))) {
                    string baseURI = locationId.getSystemId();
                    int lineNumber = locationId.getLineNumber();
                    int columnNumber = locationId.getColumnNumber();
                    e.setLocation(baseURI, lineNumber, columnNumber);
                }
                return e;
            }
            public Saxon.Hej.tree.linked.TextImpl makeTextNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.str.UnicodeString content) {
                return new Saxon.Hej.tree.linked.TextImpl(content);
            }
        
        }
    
    }
}
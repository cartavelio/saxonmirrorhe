using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class VirtualCopy : Saxon.Hej.om.NodeInfo {
        protected internal Saxon.Ejava.util.function.Supplier<string> systemIdSupplier;
        protected internal Saxon.Hej.om.NodeInfo original;
        protected internal Saxon.Hej.tree.wrapper.VirtualCopy parent;
        protected internal Saxon.Hej.om.NodeInfo root;
        protected internal Saxon.Hej.tree.wrapper.VirtualTreeInfo tree;
        private bool dropNamespaces = false;
        protected internal VirtualCopy(Saxon.Hej.om.NodeInfo @base, Saxon.Hej.om.NodeInfo root)  {
            original = @base;
            systemIdSupplier = () => @base.getBaseURI();
            this.root = root;
        }
        public static Saxon.Hej.tree.wrapper.VirtualCopy makeVirtualCopy(Saxon.Hej.om.NodeInfo original) {
            Saxon.Hej.tree.wrapper.VirtualCopy vc;
            while ((original is Saxon.Hej.tree.wrapper.VirtualCopy)) {
                original = ((Saxon.Hej.tree.wrapper.VirtualCopy)(original)).original;
            }
            vc = new Saxon.Hej.tree.wrapper.VirtualCopy(original, original);
            Saxon.Hej.Configuration config = original.getConfiguration();
            Saxon.Hej.tree.wrapper.VirtualTreeInfo doc = new Saxon.Hej.tree.wrapper.VirtualTreeInfo(config, vc);
            long docNr = config.getDocumentNumberAllocator().allocateDocumentNumber();
            doc.setDocumentNumber(docNr);
            vc.tree = doc;
            return vc;
        }
        protected internal virtual Saxon.Hej.tree.wrapper.VirtualCopy wrap(Saxon.Hej.om.NodeInfo node) {
            Saxon.Hej.tree.wrapper.VirtualCopy vc = new Saxon.Hej.tree.wrapper.VirtualCopy(node, root);
            vc.tree = tree;
            vc.systemIdSupplier = systemIdSupplier;
            vc.dropNamespaces = dropNamespaces;
            return vc;
        }
        public Saxon.Hej.om.NodeInfo getOriginalNode() {
            return original;
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return tree;
        }
        public void setDropNamespaces(bool drop) {
            this.dropNamespaces = drop;
        }
        public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                if (dropNamespaces) {
                    Saxon.Hej.om.NamespaceMap nsMap = Saxon.Hej.om.NamespaceMap.emptyMap();
                    string ns = getURI();
                    if (!(ns.Length==0)) {
                        nsMap = nsMap.put(getPrefix(), ns);
                    }
                    Saxon.Hej.tree.iter.AxisIterator iter = original.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                    Saxon.Hej.om.NodeInfo att;
                    while (((att = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                        if (!(att.getURI().Equals(""))) {
                            nsMap = nsMap.put(att.getPrefix(), att.getURI());
                        }
                    }
                    return nsMap;
                } else {
                    return original.getAllNamespaces();
                }
            } else {
                return null;
            }
        }
        public int getFingerprint() {
            return original.getFingerprint();
        }
        public bool hasFingerprint() {
            return original.hasFingerprint();
        }
        public int getNodeKind() {
            return original.getNodeKind();
        }
        public override bool Equals(System.Object other) {
            return (((other is Saxon.Hej.tree.wrapper.VirtualCopy) && (((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)getTreeInfo()) == ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)((Saxon.Hej.tree.wrapper.VirtualCopy)(other)).getTreeInfo()))) && original.Equals(((Saxon.Hej.tree.wrapper.VirtualCopy)(other)).original));
        }
        public override int GetHashCode() {
            return (original.GetHashCode()^(((int)(((((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)getTreeInfo()).getDocumentNumber()&0x7fffffff)))<<19)));
        }
        public string getSystemId() {
            return systemIdSupplier();
        }
        public string getBaseURI() {
            return Saxon.Hej.tree.util.Navigator.getBaseURI(this);
        }
        public int getLineNumber() {
            return original.getLineNumber();
        }
        public int getColumnNumber() {
            return original.getColumnNumber();
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if ((other is Saxon.Hej.tree.wrapper.VirtualCopy)) {
                int c = root.compareOrder(((Saxon.Hej.tree.wrapper.VirtualCopy)(other)).root);
                if ((c == 0)) {
                    return original.compareOrder(((Saxon.Hej.tree.wrapper.VirtualCopy)(other)).original);
                } else {
                    return c;
                }
            } else {
                return other.compareOrder(original);
            }
        }
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return original.getUnicodeStringValue();
        }
        public string getLocalPart() {
            return original.getLocalPart();
        }
        public string getURI() {
            return original.getURI();
        }
        public string getPrefix() {
            return original.getPrefix();
        }
        public string getDisplayName() {
            return original.getDisplayName();
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return original.getConfiguration();
        }
        public virtual Saxon.Hej.type.SchemaType getSchemaType() {
            return original.getSchemaType();
        }
        public virtual Saxon.Hej.om.NodeInfo getParent() {
            if (original.Equals(root)) {
                return null;
            }
            if ((parent == null)) {
                Saxon.Hej.om.NodeInfo basep = original.getParent();
                if ((basep == null)) {
                    return null;
                }
                parent = wrap(basep);
            }
            return parent;
        }
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            Saxon.Hej.tree.wrapper.VirtualCopy newParent = null;
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
                newParent = this;
                break ;
              case Saxon.Hej.om.AxisInfo.SELF:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
                newParent = parent;
                break ;
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(this, false), nodeTest);
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(this, true), nodeTest);
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
                if ((getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
                return Saxon.Hej.tree.NamespaceNode.makeIterator(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.PARENT:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(getParent(), nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, false), nodeTest);
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.FollowingEnumeration(this), nodeTest);
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return new Saxon.Hej.tree.util.Navigator.AxisFilter(new Saxon.Hej.tree.util.Navigator.PrecedingEnumeration(this, true), nodeTest);
            }
            return makeCopier(original.iterateAxis(axisNumber, nodeTest), newParent, !(Saxon.Hej.om.AxisInfo.isSubtreeAxis[axisNumber]));
        }
        public string getAttributeValue(string uri, string local) {
            return original.getAttributeValue(uri, local);
        }
        public virtual Saxon.Hej.om.NodeInfo getRoot() {
            Saxon.Hej.om.NodeInfo n = this;
            while (true) {
                Saxon.Hej.om.NodeInfo p = n.getParent();
                if ((p == null)) {
                    return n;
                }
                n = p;
            }
        }
        public bool hasChildNodes() {
            return original.hasChildNodes();
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append("d");
            buffer.append(((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)getTreeInfo()).getDocumentNumber());
            original.generateId(buffer);
        }
        public virtual void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            if (dropNamespaces) {
                copyOptions &= ~(Saxon.Hej.om.CopyOptions.ALL_NAMESPACES);
            }
            original.copy(@out, copyOptions, locationId);
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                if (dropNamespaces) {
                    System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> allNamespaces = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>(5);
                    string ns = getURI();
                    if (ns.Length==0) {
                        if (((getParent() != null) && !(getParent().getURI().Length==0))) {
                            allNamespaces.Add(new Saxon.Hej.om.NamespaceBinding("", ""));
                        }
                    } else {
                        allNamespaces.Add(new Saxon.Hej.om.NamespaceBinding(getPrefix(), getURI()));
                    }
                    foreach (Saxon.Hej.om.AttributeInfo att in original.attributes()) {
                        Saxon.Hej.om.NodeName name = att.getNodeName();
                        if ((name.getURI() != null)) {
                            Saxon.Hej.om.NamespaceBinding b = new Saxon.Hej.om.NamespaceBinding(name.getPrefix(), name.getURI());
                            if (!(allNamespaces.Contains(b))) {
                                allNamespaces.Add(b);
                            }
                        }
                    }
                    return allNamespaces.ToArray();
                } else {
                    if ((original == root)) {
                        System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> bindings = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
                        foreach (Saxon.Hej.om.NamespaceBinding binding in original.getAllNamespaces()) {
                            bindings.Add(binding);
                        }
                        return bindings.ToArray();
                    } else {
                        return original.getDeclaredNamespaces(buffer);
                    }
                }
            } else {
                return null;
            }
        }
        public void setSystemId(string systemId) {
            this.systemIdSupplier = () => systemId;
        }
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            return original.atomize();
        }
        public virtual bool isId() {
            return original.isId();
        }
        public virtual bool isIdref() {
            return original.isIdref();
        }
        public virtual bool isNilled() {
            return original.isNilled();
        }
        public virtual string getPublicId() {
            return ((original != null) ? original.getPublicId() : null);
        }
        protected internal virtual bool isIncludedInCopy(Saxon.Hej.om.NodeInfo sourceNode) {
            return Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(root, sourceNode);
        }
        protected internal Saxon.Hej.tree.wrapper.VirtualCopy.VirtualCopier makeCopier(Saxon.Hej.tree.iter.AxisIterator axis, Saxon.Hej.tree.wrapper.VirtualCopy newParent, bool testInclusion) {
            return new Saxon.Hej.tree.wrapper.VirtualCopy.VirtualCopier(this, axis, newParent, testInclusion);
        }
        protected internal class VirtualCopier : Saxon.Hej.tree.iter.AxisIterator {
            protected internal Saxon.Hej.tree.wrapper.VirtualCopy node;
            protected internal Saxon.Hej.tree.iter.AxisIterator @base;
            private readonly Saxon.Hej.tree.wrapper.VirtualCopy parent;
            protected internal bool testInclusion;
            public VirtualCopier(Saxon.Hej.tree.wrapper.VirtualCopy node, Saxon.Hej.tree.iter.AxisIterator @base, Saxon.Hej.tree.wrapper.VirtualCopy parent, bool testInclusion)  {
                this.node = node;
                this.@base = @base;
                this.parent = parent;
                this.testInclusion = testInclusion;
            }
            public Saxon.Hej.om.Item next() {
                Saxon.Hej.om.NodeInfo next = ((/*1430*/Saxon.Hej.om.NodeInfo)@base.next());
                if ((next != null)) {
                    if ((testInclusion && !(node.isIncludedInCopy(next)))) {
                        return null;
                    }
                    Saxon.Hej.tree.wrapper.VirtualCopy vc = node.wrap(next);
                    vc.parent = parent;
                    vc.systemIdSupplier = node.systemIdSupplier;
                    next = vc;
                }
                return next;
            }
            public void close() {
                @base.close();
            }
        
        }
    
        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public virtual bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }

        /* Expanding default method reduce from interface Item */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface NodeInfo */
        public virtual string toShortString() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document-node()";
              case Saxon.Hej.type.Type.ELEMENT:
                return (("<"+getDisplayName())+"/>");
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ("@"+getDisplayName());
              case Saxon.Hej.type.Type.TEXT:
                return (("text(\""+Saxon.Hej.trans.Err.truncate30(getUnicodeStringValue()))+"\")");
              case Saxon.Hej.type.Type.COMMENT:
                return (("<!--"+Saxon.Hej.trans.Err.truncate30(getUnicodeStringValue()))+"-->");
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return (("<?"+getDisplayName())+"?>");
              case Saxon.Hej.type.Type.NAMESPACE:
                string prefix = getLocalPart();
                return (((("xmlns"+((prefix.Equals("") ? "" : (":"+prefix))))+"=\"")+getUnicodeStringValue())+'"');
              default:
                return "";
            }
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method head from interface Item */
        public virtual Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public virtual Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public virtual int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface NodeInfo */
        public virtual bool isStreamed() {
            return false;
        }

        /* Expanding default method isSameNodeInfo from interface NodeInfo */
        public virtual bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return Equals(other);
        }

        /* Expanding default method iterateAxis from interface NodeInfo */
        public virtual Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            return iterateAxis(axisNumber, Saxon.Hej.pattern.AnyNodeTest.getInstance());
        }

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children() {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this);
        }

        /* Expanding default method children from interface NodeInfo */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter) {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, filter);
        }

        /* Expanding default method attributes from interface NodeInfo */
        public virtual Saxon.Hej.om.AttributeMap attributes() {
            Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
            if ((getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                Saxon.Hej.tree.iter.AxisIterator iter = iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                Saxon.Hej.om.NodeInfo attr;
                while (((attr = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                    atts = atts.put(new Saxon.Hej.om.AttributeInfo(Saxon.Hej.om.NameOfNode.makeName(attr), (Saxon.Hej.type.SimpleType)(attr.getSchemaType()), attr.getStringValue(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE));
                }
            }
            return atts;
        }

        /* Expanding default method asActiveSource from interface NodeInfo */
        public virtual Saxon.Hej.lib.ActiveSource asActiveSource() {
            return new Saxon.Hej.om.NodeSource(this);
        }

        /* Expanding default method getGenre from interface NodeInfo */
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}
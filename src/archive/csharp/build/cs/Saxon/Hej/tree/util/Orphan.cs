using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public sealed class Orphan : Saxon.Hej.om.MutableNodeInfo {
        private short kind;
        private Saxon.Hej.om.NodeName nodeName = null;
        private Saxon.Hej.str.UnicodeString stringValue;
        private Saxon.Hej.type.SchemaType typeAnnotation = null;
        private int options = Saxon.Hej.@event.ReceiverOption.NONE;
        private readonly Saxon.Hej.om.GenericTreeInfo treeInfo;
        public Orphan(Saxon.Hej.Configuration config)  {
            treeInfo = new Saxon.Hej.om.GenericTreeInfo(config);
            treeInfo.setRootNode(this);
        }
        public Saxon.Hej.om.TreeInfo getTreeInfo() {
            return treeInfo;
        }
        public string getSystemId() {
            return treeInfo.getSystemId();
        }
        public string getPublicId() {
            return treeInfo.getPublicId();
        }
        public void setSystemId(string systemId) {
            treeInfo.setSystemId(systemId);
        }
        public bool effectiveBooleanValue() {
            return true;
        }
        public void setNodeKind(short kind) {
            this.kind = kind;
        }
        public void setNodeName(Saxon.Hej.om.NodeName nodeName) {
            this.nodeName = nodeName;
        }
        public void setStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            this.stringValue = stringValue;
        }
        public void setTypeAnnotation(Saxon.Hej.type.SchemaType typeAnnotation) {
            this.typeAnnotation = typeAnnotation;
        }
        public void setIsId(bool id) {
            setOption(Saxon.Hej.@event.ReceiverOption.IS_ID, id);
        }
        private void setOption(int option, bool on) {
            if (on) {
                options |= option;
            } else {
                options &= ~(option);
            }
        }
        private bool isOption(int option) {
            return Saxon.Hej.@event.ReceiverOption.contains(options, option);
        }
        public void setIsIdref(bool idref) {
            setOption(Saxon.Hej.@event.ReceiverOption.IS_IDREF, idref);
        }
        public void setDisableOutputEscaping(bool doe) {
            setOption(Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING, doe);
        }
        public int getNodeKind() {
            return kind;
        }
        public int getFingerprint() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public bool hasFingerprint() {
            return false;
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.COMMENT:
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return new Saxon.Hej.value.StringValue(stringValue);
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.DOCUMENT:
              case Saxon.Hej.type.Type.NAMESPACE:
                return new Saxon.Hej.value.UntypedAtomicValue(getUnicodeStringValue());
              default:
                if ((((typeAnnotation == null) || (typeAnnotation == Saxon.Hej.type.Untyped.getInstance())) || (typeAnnotation == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                    return new Saxon.Hej.value.UntypedAtomicValue(getUnicodeStringValue());
                } else {
                    return typeAnnotation.atomize(this);
                }
            }
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            if ((typeAnnotation == null)) {
                if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                    return Saxon.Hej.type.Untyped.getInstance();
                } else {
                    if ((kind == Saxon.Hej.type.Type.ATTRIBUTE)) {
                        return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                    }
                }
            }
            return typeAnnotation;
        }
        public bool Equals(Saxon.Hej.om.NodeInfo other) {
            return (this == other);
        }
        public override int GetHashCode() {
            return base.GetHashCode();
        }
        public string getBaseURI() {
            if ((kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                return getSystemId();
            } else {
                return null;
            }
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public int compareOrder(Saxon.Hej.om.NodeInfo other) {
            if (this.Equals(other)) {
                return 0;
            }
            return ((this.GetHashCode()<other.GetHashCode()) ? -1 : +(1));
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return stringValue;
        }
        public string getLocalPart() {
            if ((nodeName == null)) {
                return "";
            } else {
                return nodeName.getLocalPart();
            }
        }
        public string getURI() {
            if ((nodeName == null)) {
                return "";
            } else {
                return nodeName.getURI();
            }
        }
        public string getPrefix() {
            if ((nodeName == null)) {
                return "";
            } else {
                return nodeName.getPrefix();
            }
        }
        public string getDisplayName() {
            if ((nodeName == null)) {
                return "";
            } else {
                return nodeName.getDisplayName();
            }
        }
        public Saxon.Hej.om.NodeInfo getParent() {
            return null;
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(this);
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
              case Saxon.Hej.om.AxisInfo.PARENT:
              case Saxon.Hej.om.AxisInfo.PRECEDING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate nodeTest) {
            switch (axisNumber) {
              case Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF:
              case Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF:
              case Saxon.Hej.om.AxisInfo.SELF:
                return Saxon.Hej.tree.util.Navigator.filteredSingleton(this, nodeTest);
              case Saxon.Hej.om.AxisInfo.ANCESTOR:
              case Saxon.Hej.om.AxisInfo.ATTRIBUTE:
              case Saxon.Hej.om.AxisInfo.CHILD:
              case Saxon.Hej.om.AxisInfo.DESCENDANT:
              case Saxon.Hej.om.AxisInfo.FOLLOWING:
              case Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING:
              case Saxon.Hej.om.AxisInfo.NAMESPACE:
              case Saxon.Hej.om.AxisInfo.PARENT:
              case Saxon.Hej.om.AxisInfo.PRECEDING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING:
              case Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR:
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown axis number "+axisNumber));
            }
        }
        public string getAttributeValue(string uri, string local) {
            return null;
        }
        public Saxon.Hej.om.NodeInfo getRoot() {
            return this;
        }
        public bool hasChildNodes() {
            return false;
        }
        public void generateId(Saxon.Ejava.lang.StringBuilder buffer) {
            buffer.append('Q');
            buffer.append(GetHashCode());
        }
        public Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer) {
            return null;
        }
        public Saxon.Hej.om.NamespaceMap getAllNamespaces() {
            return null;
        }
        public bool isId() {
            return (isOption(Saxon.Hej.@event.ReceiverOption.IS_ID) || (((kind == Saxon.Hej.type.Type.ATTRIBUTE) && nodeName.Equals(Saxon.Hej.om.StandardNames.XML_ID_NAME))));
        }
        public bool isIdref() {
            return isOption(Saxon.Hej.@event.ReceiverOption.IS_IDREF);
        }
        public bool isDisableOutputEscaping() {
            return isOption(Saxon.Hej.@event.ReceiverOption.DISABLE_ESCAPING);
        }
        public void insertChildren(Saxon.Hej.om.NodeInfo[] source, bool atStart, bool inherit) {
        }
        public void insertSiblings(Saxon.Hej.om.NodeInfo[] source, bool before, bool inherit) {
        }
        public void setAttributes(Saxon.Hej.om.AttributeMap attributes) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public void removeAttribute(Saxon.Hej.om.NodeInfo attribute) {
        }
        public void addAttribute(Saxon.Hej.om.NodeName nameCode, Saxon.Hej.type.SimpleType attType, string value, int properties, bool inheritNamespaces) {
        }
        public void delete() {
            kind = -1;
        }
        public bool isDeleted() {
            return (kind == -1);
        }
        public void replace(Saxon.Hej.om.NodeInfo[] replacement, bool inherit) {
            throw new Saxon.Ejava.lang.IllegalStateException("Cannot replace a parentless node");
        }
        public void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue) {
            this.stringValue = stringValue;
        }
        public void rename(Saxon.Hej.om.NodeName newNameCode, bool inherit) {
            if (((kind == Saxon.Hej.type.Type.ATTRIBUTE) || (kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION))) {
                nodeName = newNameCode;
            }
        }
        public void addNamespace(Saxon.Hej.om.NamespaceBinding nscode, bool inherit) {
        }
        public void removeTypeAnnotation() {
            typeAnnotation = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        public Saxon.Hej.@event.Builder newBuilder() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Cannot create children for an Orphan node");
        }
    
        /* Expanding default method reduce from interface Item */
        public Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface NodeInfo */
        public string toShortString() {
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
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
        }

        /* Expanding default method head from interface Item */
        public Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface NodeInfo */
        public bool isStreamed() {
            return false;
        }

        /* Expanding default method removeNamespace from interface MutableNodeInfo */
        public void removeNamespace(string prefix) {
        }

        /* Expanding default method addNamespace from interface MutableNodeInfo */
        public void addNamespace(string prefix, string uri) {
        }

        /* Expanding default method getConfiguration from interface NodeInfo */
        public Saxon.Hej.Configuration getConfiguration() {
            return getTreeInfo().getConfiguration();
        }

        /* Expanding default method isSameNodeInfo from interface NodeInfo */
        public bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other) {
            return Equals(other);
        }

        /* Expanding default method getLineNumber from interface NodeInfo */
        public int getLineNumber() {
            return -1;
        }

        /* Expanding default method getColumnNumber from interface NodeInfo */
        public int getColumnNumber() {
            return -1;
        }

        /* Expanding default method children from interface NodeInfo */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children() {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this);
        }

        /* Expanding default method children from interface NodeInfo */
        public System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter) {
            return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, filter);
        }

        /* Expanding default method attributes from interface NodeInfo */
        public Saxon.Hej.om.AttributeMap attributes() {
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

        /* Expanding default method copy from interface NodeInfo */
        public void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.tree.util.Navigator.copy(this, @out, copyOptions, locationId);
        }

        /* Expanding default method asActiveSource from interface NodeInfo */
        public Saxon.Hej.lib.ActiveSource asActiveSource() {
            return new Saxon.Hej.om.NodeSource(this);
        }

        /* Expanding default method isNilled from interface NodeInfo */
        public bool isNilled() {
            return false;
        }

        /* Expanding default method getGenre from interface NodeInfo */
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}
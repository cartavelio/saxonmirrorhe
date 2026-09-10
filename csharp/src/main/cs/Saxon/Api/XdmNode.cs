using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Saxon.Ejava.util;
using Saxon.Hej;
using Saxon.Hej.@event;
using Saxon.Hej.expr.parser;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.tree.wrapper;
using Saxon.Api.Support;
using Saxon.Hej.type;
using Saxon.Impl.Helpers;
using Type = Saxon.Hej.type.Type;

namespace Saxon.Api {
    /// <summary inherits="XdmItem">
    /// The class <c>XdmNode</c> represents a Node in the XDM Data Model. A Node
    /// is an <c>XdmItem</c>, and is therefore an <c>XdmValue</c> in its own right, and may also participate
    /// as one item within a sequence value.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XdmNode</c> is implemented as a wrapper around an object
    /// of type <c>Saxon.Hej.NodeInfo</c>. Because this is a key interface
    /// within Saxon, it is exposed via this API, even though it is an internal
    /// interface that is not part of the API proper.</para>
    /// <para>The <c>XdmNode</c> interface exposes basic properties of the node, such
    /// as its name, its string value, and its typed value. Navigation to other nodes
    /// is supported through a single method, <c>EnumerateAxis</c>, which allows
    /// other nodes to be retrieved by following any of the XPath axes.</para>
    /// <para>Navigation from a node to other related nodes can be achieved using the
    /// <see cref="XdmValue.Select"/> method, which this class inherits.</para>
    /// </remarks>
    [Serializable]
    public class XdmNode : XdmItem {

        /// <summary>
        /// Construct an XdmNode as a wrapper around an instance of the internal class <c>Saxon.Hej.om.NodeInfo</c>
        /// </summary>
        /// <remarks>
        /// Applications using the class <c>Saxon.Hej.om.NodeInfo</c> should be aware that it is subject to occasional
        /// changes from one release to the next. For documentation on this class, see the Saxon-EE Javadoc documentation
        /// for the very similar class <see cref="Saxon.Hej.om.NodeInfo"/>.
        /// </remarks>
        /// <param name="node">The object to be wrapped</param>
		public XdmNode(Saxon.Hej.om.NodeInfo node) : base(node) {
            
        }
        
        /// <summary>
        /// Determine whether the item is an atomic value
        /// </summary>
        /// <returns>
        /// false (the item is not an atomic value)
        /// </returns>
        public override bool IsAtomic() {
            return false;
        }

        /// <summary>
        /// The name of the node, as a <c>QName</c>. Returns null in the case of unnamed nodes.
        /// </summary>

        public QName NodeName {
            get {
                NodeInfo node = (NodeInfo) value;
                string local = node.getLocalPart();
                if (local == "") {
                    return null;
                }

                string prefix = node.getPrefix();
                string uri = node.getURI();
                return new QName(prefix, uri, local);
            }
        }
        
        /// <summary>
        /// The kind of node, as an instance of <c>Saxon.Api.XdmNodeKind</c>.
        /// </summary>

        public XdmNodeKind NodeKind {
            get {
                NodeInfo node = (NodeInfo) value;
                int kind = node.getNodeKind();
                switch (kind) {
                    case Type.DOCUMENT:
                        return XdmNodeKind.Document;
                    case Type.ELEMENT:
                        return XdmNodeKind.Element;
                    case Type.ATTRIBUTE:
                        return XdmNodeKind.Attribute;
                    case Type.TEXT:
                        return XdmNodeKind.Text;
                    case Type.COMMENT:
                        return XdmNodeKind.Comment;
                    case Type.PROCESSING_INSTRUCTION:
                        return XdmNodeKind.ProcessingInstruction;
                    case Type.NAMESPACE:
                        return XdmNodeKind.Namespace;
                    default:
                        throw new ArgumentException("Unknown node kind");
                }
            }
        }
        
        /// <summary>
        /// The kind of node, as an instance of <c>System.Xml.XmlNodeType</c>.
        /// </summary>
        /// <remarks>For a namespace node in the XDM model, the value <c>XmlNodeType.None</c> 
        /// is returned.
        /// </remarks>

        public XmlNodeType NodeType {
            get {
                NodeInfo node = (NodeInfo) value;
                int kind = node.getNodeKind();
                switch (kind) {
                    case Type.DOCUMENT:
                        return XmlNodeType.Document;
                    case Type.ELEMENT:
                        return XmlNodeType.Element;
                    case Type.ATTRIBUTE:
                        return XmlNodeType.Attribute;
                    case Type.TEXT:
                        return XmlNodeType.Text;
                    case Type.COMMENT:
                        return XmlNodeType.Comment;
                    case Type.PROCESSING_INSTRUCTION:
                        return XmlNodeType.ProcessingInstruction;
                    case Type.NAMESPACE:
                        return XmlNodeType.None;
                    default:
                        throw new ArgumentException("Unknown node kind");
                }
            }
        }

        /// <summary>
        /// Get the line number of the node in a source document. 
        /// </summary>
        /// <remarks>
        /// For a document constructed using the document
        /// builder, this is available only if the line numbering option was set when the document was built (and
        /// then only for element nodes). If the line number is not available, the value -1 is returned.
        /// Line numbers will be as reported by the XML parser; the Microsoft XML parser reports the line number
        /// of the opening "&lt;" of the start tag.
        /// </remarks>

        public int LineNumber => ((NodeInfo) value).getLineNumber();

        /// <summary>
        /// Get the column number of the node in a source document. 
        /// </summary>
        /// <remarks>
        /// For a document constructed using the document
        /// builder, this is available only if the line numbering option was set when the document was built (and
        /// then only for element nodes). If the column number is not available, the value -1 is returned.
        /// Column numbers will be as reported by the XML parser; the Microsoft XML parser reports the column number
        /// of the opening "&lt;" of the start tag.
        /// </remarks>

        public int ColumnNumber => ((NodeInfo) value).getColumnNumber();

        /// <summary>
        /// The name of the type annotation of the node. 
        /// </summary>
        /// <remarks>
        /// <para>For elements and attributes, this
        /// will be the name of the schema type against which the node was validated; if
        /// the node has not been validated it will be <c>xs:untyped</c> for elements,
        /// and <c>xs:untypedAtomic</c> for attributes.
        /// </para>
        /// <para>For a document node, the value will be <c>xs:anyType</c> if the document
        /// has been validated, or <c>xs:untyped</c> otherwise.
        /// </para>
        /// <para>For any other kind of node, the value will be null.
        /// </para>
        /// <para>If the node has been validated against an anonymous type in the schema,
        /// the result will be a system-generated name, with the namespace URI
        /// <c>http://ns.saxonica.com/anonymous-type</c>.
        /// </para>
        /// </remarks>

        public QName TypeAnnotationName {
            get {
                SchemaType type = ((NodeInfo)value).getSchemaType();
                return type == null ? null : new QName(type.getStructuredQName());
            }
        }

        /// <summary>
        /// The typed value of the node, as an instance of <c>XdmValue</c>.
        /// </summary>
        /// <remarks>
        /// A <c>DynamicError</c> is thrown if the node has no typed value, as will be the case for
        /// an element with element-only content.
        /// </remarks>

        public XdmValue TypedValue => XdmValue.Wrap(((NodeInfo) value).atomize());

        /// <summary>
        /// Get a <see cref="Processor"/> suitable for use with this <see cref="XdmNode"/>.
        /// </summary>
        /// <remarks>
        /// <para>In most cases this will be the original <see cref="Processor"/>
        /// object used to create the <see cref="DocumentBuilder"/> that built the document that 
        /// contains this node. If that <see cref="Processor"/> is not available, it will be a 
        /// compatible <c>Processor</c>, one that shares the same underlying <see cref="Saxon.Hej.Configuration"/>, 
        /// and hence is initialized with the same configuration settings, schema components, license features,
        /// and so on.</para>
        /// <para><i>Note: the only case where the original <c>Processor</c> is not available is when
        /// the same <c>Configuration</c> is used with multiple APIs, for example mixing s9api
        /// and JAXP or XQJ in the same application.</i></para>
        /// </remarks>
        /// <returns>Returns a <c>Processor</c> suitable for performing further operations on this node, for example
        /// for creating a <see cref="Serializer"/> or an <see cref="XPathCompiler"/>.</returns>
        public Processor Processor {
            get {
                Configuration config = Implementation.getConfiguration();
                object originator = config.getProcessor();
                if (originator is Processor processor) {
                    return processor;
                }
                else {
                    return new Processor(new Saxon.Hej.s9api.Processor(config));
                }
            }
        }

        /// <summary>
        /// Unwraps the underlying DOM <c>XmlNode</c> object from this <c>XdmNode</c>.
        /// If the method does not wrap a DOM <c>XmlNode</c> then null is returned
        /// </summary>
        /// <returns>The underlying <c>XmlNode</c></returns>
        public XmlNode GetUnderlyingXmlNode() {
            NodeInfo localNode = (NodeInfo)value;
            if (localNode is VirtualNode virtualNode) {
                object realNode = virtualNode.getRealNode();
                if (realNode is XmlNode xmlNode) {
                    return xmlNode;
                }
            }
            if (value is Saxon.Impl.Dom.DomNodeWrapper wrapper) {
                return (XmlNode) wrapper.getRealNode();
            }

            return null;
        }
        
        /// <summary>
        /// Incorrectly-named synonym of GetUnderlyingXmlNode, retained temporarily for compatibility with 11.0 and 11.1
        /// </summary>
        /// <returns>The underlying <c>XmlNode</c> if there is one</returns>
    
        [Obsolete("This is an obsolete synonym of GetUnderlyingXmlNode")]
        public XmlNode getUnderlyingXmlNode() {
            return GetUnderlyingXmlNode();
        }
        
        /// <summary>
        /// Unwraps the underlying Linq <c>XNode</c> object from this <c>XdmNode</c>.
        /// If the method does not wrap a Linq <c>XNode</c> then null is returned
        /// </summary>
        /// <remarks>
        /// <para>The result will always be null if the supplied node is an attribute or namespace node,
        /// even if it is within a tree that wraps a Linq tree.
        /// </para>
        /// </remarks>
        /// <returns>The underlying <c>XNode</c> if there is one, or null otherwise</returns>
        public System.Xml.Linq.XNode GetUnderlyingXNode() {
            NodeInfo localNode = (NodeInfo)value;
            if (localNode is VirtualNode virtualNode) {
                object realNode = virtualNode.getRealNode();
                if (realNode is XNode xNode) {
                    return xNode;
                }
            }
#if EE            
            if (value is Saxon.Impl.XDoc.XNodeWrapper wrapper) {
                return (XNode) wrapper.getRealNode();
            }
#endif

            return null;
        }

        /// <summary>
        /// Get the parent of this node.
        /// </summary>
        /// <remarks>
        /// Returns either a document node, an element node, or null in the case where
        /// this node has no parent. 
        /// </remarks>

        public XdmNode Parent {
            get {
                NodeInfo parent = ((NodeInfo) value).getParent();
                return (parent == null ? null : (XdmNode) XdmValue.Wrap(parent));
            }
        }

        /// <summary>
        /// Get the root of the tree containing this node.
        /// </summary>
        /// <remarks>
        /// Returns the root of the tree containing this node (which might be this node itself).
        /// </remarks>

        public XdmNode Root {
            get {
                XdmNode parent = Parent;
                if (parent == null) {
                    return this;
                }
                else {
                    return parent.Root;
                }
            }
        }
        
        /// <summary>
        /// Get the outermost element of the tree containing this node.
        /// </summary>
        /// <remarks>
        /// <para>If this node is a document node, the property returns the first element child
        /// of the document node. Any subsequent element children are ignored. If there are no element
        /// children, the method returns null.</para>
        /// <para>In all other cases, the property returns the value of <c>ancestor-or-self::*[last()]</c>,
        /// that is the outermost element ancestor of the starting node if there is one, or null otherwise.</para>
        /// </remarks>

        public XdmNode OutermostElement => 
            NodeKind.Equals(XdmNodeKind.Document) 
                ? Select(Steps.Child("*")).First() 
                : Select(Steps.AncestorOrSelf("*")).Last();


        /// <summary>
        /// Get a the string value of a named attribute of this element. 
        /// </summary>
        /// <remarks>
        /// Returns null if this node is not an element, or if this element has no
        /// attribute with the specified name.
        /// </remarks>
        /// <param name="name">The name of the attribute whose value is required</param>
        public string GetAttributeValue(QName name) {
            return ((NodeInfo) value).getAttributeValue(name.Uri, name.LocalName);
        }
        
        /// <summary>
        /// Get a the string value of a named attribute (in no namespace) of this element. 
        /// </summary>
        /// <remarks>
        /// Returns null if this node is not an element, or if this element has no
        /// attribute with the specified name.
        /// </remarks>
        /// <param name="name">The name of the attribute whose value is required, interpreted as no-namespace name</param>
        public string GetAttributeValue(string name) {
            return ((NodeInfo) value).getAttributeValue("", name);
        }

        /// <summary>
        /// Get a IEnumerable of XdmNodes by applying a <c>Step</c> to this
        /// XdmNode value.
        /// </summary>
        /// <param name="step">the <c>Step</c> to be applied to this node</param>
        /// <returns>an IEnumerable of nodes obtained by applying the <c>Step</c> function to this node</returns>
        public IEnumerable<XdmNode> Select(Step<XdmNode, XdmNode> step) {
            return step.Invoke(this);
        }

        /// <summary>
        /// Get the nodes found on the child axis that satisfy a supplied <c>Predicate</c>.
        /// </summary>
        /// <param name="filter">the predicate to be applied</param>
        /// <returns> an <c>Iterable</c> containing those nodes found on the child axis that satisfy the supplied predicate.</returns>
        public IEnumerable<XdmNode> Children(Predicate<XdmNode> filter) {
            return Children().Where(filter.Invoke);
        }

        /// <summary>
        /// Get the element children of this node
        /// </summary>
        /// <returns> an <c>Iterable</c> containing all nodes on the child axis.</returns>
        public IEnumerable<XdmNode> Children() {
            return new SequenceEnumerable<XdmNode>(() => ((NodeInfo) value).iterateAxis(GetAxisNumber(XdmAxis.Child)));
        }

        /// <summary>
        /// Get the element children of this node having a specified local name, irrespective of the namespace
        /// </summary>
        /// <param name="localName">the local name of the child elements to be selected, or "*" to select all children that are element nodes</param>
        /// <returns> an <c>Iterable</c> containing the element children of this node that have the required local name.</returns>
        
        public IEnumerable<XdmNode> Children(string localName) {
            Step<XdmNode, XdmNode> localNameStep = Steps.Child(localName);
            return localNameStep.Invoke(this);
        }

        /// <summary>
        /// Get the element children having a specified namespace URI and local name
        /// </summary>
        /// <param name="uri">the namespace URI of the child elements to be selected: 
        /// supply a zero-length string to indicate the null namespace</param>
        /// <param name="localName">the local name of the child elements to be selected</param>
        /// <returns> an <c>Iterable</c> containing the element children of this node that have the required local name and namespace URI.</returns>
        
        public IEnumerable<XdmNode> Children(string uri, string localName) {
            return Steps.Child(uri, localName).Invoke(this);
        }

        /// <summary>
        /// Get an enumerable that supplies all the nodes on one of the XPath
        /// axes, starting with this node.
        /// </summary>
        /// <param name="axis">
        /// The axis to be navigated, for example <c>XdmAxis.Child</c> for the child axis.
        /// </param>
        /// <remarks>
        /// The nodes are returned in axis order: that is, document order for a forwards
        /// axis, reverse document order for a reverse axis.
        /// </remarks>
        public IEnumerable<XdmNode> EnumerableOverAxis(XdmAxis axis) {
            return new SequenceEnumerable<XdmNode>(() => ((NodeInfo) value).iterateAxis(GetAxisNumber(axis)));
        }


        /// <summary>
        /// Get an enumerator that supplies all the nodes on one of the XPath
        /// axes, starting with this node.
        /// </summary>
        /// <param name="axis">
        /// The axis to be navigated, for example <c>XdmAxis.Child</c> for the child axis.
        /// </param>
        /// <remarks>
        /// The nodes are returned in axis order: that is, document order for a forwards
        /// axis, reverse document order for a reverse axis.
        /// </remarks>
        public IEnumerator<XdmNode> EnumerateAxis(XdmAxis axis) {
            return new SequenceEnumerator<XdmNode>(((NodeInfo) value).iterateAxis(GetAxisNumber(axis)));
        }

        /// <summary>
        /// Get an enumerator that selects all the nodes on one of the XPath
        /// axes, provided they have a given name. The nodes selected are those of the principal
        /// node kind (elements for most axes, attributes for the attribute axis, namespace nodes
        /// for the namespace axis) whose name matches the name given in the second argument.
        /// </summary>
        /// <param name="axis">
        /// The axis to be navigated, for example <c>XdmAxis.Child</c> for the child axis.
        /// </param>
        /// <param name="nodeName">
        /// The name of the required nodes, for example <c>new QName("", "item")</c> to select
        /// nodes with local name "item", in no namespace.
        /// </param>
        /// <remarks>
        /// The nodes are returned in axis order: that is, document order for a forwards
        /// axis, reverse document order for a reverse axis.
        /// </remarks>
        public IEnumerator<XdmNode> EnumerateAxis(XdmAxis axis, QName nodeName) {
            int kind;
            switch (axis) {
                case XdmAxis.Attribute:
                    kind = Saxon.Hej.type.Type.ATTRIBUTE;
                    break;
                case XdmAxis.Namespace:
                    kind = Saxon.Hej.type.Type.NAMESPACE;
                    break;
                default:
                    kind = Saxon.Hej.type.Type.ELEMENT;
                    break;
            }

            NamePool pool = ((NodeInfo) value).getConfiguration().getNamePool();
            NameTest test = new NameTest(kind, nodeName.NamespaceUri, nodeName.LocalName, pool);
            return new SequenceEnumerator<XdmNode>(((NodeInfo) value).iterateAxis(GetAxisNumber(axis), test));
        }

        private static byte GetAxisNumber(XdmAxis axis) {
            switch (axis) {
                case XdmAxis.Ancestor: return AxisInfo.ANCESTOR;
                case XdmAxis.AncestorOrSelf: return AxisInfo.ANCESTOR_OR_SELF;
                case XdmAxis.Attribute: return AxisInfo.ATTRIBUTE;
                case XdmAxis.Child: return AxisInfo.CHILD;
                case XdmAxis.Descendant: return AxisInfo.DESCENDANT;
                case XdmAxis.DescendantOrSelf: return AxisInfo.DESCENDANT_OR_SELF;
                case XdmAxis.Following: return AxisInfo.FOLLOWING;
                case XdmAxis.FollowingSibling: return AxisInfo.FOLLOWING_SIBLING;
                case XdmAxis.Namespace: return AxisInfo.NAMESPACE;
                case XdmAxis.Parent: return AxisInfo.PARENT;
                case XdmAxis.Preceding: return AxisInfo.PRECEDING;
                case XdmAxis.PrecedingSibling: return AxisInfo.PRECEDING_SIBLING;
                case XdmAxis.Self: return AxisInfo.SELF;
            }

            return 0;
        }

        /// <summary>
        /// Get the base URI of the node.
        /// </summary>

        public Uri BaseUri {
            get {
                string baseUriStr = ((NodeInfo) value).getBaseURI();
                return baseUriStr is null or "" ? null : new Uri(baseUriStr);
            }
        }

        /// <summary>
        /// Get the document URI of the node.
        /// </summary>

        public Uri DocumentUri {
            get {
                string s = ((NodeInfo) value).getSystemId();
                return string.IsNullOrEmpty(s) ? null : new Uri(s);
            }
        }

        /// <summary>
        /// Send the node (that is, the subtree rooted at this node) to an <c>XmlWriter</c>
        /// </summary>
        /// <remarks>
        /// Note that an <c>XmlWriter</c> can only handle a well-formed XML document. This method
        /// will therefore signal an exception if the node is a document node with no children, or with
        /// more than one element child.
        /// </remarks>
        /// <param name="writer">
        /// The <c>XmlWriter</c> to which the node is to be written
        /// </param>
        public void WriteTo(XmlWriter writer) {
            NodeInfo node = ((NodeInfo) value);
            PipelineConfiguration pipe = node.getConfiguration().makePipelineConfiguration();
            XmlWriterReceiver receiver = new (writer, pipe);
            NamespaceDifferencer differencer = new (receiver, new Properties());
            differencer.open();
            node.copy(differencer, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Loc.NONE);
            differencer.close();
        }

        /// <summary>
        /// Return a serialization of this node as lexical XML
        /// </summary>
        /// <remarks>
        /// <para>In the case of an element node, the result will be a well-formed
        /// XML document serialized as defined in the W3C XSLT/XQuery serialization specification,
        /// using options <c>method="xml"</c>, <c>indent="yes"</c>, <c>omit-xml-declaration="yes"</c>.</para>
        /// <para>In the case of a document node, the result will be a well-formed
        /// XML document provided that the document node contains exactly one element child,
        /// and no text node children. In other cases it will be a well-formed external
        /// general parsed entity.</para>
        /// <para>In the case of an attribute node, the output is a string in the form
        /// <c>name="value"</c>. The name will use the original namespace prefix.</para>
        /// <para>Other nodes, such as text nodes, comments, and processing instructions, are
        /// represented as they would appear in lexical XML.</para>
        /// </remarks>

        public string OuterXml {
            get {
                NodeInfo node = ((NodeInfo) value);

                if (node.getNodeKind() == Type.ATTRIBUTE) {
                    string val = node.getStringValue().Replace("\"", "&quot;");
                    val = val.Replace("<", "&lt;");
                    val = val.Replace("&", "&amp;");
                    return node.getDisplayName() + "=\"" + val + '"';
                }
                else if (node.getNodeKind() == Type.NAMESPACE) {
                    string val = node.getStringValue().Replace("\"", "&quot;");
                    val = val.Replace("<", "&lt;");
                    val = val.Replace("&", "&amp;");
                    string name = node.getDisplayName();
                    name = (name.Equals("") ? "xmlns" : "xmlns:" + name);
                    return name + "=\"" + val + '"';
                }

                return Saxon.Hej.query.QueryResult.serialize(node).Trim();
            }
        }

        /// <summary>
        /// Two instances of <c>XdmNode</c> are equal if they represent the same node. That is, the <c>Equals()</c>
        /// method returns the same result as the XPath "is" operator.
        /// </summary>
        /// <param name="obj">The object node to be compared</param>
        public override bool Equals(object obj) {
            return obj is XdmNode node && ((NodeInfo) value).Equals((NodeInfo) node.value);
        }

        /// <summary>
        /// The hash code of a node reflects the equality relationship: if two <c>XdmNode</c> instances
        /// represent the same node, then they have the same hash code
        /// </summary>
        public override int GetHashCode() {
            return ((NodeInfo) value).GetHashCode();
        }

        /// <summary>
        /// Return a string representation of the node.
        /// </summary>
        /// <remarks>
        /// This method returns the value of the <see cref="OuterXml"/> property, that is, a serialization of the node.
        /// To get the string value of a node as defined in XPath, use the <see cref="XdmItem.StringValue"/> property.
        /// </remarks>
        public override string ToString() {
            return OuterXml;
        }

        /// <summary>
        /// Escape hatch to the underlying class in the Java implementation
        /// </summary>

		public Saxon.Hej.om.NodeInfo Implementation => ((NodeInfo) value);
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
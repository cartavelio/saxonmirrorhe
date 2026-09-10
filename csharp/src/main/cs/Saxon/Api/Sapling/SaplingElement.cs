using Saxon.Ejava.lang;

namespace Saxon.Api.Sapling
{
    /// <summary>
    /// An element node in the sapling tree.
    /// <para>The element has a name (a QName), an ordered list of children, a set of attributes,
    /// and a set of namespaces. Namespace bindings for the element name and attribute names are
    /// managed implicitly, but additional bindings may be explicitly added.</para>
    /// <para>Like all sapling nodes, a <code>SaplingElement</code> is immutable. All operations, such as adding children or attributes,
    /// deliver a new element node. A sapling node generally exists only transiently during tree
    /// construction; to make use of the constructed tree, it will usually be converted to a regular tree when
    /// construction is complete, using <see cref="ToXdmNode"/>.</para>
    /// </summary>
    public class SaplingElement: SaplingNode
    {
        /// <summary>
        /// Construct an empty element with the specified name in no namespace.
        /// </summary>
        /// <param name="name">The element name. This should be an NCName, but this constraint is not enforced
        /// during construction.</param>
        public SaplingElement(string name)
        {
            NodeKind = XdmNodeKind.Element;
            javaDelegate = new Saxon.Hej.sapling.SaplingElement(name);
        }

        /// <summary>
        /// Construct an empty element with the specified name.
        /// </summary>
        /// <param name="name">The element name.</param>
        public SaplingElement(QName name)
        {
            NodeKind = XdmNodeKind.Element;
            javaDelegate = new Saxon.Hej.sapling.SaplingElement(name);
        }

        private SaplingElement(Saxon.Hej.sapling.SaplingElement javaElement)
        {
            NodeKind = XdmNodeKind.Element;
            javaDelegate = javaElement;
        }

        /// <summary>
        /// Constructs an XdmNode from the sapling document.
        /// </summary>
        /// <param name="processor">The processor that is to own the resulting node.</param>
        /// <returns>The parentless element node constructed from this sapling element. The implementation model
        /// for the tree will be the default tree model of the processor's configuration.</returns>
        /// <exception cref="SaxonApiException">Throws <c>SaxonApiException</c> if construction fails.</exception>
        public XdmNode ToXdmNode(Processor processor)
        {
            return ((Saxon.Hej.sapling.SaplingDocument)javaDelegate).toXdmNode(processor);
        }

        /// <summary>
        /// Add a number of child nodes to the element node, returning a new element with additional children
        /// beyond those already present. Neither the original element node nor the children are modified.
        /// <para>Note: because adding a child always returns a new parent element, it is impossible
        /// to create cycles by adding the node to itself, directly or indirectly.</para>
        /// </summary>
        /// <param name="children">The nodes to add, in document order.</param>
        /// <returns>A new element node with the additional children.</returns>
        /// <exception cref="IllegalArgumentException">if any of the supplied nodes is a document node.</exception>
        public SaplingElement WithChild(params SaplingNode[] children)
        {
            var jchildren = new Saxon.Hej.sapling.SaplingNode[children.Length];
            for (int pos = 0; pos < children.Length; pos++)
            {
                jchildren[pos] = children[pos].javaDelegate;
            }

            var newElement = ((Saxon.Hej.sapling.SaplingElement)javaDelegate).withChild(jchildren);
            return new SaplingElement(newElement);
        }

        /// <summary>
        /// Add a text node to this element, returning a new sapling element node.
        /// <para>This is a convenience method. It is equivalent to
        /// <code>WithChild(Saplings.Text(text))</code>.
        /// </summary>
        /// <param name="text">The value of the text node.</param>
        /// <returns>A new element node with a new text node as an additional child.</returns>
        public SaplingElement WithText(string text)
        {
            return WithChild(new SaplingText(text));
        }

        /// <summary>
        /// Add a comment to this element, returning a new sapling element node.
        /// <para>This is a convenience method. It is equivalent to
        /// <code>WithChild(Saplings.Comment(comment))</code>.
        /// </summary>
        /// <param name="comment">The value of the comment.</param>
        /// <returns>A new element node with a new comment node as an additional child.</returns>
        public SaplingElement WithComment(string comment)
        {
            return WithChild(new SaplingComment(comment));
        }

        /// <summary>
        /// Add a processing instruction to this element, returning a new sapling element node.
        /// <para>This is a convenience method. It is equivalent to
        /// <code>WithChild(Saplings.ProcessingInstruction(target, content))</code>.
        /// </summary>
        /// <param name="target">The PI target.</param>
        /// <param name="content">The PI content.</para>
        /// <returns>A new element node with a new processing instruction node as an additional child.</returns>
        public SaplingElement WithProcessingInstruction(string target, string content)
        {
            return WithChild(new SaplingProcessingInstruction(target, content));
        }
        
        /// <summary>
        /// Add to, or replace, an attribute on an element node, returning a new element with the modified set of attributes.
        /// </summary>
        /// <param name="name">The attribute name in the form of an NCName. The attribute will be in no namespace.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>A new element node with the modified set of attributes.</returns>
        public SaplingElement WithAttribute(string name, string value)
        {
            return WithAttribute(new QName("", "", name), value);
        }
        
        /// <summary>
        /// Add to, or replace, an attribute on an element node, returning a new element with the modified set of attributes.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>A new element node with the modified set of attributes.</returns>
        public SaplingElement WithAttribute(QName name, string value)
        {
            var javaChild = ((Saxon.Hej.sapling.SaplingElement) javaDelegate).withAttr(name, value);
            return new SaplingElement(javaChild);
        }

        /// <summary>
        /// Add a namespace binding for a prefix/URI pair. Namespace bindings are added automatically for
        /// prefixes used in element and attribute names; they only need to be added explicitly if the binding
        /// is additional to those in element and attribute names. A namespace binding for the XML namespace
        /// is implicitly present on every element.
        /// </summary>
        /// <param name="prefix">the namespace prefix. This must either be a zero length string, or it must take the
        /// form of an NCName, but this constraint is not currently enforced.</param>
        /// <param name="uri">the namespace URI. If this is the empty string, then (a) if the prefix is empty, the
        /// namespace binding is ignored; (b) otherwise, an exception is raised (namespace undeclarations
        /// are not permitted).</param>
        /// <returns>A new element node with the modified set of namespace bindings.</returns>
        /// <exception cref="IllegalArgumentException">If the <c>uri</c> is empty and the <c>prefix</c> is not.</exception>
        /// <exception cref="IllegalStateException">If the element already has a namespace binding for the
        /// <c>prefix</c> with a different <c>uri</c>.</exception>
        public SaplingElement WithNamespace(string prefix, string uri)
        {
            var javaChild = ((Saxon.Hej.sapling.SaplingElement) javaDelegate).withNamespace(prefix, uri);
            return new SaplingElement(javaChild);
        }
    }
}
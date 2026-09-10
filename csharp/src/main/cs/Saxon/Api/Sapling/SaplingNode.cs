namespace Saxon.Api.Sapling
{
    /// <summary>
    /// his class represents a sapling node.
    /// <para>Sapling nodes represent transient
    /// nodes used in the course of tree construction; they are intended to have a
    /// very lightweight implementation, making construction as simple as possible,
    /// and they do not support any complex navigation or query interfaces.
    /// Typically when construction of a sapling tree is complete it will be
    /// converted to a regular <see cref="XdmNode"/> for
    /// further processing, or it can be sent to a destination such as a
    /// <see cref="Serializer"/> or <see cref="SchemaValidator"/>.</para>
    ///
    /// <para>The <see cref="SaplingDocument"/> node at the root of a constructed tree
    /// implements the JAXP <see cref="Source"/> interface which is widely accepted
    /// by a variety of interfaces in Saxon: this means that it can readily be used
    /// as input to a query, a transformation, a document builder, or a schema
    /// validator.</p> <p>Sapling nodes are immutable objects; operations such as
    /// <see cref="SaplingElement#withChild"/> create a new object and leave the
    /// original unchanged. This eliminates the need (found in other models such as
    /// the DOM) to make copies of nodes when attaching them to a tree.</para>
    ///
    /// <para>The data model supported by Sapling nodes is close to the XDM model,
    /// but with some limitations:</para>
    ///
    /// <list>
    ///     <item><description>All nodes are untyped (elements are of type xs:untyped, attributes are xs:untypedAtomic).</description></item>
    ///     <item><description>The ID, IDREF, and NILLED properties cannot be set.</description></item>
    ///     <item><description>Some constraints on the model (for example, rules on the lexical form of names) are not enforced
    ///     by the construction API.</description></item>
    ///     <item><description>Elements cannot have an independent base URI property, except to the extent that this can be
    ///     defined using <code>xml:base</code> attributes; only document nodes have their own base URI</description></item>
    ///     <item><description>Zero length text nodes and adjacent text node siblings are permitted (they will be merged or
    ///     eliminated when the sapling tree is converted to a regular node tree)</description></item>
    /// </list>
    ///
    /// <paraAttributes are not represented as nodes in their own right, but rather
    /// as properties of element nodes.</para>
    ///
    /// <para>The internal representation of document and element nodes include a list
    /// of the node's children; but there are no links back from a node to its
    /// parent. This enables a node to be attached as a child to many different
    /// parents. This does not create problems, because nodes are immutable, and
    /// there are no operations that depend on concepts of node identity or document
    /// order.</para>
    ///
    /// <para>As in XDM, an element node contains a set of in-scope namespaces. There
    /// is no requirement that the in-scope namespaces of an element should be in
    /// any way related to those of its child or parent elements. Operations that
    /// construct a regular tree from a sapling tree will introduce namespace
    /// inheritance, whereby namespaces defined on a parent element are inherited by
    /// its children. Namespace bindings for the (prefix, URI) pairs used in the
    /// names of elements and attributes are added to an element automatically;
    /// additional namespace bindings may be added by explicit API request.</para>
    /// </summary>
    public abstract class SaplingNode
    {
        /// <summary>
        /// The underlying Java implementation.
        /// <para>Most of the implementation details are handled by the delegated Java object.
        /// Ideally, this would be private or protected, but that's not practical.</para>
        /// </summary>
        public Saxon.Hej.sapling.SaplingNode javaDelegate;
        
        /// <summary>
        /// Identifies the kind of node.
        /// </summary>
        public XdmNodeKind NodeKind { get; protected set; }
    }
}
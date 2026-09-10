using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Type = Saxon.Hej.type.Type;

namespace Saxon.Api {
    /// <summary>
    /// Instances of <c>XdmNodeKind</c> represent the item types denoted in XPath as <c>document-node()</c>,
    /// <c>element()</c>, <c>attribute()</c>, <c>text()</c>, and so on. These are all represented by singular named instances.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class XdmNodeKind : XdmItemType {
        private readonly int kind;

        internal XdmNodeKind(NodeKindTest test) : base(test) {
            kind = test.getNodeKind();
        }

        /// <summary>
        /// The item type <c>document-node()</c>
        /// </summary>
        public static readonly XdmNodeKind Document = new(NodeKindTest.DOCUMENT);

        /// <summary>
        /// The item type <c>element()</c>
        /// </summary>
        public static readonly XdmNodeKind Element = new(NodeKindTest.ELEMENT);

        /// <summary>
        /// The item type <c>attribute()</c>
        /// </summary>
        public static readonly XdmNodeKind Attribute = new(NodeKindTest.ATTRIBUTE);

        /// <summary>
        /// The item type <c>text()</c>
        /// </summary>
        public static readonly XdmNodeKind Text = new(NodeKindTest.TEXT);

        /// <summary>
        /// The item type <c>comment()</c>
        /// </summary>
        public static readonly XdmNodeKind Comment = new(NodeKindTest.COMMENT);

        /// <summary>
        /// The item type <c>processing-instruction()</c>
        /// </summary>
        public static readonly XdmNodeKind ProcessingInstruction =
            new XdmNodeKind(NodeKindTest.PROCESSING_INSTRUCTION);

        /// <summary>
        /// The item type <c>namespace-node()</c>
        /// </summary>
        public static readonly XdmNodeKind Namespace = new(NodeKindTest.NAMESPACE);

        internal static XdmNodeKind ForNodeKindTest(NodeKindTest test) {
            int kind = test.getPrimitiveType();
            switch (kind) {
                case Type.DOCUMENT:
                    return Document;
                case Type.ELEMENT:
                    return Element;
                case Type.ATTRIBUTE:
                    return Attribute;
                case Type.TEXT:
                    return Text;
                case Type.COMMENT:
                    return Comment;
                case Type.PROCESSING_INSTRUCTION:
                    return ProcessingInstruction;
                case Type.NAMESPACE:
                    return Namespace;
                default:
                    throw new ArgumentException("Unknown node kind");
            }
        }

        /// <summary>
        /// Get the item type representing the node kind of a supplied node
        /// </summary>
        /// <param name="node">The node whose node kind is required</param>
        /// <returns>The relevant node kind</returns>
        public static XdmNodeKind ForNode(XdmNode node) {
            return node.NodeKind;
        }

        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">item the item to be tested against this item type</param>
        /// <returns>true if the item matches this item type, false if it does not match.</returns>
        public override bool Matches(XdmItem item) {
            return item.Unwrap() is NodeInfo && kind == ((Saxon.Hej.om.NodeInfo) item.Unwrap()).getNodeKind();
        }

        /// <summary>
        /// Ask whether this node kind is the same as another
        /// </summary>
        /// <param name="other">The other object</param>
        /// <returns>True if the argument is the same node kind.</returns>
        public override bool Equals(object other) {
            return other is XdmNodeKind nodeKind && kind == nodeKind.kind;
        }

        /// <summary>
        /// Get a hash code
        /// </summary>
        /// <returns>A suitable hash code</returns>
        public override int GetHashCode() {
            return kind;
        }

        /// <summary>
        ///  Determine whether this ItemType subsumes another ItemType. Specifically,
        /// <code>A.subsumes(B)</code> is true if every value that matches the ItemType B also matches
        /// the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this ItemType subsumes the other ItemType. This includes the case where A and B
        ///         represent the same ItemType.</returns>
        public override bool Subsumes(XdmItemType other) {
            Saxon.Hej.type.ItemType otherType = other.Unwrap();
            return otherType is NodeTest test && test.getUType() == underlyingType.getUType();
        }

        /// <summary>
        /// Get the item type corresponding to an <c>XmlNodeType</c> as defined in the <c>System.Xml</c> package
        /// </summary>
        /// <param name="type">The <c>XmlNodeType</c> to be converted</param>
        /// <returns>The corresponding <c>XdmNodeKind</c></returns>
        public static XdmNodeKind ForNodeType(XmlNodeType type) {
            return type switch {
                XmlNodeType.Document => Document,
                XmlNodeType.Element => Element,
                XmlNodeType.Attribute => Attribute,
                XmlNodeType.Text => Text,
                XmlNodeType.Comment => Comment,
                XmlNodeType.ProcessingInstruction => ProcessingInstruction,
                _ => throw new ArgumentException("Unknown node kind")
            };
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
using System;
using System.Net.NetworkInformation;
using Saxon.Hej.lib;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.s9api;
using Saxon.Hej.type;

namespace Saxon.Api {
    /// <summary>
    /// Abstract class representing an item type. This may be the generic item type <c>item()</c>,
    /// an atomic type, the generic node type <code>node()</code>, a specific node kind such as
    /// <c>element()</c> or <c>text()</c>, or the generic function type <code>function()</code>.
    /// </summary>
    /// <remarks>
    /// <para>More specific node types (such as <c>element(E)</c> or <c>schema-element(E)</c>) cannot currently
    /// be instantiated directly in this API.</para>
    /// <para>An <c>XdmItemType</c> may however be constructed using the method <c>Processor.ParseItemType()</c></para>
    /// </remarks>

    public abstract class XdmItemType {
        protected readonly Saxon.Hej.type.ItemType underlyingType;

        protected XdmItemType(Saxon.Hej.type.ItemType underlyingType) {
            this.underlyingType = underlyingType;
        }
        
        // Construct an XdmItemType wrapping an ItemType. Because no Configuration is available,
        // not all types can be constructed precisely, so in some cases the result is an approximation.
        // For a more accurate type, use the method that accepts the Configuration as a second argument,
        // or use Processor.ParseItemType.
        internal static XdmItemType MakeXdmItemType(Saxon.Hej.type.ItemType type) {
            switch (type) {
                case BuiltInAtomicType builtInAtomicType:
                    return XdmAtomicType.BuiltInAtomicType(
                        new QName(builtInAtomicType.getStructuredQName()));
                case AtomicType atomicType:
                    return new XdmAtomicType(atomicType);
                default: {
                    switch (type) {
                        case ErrorType:
                        case AnyNodeTest:
                            return XdmAnyNodeType.Instance; // TODO: need to represent this properly
                        case NodeTest test: {
                            int kind = test.getPrimitiveType();
                            return XdmNodeKind.ForNodeKindTest((NodeKindTest) NodeKindTest.makeNodeKindTest(kind));
                        }
                        case MapType:
                            return XdmAnyMapType.Instance;
                        case ArrayItemType:
                            return XdmAnyArrayType.Instance;
                        case AnyItemType:
                            return XdmAnyItemType.Instance;
                        case FunctionItemType:
                            return XdmAnyFunctionType.Instance;
                        default:
                            return null;
                    }
                }
            }
        }

        internal static XdmItemType MakeXdmItemType(Saxon.Hej.type.ItemType type, Saxon.Hej.Configuration config) {
            return type switch {
                BuiltInAtomicType builtInAtomicType => XdmAtomicType.BuiltInAtomicType(
                    new QName(builtInAtomicType.getStructuredQName())),
                AtomicType atomicType => new XdmAtomicType(atomicType),
                _ => new ConstructedItemType(type, config)
            };
        }

        internal ItemType Unwrap() {
            return underlyingType;
        }

        /// <summary>
        /// Determine whether this item type matches a given item.
        /// </summary>
        /// <param name="item">the item to be tested against this item type</param>
        /// <returns>true if the item matches this item type, false if it does not match.</returns>
        public abstract bool Matches(XdmItem item);

        /// <summary>
        /// Determine whether this ItemType subsumes another ItemType. Specifically,
        /// <code>A.subsumes(B)</code> is true if every value that matches the ItemType B also matches
        /// the ItemType A.
        /// </summary>
        /// <param name="other">the other ItemType</param>
        /// <returns>true if this ItemType subsumes the other ItemType. This includes the case where A and B
        ///         represent the same ItemType.</returns>
        public abstract bool Subsumes(XdmItemType other);

        /// <summary>
        /// Get a string representation of the type. This will be a string that confirms to
        /// XPath ItemType production, for example a QName (always in 'Q{uri}local' format, or a
        /// construct such as 'node()' or 'map(*)'). If the type is an anonymous schema type, the 
        /// name of the nearest named base type will be given, preceded by the character '&lt;'
        /// </summary>
        /// <returns>a string representation of the type</returns>
        public override string ToString() {
            return underlyingType.ToString();
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
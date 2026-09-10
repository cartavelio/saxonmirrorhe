using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.ma.arrays;
using Saxon.Hej.ma.map;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.s9api;
using Saxon.Hej.type;
using Saxon.Hej.value;

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
                            return XdmNodeKind.ForNodeKindTest((NodeKindTest)NodeKindTest.makeNodeKindTest(kind));
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

        /// <summary>
        /// Construct an <c>XdmSequenceType</c> from an <c>XdmItemType</c> and an occurrence indicator
        /// </summary>
        /// <param name="indicator">The occurrence indicator. The values '*', '?', and '!' have
        /// their usual meaning. A single space (' ') indicates "exactly one", and digit zero ('0')
        /// indicates "exactly zero". These values are also available as constants in class
        /// <c>XdmSequenceType.</param>
        /// <returns>An <c>XdmSequenceType</c> that combines this <c>XdmItemType</c> with
        /// the given occurrence indicator.</returns>

        public XdmSequenceType Occurs(char indicator) {
            return new XdmSequenceType(this, indicator);
        }

        /// <summary>
        /// Construct an <c>XdmSequenceType</c> that allows a sequence to contain exactly one item of this
        /// item type
        /// </summary>
        /// <returns>the result of <c>.Occurs(' ')</c></returns>

        public XdmSequenceType One => new XdmSequenceType(this, ' ');

        /// <summary>
        /// Construct an <c>XdmSequenceType</c> that allows a sequence to contain zero or one items of this
        /// item type
        /// </summary>
        /// <returns>the result of <c>.Occurs('?')</c></returns>

        public XdmSequenceType ZeroOrOne => new XdmSequenceType(this, '?');

        /// <summary>
        /// Construct an <c>XdmSequenceType</c> that allows a sequence to contain zero or more items of this
        /// item type
        /// </summary>
        /// <returns>the result of <c>.Occurs('*')</c></returns>

        public XdmSequenceType ZeroOrMore => new XdmSequenceType(this, '*');

        /// <summary>
        /// Construct an <c>XdmSequenceType</c> that allows a sequence to contain one or more items of this
        /// item type
        /// </summary>
        /// <returns>the result of <c>.Occurs('+')</c></returns>

        public XdmSequenceType OneOrMore => new XdmSequenceType(this, '+');

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
        /// Get a string representation of the type. This will be generally a string that conforms to
        /// XPath ItemType production, for example a QName for an atomic type, or a
        /// construct such as 'node()' or 'map(*)'). If the type is an anonymous schema type, the 
        /// name of the nearest named base type will be given, preceded by the character '&lt;'.
        /// QNames in the XML Schema namespace use the prefix "xs" (for example "xs:boolean");
        /// other QNames are expressed using EQName notation (<c>Q{uri}local</c>).
        /// </summary>
        /// <returns>a string representation of the type</returns>
        public override string ToString() {
            return underlyingType.ToString();
        }

        public override bool Equals(object other) {
            return other is XdmItemType otherType && underlyingType.Equals(otherType.underlyingType);
        }

        public override int GetHashCode() {
            return underlyingType.GetHashCode();
        }

        /// <summary>
        /// Get an <c>XdmItemType</c> representing the type of a supplied <c>XdmItem</c>.
        /// </summary>
        /// <remarks>
        /// <para>Note that the results are to some extent arbitrary, since an item may conform
        ///    to many different item types, and none of them is necessarily more specific
        ///    than all the others.
        /// </para>
        /// <para>If the supplied item is an atomic value, the returned ItemType will 
        ///    reflect the most specific atomic type of the item.
        /// </para>
        /// <para>For a map, the result is simply {@link net.sf.saxon.s9api.ItemType#ANY_MAP}. For an array, the result is
        ///    simply {@link net.sf.saxon.s9api.ItemType#ANY_ARRAY}. For any other function, it is an instance of
        ///    {@link XdmFunctionItem} that reflects the signature of the function.
        /// </para>
        /// <para>
        ///    Future versions of Saxon may return a more precise type.
        /// </para>
        /// </remarks>
        /// <param name="item">item the supplied item whose type is required</param>
        /// <param name="processor">the Saxon Processor. This is needed because in some cases,
        /// items are not tied to a particular Processor, but item types are.</para>
        /// <returns>a type that matches the supplied item</returns>

        public static XdmItemType ForItem(XdmItem item, Processor processor) {
            
            Configuration config = processor.Implementation;
            if (item.IsAtomic()) {
                AtomicValue value = (AtomicValue)item.Unwrap();
                AtomicType type = value.getItemType();
                return new ConstructedItemType(type, config);
            }

            if (item.IsNode()) {
                NodeInfo node = (NodeInfo)item.Unwrap();
                int kind = node.getNodeKind();
                if (node.getLocalPart() == "") {
                    return new ConstructedItemType(NodeKindTest.makeNodeKindTest(kind), config);
                }

                return new ConstructedItemType(new SameNameTest(node), config);
            }

            Saxon.Hej.om.Item it = (Saxon.Hej.om.Item)item.Unwrap();
            if (it is MapItem) {
                return XdmAnyMapType.Instance;
            }

            if (it is ArrayItem) {
                return XdmAnyArrayType.Instance;
            }

            Function functionItem = (Function)it;
            FunctionItemType fit = functionItem.getFunctionItemType();
            Saxon.Hej.value.SequenceType basicReturnType = fit.getResultType();
            Saxon.Hej.value.SequenceType[] basicArgTypes = fit.getArgumentTypes();
            return new ConstructedItemType(
                new SpecificFunctionType(basicArgTypes, basicReturnType),
                config);


        }
    }
}

// Copyright (c) 2021 Saxonica Limited
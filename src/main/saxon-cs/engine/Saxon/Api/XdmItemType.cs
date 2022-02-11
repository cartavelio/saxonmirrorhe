using System;
using System.Net.NetworkInformation;
using net.sf.saxon.lib;
using net.sf.saxon.ma.arrays;
using net.sf.saxon.ma.map;
using net.sf.saxon.om;
using net.sf.saxon.pattern;
using net.sf.saxon.type;

namespace Saxon.Api {
    /// <summary>
    /// Abstract class representing an item type. This may be the generic item type <c>item()</c>,
    /// an atomic type, the generic node type <code>node()</code>, a specific node kind such as
    /// <c>element()</c> or <c>text()</c>, or the generic function type <code>function()</code>.
    /// </summary>
    /// <remarks>
    /// More specific node types (such as <c>element(E)</c> or <c>schema-element(E)</c>) cannot currently
    /// be instantiated in this API.
    /// </remarks>
    public abstract class XdmItemType {
        protected net.sf.saxon.type.ItemType type;

        private ConversionRules defaultConversionRules = new ConversionRules();

        internal static XdmItemType MakeXdmItemType(net.sf.saxon.type.ItemType type) {
            if (type is AtomicType) {
                return XdmAtomicType.BuiltInAtomicType(
                        QName.FromStructuredQName(((AtomicType) type).getStructuredQName()));
            }
            else if (type is ErrorType) {
                return XdmAnyNodeType.Instance; // TODO: need to represent this properly
            }
            else if (type is NodeTest) {
                if (type is AnyNodeTest) {
                    return XdmAnyNodeType.Instance;
                }
                else {
                    int kind = ((NodeTest) type).getPrimitiveType();
                    return XdmNodeKind.ForNodeKindTest((NodeKindTest) NodeKindTest.makeNodeKindTest(kind));
                }
            }
            else if (type is MapType) {
                return XdmAnyMapType.Instance;
            }
            else if (type is ArrayItemType) {
                return XdmAnyArrayType.Instance;
            }
            else if (type is AnyItemType) {
                return XdmAnyItemType.Instance;
            }
            else if (type is FunctionItemType) {
                return XdmAnyFunctionType.Instance;
            }
            else {
                return null;
            }
        }

        internal ItemType Unwrap() {
            return type;
        }

        internal void SetJXdmItemType(ItemType type) {
            this.type = type;
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
        virtual public bool Subsumes(XdmItemType other) {
            return type.subsumes(other.type);
        }


        /// <summary>
        /// Get a string representation of the type. This will be a string that confirms to
        /// XPath ItemType production, for example a QName (always in 'Q{uri}local' format, or a
        /// construct such as 'node()' or 'map(*)'). If the type is an anonymous schema type, the 
        /// name of the nearest named base type will be given, preceded by the character '&lt;'
        /// </summary>
        /// <returns>a string representation of the type</returns>
        public override String ToString() {
            return type.ToString();
        }
    }
}
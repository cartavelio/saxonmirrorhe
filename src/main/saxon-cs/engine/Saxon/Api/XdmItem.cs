using System;
using net.sf.saxon.om;

namespace Saxon.Api {
    /// <summary inherits="XdmValue">
    /// The class <c>XdmItem</c> represents an item in a sequence, as defined
    /// by the XDM data model. An item may be an atomic value, a node, a function (including maps
    /// and arrays), or an external object.
    /// </summary>
    /// <remarks>
    /// <para>An item is a member of a sequence, but it can also be considered as
    /// a sequence (of length one) in its own right. <c>XdmItem</c> is a subtype
    /// of <c>XdmValue</c> because every item in the XDM data model is also a
    /// value.</para>
    /// <para>It cannot be assumed that every sequence of length one will always be 
    /// represented by an <c>XdmItem</c>. It is quite possible for an <c>XdmValue</c>
    /// that is not an <c>XdmItem</c> to hold a singleton sequence. Use <see cref="XdmValue.Simplify"/> 
    /// to ensure that a singleton is represented as an <c>XdmItem</c>.</para>
    /// </remarks> 

    [Serializable]
    public abstract class XdmItem : XdmValue {
        
        protected XdmItem() {}
        protected XdmItem(Item item) : base(item) {}

        /// <summary>
        /// Determine whether the item is an atomic value.
        /// </summary>
        /// <returns>
        /// True if the item is an atomic value, false if it is a node, function, or external object.
        /// </returns>

        public abstract bool IsAtomic();

        /// <summary>
        /// Determine whether the item is a node or some other type of item.
        /// </summary>
        /// <returns>True if the item is a node, false if it is an atomic value or a function (including maps and arrays).</returns>
        public bool IsNode()
        {
            return value is NodeInfo;
        }

        /// <summary>
        /// Get the string value of the item. 
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>For an atomic value, the result is the same as casting the value to a string.</item>
        /// <item>For a node, the method returns the string
        /// value of the node. This is not the same as the result of <see cref="XdmNode.ToString()"/>, which
        /// returns the result of serializing the node.</item>
        /// <item>For a function, including a map or array, the result is an error.</item>
        /// <item>For an external object, the result is the same as the result of calling <c>ToString()</c>
        /// on the external object.</item>
        /// </list>
        /// <para>In all cases the result is the same as applying the XPath <c>string()</c> function.</para>
        /// </remarks>
        /// <returns>The result of converting the item to a string.</returns>

        public String GetStringValue()
        {
            return ((Item)value).getStringValue();
        }



        /// <summary>
        /// Determine whether this item matches a given item type.
        /// </summary>
        /// <param name="typei">The item type to be tested against this item</param>
        /// <returns>True if the item matches this item type, false if it does not match.</returns>
        public bool Matches(XdmItemType typei)
        {
            return typei.Matches(this);
        }
    }
}
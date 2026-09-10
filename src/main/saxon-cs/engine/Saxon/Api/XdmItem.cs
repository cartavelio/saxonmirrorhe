using System;
using Saxon.Hej.om;

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
    /// that is not an <c>XdmItem</c> to hold a singleton sequence. Use <see cref="XdmValue.Simplify()"/> 
    /// to ensure that a singleton is represented as an <c>XdmItem</c>.</para>
    /// </remarks> 

    [Serializable]
    public abstract class XdmItem : XdmValue {
        
        protected XdmItem() {}
        protected XdmItem(Item item) : base(item) {}

        /// <summary>
        /// Ask whether the item is an atomic value.
        /// </summary>
        /// <returns>
        /// True if the item is an atomic value, false if it is a node, function, or external object.
        /// </returns>

        public abstract bool IsAtomic();

        /// <summary>
        /// Ask whether the item is a node or some other type of item.
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
        public string StringValue => ((Item) value).getStringValue();


        /// <summary>
        /// Ask whether this item matches a given item type.
        /// </summary>
        /// <param name="typei">The item type to be tested against this item</param>
        /// <returns>True if the item matches this item type, false if it does not match.</returns>
        public bool Matches(XdmItemType typei)
        {
            return typei.Matches(this);
        }
        
        /// <summary>
        /// Create an <c>XdmItem</c> from an underlying Saxon <c>Saxon.Hej.om.Item</c> object.
        /// The result is the same as applying the <c>XdmValue.Wrap</c> method, but
        /// it is returned more conveniently as an instance of <c>XdmItem</c>
        /// </summary>
        /// <param name="value">An object representing an XDM value in the
        /// underlying Saxon implementation. Must not be null.</param>
        /// <returns>An <c>XdmItem</c> that wraps the underlying Saxon XDM item
        /// representation.</returns>
        public static XdmItem WrapItem(Saxon.Hej.om.Item value) {
            return (XdmItem) Wrap(value);
        }
        
        /// <summary>
        /// Create an <c>XdmNode</c> from an underlying Saxon <c>Saxon.Hej.om.NodeInfo</c> object.
        /// The result is the same as applying the <c>XdmValue.Wrap</c> method, but
        /// it is returned more conveniently as an instance of <c>XdmNode</c>
        /// </summary>
        /// <param name="value">An object representing an XDM node in the
        /// underlying Saxon implementation. Must not be null.</param>
        /// <returns>An <c>XdmNode</c> that wraps the underlying Saxon XDM node
        /// representation.</returns>
        /// 
        public static XdmNode WrapItem(Saxon.Hej.om.NodeInfo value) {
            return (XdmNode) Wrap(value);
        }
        
        /// <summary>
        /// Create an <c>XdmAtomicValue</c> from an underlying Saxon <c>Saxon.Hej.value.AtomicValue</c> object.
        /// The result is the same as applying the <c>XdmValue.Wrap</c> method, but
        /// it is returned more conveniently as an instance of <c>XdmAtomicValue</c>
        /// </summary>
        /// <param name="value">An object representing an XDM atomic value in the
        /// underlying Saxon implementation. Must not be null.</param>
        /// <returns>An <c>XdmAtomicValue</c> that wraps the underlying Saxon atomic value
        /// representation.</returns>
        /// 
        public static XdmAtomicValue WrapItem(Saxon.Hej.value.AtomicValue value) {
            return (XdmAtomicValue) Wrap(value);
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited
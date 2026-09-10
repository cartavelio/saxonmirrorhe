using System;

namespace Saxon.Api {
    /// <summary inherits="XdmItem">
    /// The class <c>XdmExternalObjectValue</c> represents an XDM item that wraps an external .NET object.
    /// As such, it is outside the scope of the W3C XDM specification (but permitted as an extension).
    /// </summary>
    [Serializable]
    public class XdmExternalObjectValue : XdmItem
    {

        /// <summary>
        /// Constructor to create an <c>XdmExternalObjectValue</c> that wraps a supplied .NET object.
        /// </summary>
        /// <param name="o">The supplied .NET object.</param>
        public XdmExternalObjectValue(object o)
        {
            value = new JDotNetObjectValue(o);
        }

        /// <summary>
        /// Determine whether the item is an atomic value.
        /// </summary>
        /// <returns>
        /// False (the item is not an atomic value).
        /// </returns>
        public override bool IsAtomic()
        {
            return false;
        }



        /// <summary>
        /// Compare two external objects for equality. Two instances of <c>XdmExternalObjectValue</c> are equal
        /// if the .NET objects that they wrap are equal.
        /// </summary>
        /// <param name="other">The object to be compared</param>
        /// <returns>True if the other object is an <c>XdmExternalObjectValue</c> and the two wrapped objects are 
        /// equal under the equals method.</returns>
        public bool Equals(XdmExternalObjectValue other)
        {
            return other is XdmExternalObjectValue && value == other.value;
        }

        /// <summary>
        /// Return a hash code for the object. This respects the semantics of <c>equals(Object)</c>.
        /// </summary>
        /// <returns>A suitable hash code.</returns>
        public override int GetHashCode()
        {
            return ((java.lang.Object)value).hashCode();
        }

        /// <summary>
        /// Get the wrapped .NET object.
        /// </summary>
        /// <returns>The wrapped object.</returns>
        public object GetExternalObject()
        {
            return ((JObjectValue)value).getObject();
        }

        /// <summary>
        /// Get the result of converting the external value to a string.
        /// </summary>
        /// <returns>The result of applying <c>ToString()</c> to the wrapped external object.</returns>
        public override string ToString()
        {
            return GetExternalObject().ToString();
        }
    }
}
using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XdmItem</c> that wraps an external .NET object. 
    /// </summary>
    /// <remarks>
    /// <para>External objects are a Saxon extension, and
    /// represent a fourth kind of item, after nodes, atomic values, and functions. They are primarily useful
    /// in conjunction with extension functions: an external object may be returned from one extension function,
    /// and passed as an argument to another. They can also be supplied as parameters to a stylesheet or query,
    /// which in turn might pass them to an extension function.</para>
    /// <para>
    /// In the first release of SaxonCS (11.0), this type was generic, and was parameterized by the type of the
    /// wrapped object. This caused problems when attempting to reconstruct an <c>XdmExternalObject</c> from its
    /// internal representation (see bug 5151); in consequence the type parameter was dropped in 11.1. 
    /// </para>
    /// </remarks>
    public class XdmExternalObject : XdmItem {
        
        /// <summary>
        /// Wrap an external object as an XdmItem
        /// </summary>
        /// <param name="value">The object to be wrapped</param>
        public XdmExternalObject(object value) : base(new ObjectValue<object>(value)) {
            
        }
        /// <summary>
        /// Ask whether the item is an atomic value.
        /// </summary>
        /// <returns>
        /// False: the item is not an atomic value.
        /// </returns>
        public override bool IsAtomic() {
            return false;
        }
        
        /// <summary>
        /// Get the wrapped external object
        /// </summary>
        /// <returns>The wrapped external object</returns>
        public object GetObject() {
            return ((ObjectValue<object>)Unwrap()).getObject();
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
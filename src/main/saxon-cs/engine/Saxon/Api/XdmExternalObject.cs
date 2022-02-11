using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XdmItem</c> that wraps an external .NET object. 
    /// </summary>
    /// <remarks>
    /// External objects are a Saxon extension, and
    /// represent a fourth kind of item, after nodes, atomic values, and functions. They are primarily useful
    /// in conjunction with extension functions: an external object may be returned from one extension function,
    /// and passed as an argument to another. They can also be supplied as parameters to a stylesheet or query,
    /// which in turn might pass them to an extension function.
    /// </remarks>
    /// <typeparam name="T">The type of the wrapped object</typeparam>
    public class XdmExternalObject<T> : XdmItem {
        
        /// <summary>
        /// Wrap an external object as an XdmItem
        /// </summary>
        /// <param name="value">The object to be wrapped</param>
        public XdmExternalObject(T value) : base(new ObjectValue<T>(value)) {
            
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
        public T GetObject() {
            return ((ObjectValue<T>)Unwrap()).getObject();
        }
    }
}

// Copyright (c) 2021 Saxonica Limited
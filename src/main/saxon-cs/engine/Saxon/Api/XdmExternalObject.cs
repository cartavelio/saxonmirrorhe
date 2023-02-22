using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XdmItem</c> that wraps an external .NET object
    /// </summary>
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
using System;
using System.Collections;
using System.Collections.Generic;

namespace Saxon.Api {
    /// <summary>
    /// An implementation of <code>IEnumerator</code> that iterates over an empty sequence.
    /// </summary>

    public class EmptyEnumerator<T> : IEnumerator<T>
        where T : XdmItem
    {

        /// <summary>
        /// Create an instance of the enumerator with the <c>XdmItem</c> as the generic type
        /// </summary>
        public static EmptyEnumerator<XdmItem> INSTANCE = new EmptyEnumerator<XdmItem>();

        /// <summary>
        /// Create an instance of the enumerator with the <c>XdmNode</c> as the generic type
        /// </summary>
        public static EmptyEnumerator<XdmNode> NODE_INSTANCE = new EmptyEnumerator<XdmNode>();

        private EmptyEnumerator() { }

        /// <summary>
        /// Reset the enumerator
        /// </summary>
        public void Reset() { }




        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The current item in the enumerator
        /// </summary>
        T IEnumerator<T>.Current
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Move to the next item in the enumerator..
        /// </summary>
        /// <returns>true if successful move, false otherwise.</returns>
        public bool MoveNext()
        {
            return false;
        }

        /// <summary>
        /// The Dispose method is not implemented on this Enumerator
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
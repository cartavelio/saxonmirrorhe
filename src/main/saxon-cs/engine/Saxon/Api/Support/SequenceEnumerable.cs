using System;
using System.Collections;
using System.Collections.Generic;
using net.sf.saxon.om;

namespace Saxon.Api.Support {
    /// <summary>
    /// This class is an implementation of <c>IEnumerable</c> that wraps
    /// a (Java) <c>SequenceIterator</c>.
    /// </summary>
    /// <remarks>
    /// Because the underlying value can be evaluated lazily, it is possible
    /// for exceptions to occur as the sequence is being read.
    /// </remarks>

    [Serializable]
    internal class SequenceEnumerable<T> : IEnumerable<T>
        where T : XdmItem
    {

        private SequenceIterator iterator;

        internal SequenceEnumerable(SequenceIterator iterator)
        {
            this.iterator = iterator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)new SequenceEnumerator<XdmNode>(iterator);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)new SequenceEnumerator<XdmNode>(iterator);
        }
    }
}
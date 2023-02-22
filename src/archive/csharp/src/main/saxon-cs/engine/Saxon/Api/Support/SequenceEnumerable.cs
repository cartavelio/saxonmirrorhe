using System;
using System.Collections;
using System.Collections.Generic;
using Saxon.Hej.om;

namespace Saxon.Api.Support {
    /// <summary>
    /// This class is an implementation of <c>IEnumerable</c> that wraps
    /// a supplier of a (Java) <c>SequenceIterator</c>.
    /// </summary>
    /// <remarks>
    /// Because the underlying value can be evaluated lazily, it is possible
    /// for exceptions to occur as the sequence is being read.
    /// </remarks>

    [Serializable]
    internal class SequenceEnumerable<T> : IEnumerable<T>
        where T : XdmItem
    {

        private Func<SequenceIterator> iteratorFactory;

        internal SequenceEnumerable(Func<SequenceIterator> iteratorFactory)
        {
            this.iteratorFactory = iteratorFactory;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)new SequenceEnumerator<XdmNode>(iteratorFactory.Invoke());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<T>)new SequenceEnumerator<XdmNode>(iteratorFactory.Invoke());
        }
    }
}

// Copyright (c) 2021 Saxonica Limited